using System.Collections.Generic;
using CoreSharp.Cqrs.Events;

namespace CoreSharp.Breeze.Events
{
    public class BreezeBeforeFlushAsyncEvent : IAsyncEvent
    {
        public BreezeBeforeFlushAsyncEvent(List<EntityInfo> entitiesToPersist)
        {
            EntitiesToPersist = entitiesToPersist;
        }

        public List<EntityInfo> EntitiesToPersist { get; }
    }
}
