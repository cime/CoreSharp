using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.NHibernate.Profiler.Contracts;
using Newtonsoft.Json;

namespace CoreSharp.NHibernate.Profiler
{
    internal class NHibernateTcpServer : IDisposable
    {
        private static readonly object _staticLock = new object();
        private readonly object _lock = new object();
        private readonly Thread _workerThread;
        private bool _doWork = true;
        private TcpListener _listener;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private static NHibernateTcpServer _instance;

        public static NHibernateTcpServer GetInstance()
        {
            if (_instance == null)
            {
                lock (_staticLock)
                {
                    if (_instance == null)
                    {
                        _instance = new NHibernateTcpServer();
                    }
                }
            }

            return _instance;
        }

        private readonly ConcurrentQueue<Message> _messageQueue = new ConcurrentQueue<Message>();
        private readonly List<StreamWriter> _clients = new List<StreamWriter>();

        public NHibernateTcpServer()
        {
            _listener = new TcpListener(IPAddress.Any, 9978);
            _listener.Start();

            _cancellationTokenSource.Token.Register(() =>
            {
                _doWork = false;
                _listener.Server.Dispose();
                _listener.Stop();

                try
                {
                    foreach (var stream in _clients)
                    {
                        stream.Dispose();
                    }
                }
                catch (Exception ex)
                {
                }
            });

            Task.Run(async() =>
            {
                while (_doWork)
                {
                    var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);

                    lock (_lock)
                    {
                        _clients.Add(new StreamWriter(client.GetStream()) { AutoFlush = true });
                    }
                }
            }, _cancellationTokenSource.Token);

            _workerThread = new Thread(new ThreadStart(DoWork))
            {
                IsBackground = true,
                Name = "Profiling data server",
                Priority = ThreadPriority.BelowNormal
            };
            _workerThread.Start();
        }

        private void DoWork()
        {
            var statisticSource = new StatisticsSourceAggregator();
            var lastSentStatistic = DateTime.Now;

            while (_doWork)
            {
                while (!_messageQueue.IsEmpty)
                {
                    if (_messageQueue.TryDequeue(out var message))
                    {
                        Write(message);
                    }

                    if ((DateTime.Now - lastSentStatistic).TotalMilliseconds > 1000)
                    {
                        var stats = statisticSource.GetStatistics();

                        if (stats.Any())
                        {
                            AddMessageToQueue(new Message()
                            {
                                Type = "Statistic",
                                Content = JsonConvert.SerializeObject(stats)
                            });
                        }

                        lastSentStatistic = DateTime.Now;
                    }
                }
            }
        }

        public void AddMessageToQueue(Message message)
        {
            _messageQueue.Enqueue(message);
        }

        private void Write(Message message)
        {
            try
            {
                var msg = JsonConvert.SerializeObject(message, Formatting.None);

                lock (_lock)
                {
                    foreach (var stream in _clients)
                    {
                        try
                        {
                            stream.WriteLine(msg);
                        }
                        catch (Exception)
                        {
                            stream.Dispose();

                            lock (_lock)
                            {
                                _clients.Remove(stream);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public void Dispose()
        {
            _doWork = false;
            _workerThread.Join();

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
