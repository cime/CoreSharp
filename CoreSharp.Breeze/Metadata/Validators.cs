using System.Collections.Generic;
using System.Linq;

namespace CoreSharp.Breeze.Metadata
{
    public class Validators : MetadataList<Validator>
    {
        public Validators()
        {
        }

        public Validators(List<Dictionary<string, object>> listOfDict) : base(listOfDict)
        {
        }

        protected override Validator Convert(Dictionary<string, object> item)
        {
            return new Validator(item);
        }

        public void Remove(string validatorName)
        {
            var val = this.FirstOrDefault(o => o.Name == validatorName);
            if(val == null) return;
            Remove(val);
        }
    }
}
