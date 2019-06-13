using System.Collections.Generic;
using CoreSharp.Common.Events;

namespace CoreSharp.Breeze.Events
{
    public class BreezeAfterFlushAsyncEvent : IAsyncEvent
    {
        public BreezeAfterFlushAsyncEvent(List<EntityInfo> entitiesToPersist)
        {
            EntitiesToPersist = entitiesToPersist;
        }

        public List<EntityInfo> EntitiesToPersist { get; }
    }
}
