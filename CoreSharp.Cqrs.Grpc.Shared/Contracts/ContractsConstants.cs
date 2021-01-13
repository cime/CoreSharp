using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CoreSharp.Cqrs.Grpc.Contracts
{
    public static class ContractsConstants
    {
        public static ReadOnlyCollection<Type> BuildInTypes = new List<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(string)
        }.AsReadOnly();

    }
}
