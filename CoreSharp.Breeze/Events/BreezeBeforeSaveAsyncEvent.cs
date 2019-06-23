using System.Collections.Generic;
using CoreSharp.Cqrs.Events;

namespace CoreSharp.Breeze.Events
{
    public class BreezeBeforeSaveAsyncEvent : IAsyncEvent
    {
        public BreezeBeforeSaveAsyncEvent(List<EntityInfo> entitiesToPersist)
        {
            EntitiesToPersist = entitiesToPersist;
        }

        public List<EntityInfo> EntitiesToPersist { get; }
    }
}
