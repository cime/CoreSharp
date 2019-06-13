using System;
using System.Collections.Generic;
using CoreSharp.Common.Events;

namespace CoreSharp.Breeze.Events
{
    public class BreezeAfterSaveAsyncEvent : IAsyncEvent
    {
        public BreezeAfterSaveAsyncEvent(Dictionary<Type, List<EntityInfo>> saveMap, List<KeyMapping> keyMappings)
        {
            SaveMap = saveMap;
            KeyMappings = keyMappings;
        }

        public Dictionary<Type, List<EntityInfo>> SaveMap { get; }

        public List<KeyMapping> KeyMappings { get; }
    }
}
