using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSharp.Breeze.Metadata
{
    public class StructuralTypes : MetadataList<StructuralType>
    {
        public StructuralTypes()
        {
        }

        public StructuralTypes(List<Dictionary<string, object>> listOfDict) : base(listOfDict)
        {
        }

        public StructuralType GetStructuralType(Type itemType)
        {
            return this.FirstOrDefault(o => o.TypeFullName == itemType.FullName);
        }

        protected override StructuralType Convert(Dictionary<string, object> item)
        {
            return StructuralType.CreateInstance(item);
        }
    }
}
