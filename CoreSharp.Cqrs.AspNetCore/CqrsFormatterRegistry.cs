using System;
using System.Collections.Generic;

namespace CoreSharp.Cqrs.AspNetCore
{
    public class CqrsFormatterRegistry
    {
        private readonly Dictionary<string, ICqrsFormatter> _formatters = new Dictionary<string, ICqrsFormatter>();
        private bool _isLocked = false;

        public void Register(string name, ICqrsFormatter formatter)
        {
            if (_isLocked)
            {
                throw new AccessViolationException($"CqrsFormatterRegistry is locked");
            }

            name = name ?? string.Empty;

            _formatters[name] = formatter;
        }

        public ICqrsFormatter GetFormatter(string name)
        {
            _isLocked = true;

            name = name ?? string.Empty;

            return _formatters[name];
        }
    }
}
