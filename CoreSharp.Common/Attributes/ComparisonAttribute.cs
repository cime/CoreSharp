namespace CoreSharp.Common.Attributes
{
    public abstract class ComparisonAttribute : ValidationAttribute
    {
        protected ComparisonAttribute()
        {  
        }

        protected ComparisonAttribute(object value)
        {
            CompareToValue = value;
        }

        public object CompareToValue { get; set; }

        public string ComparsionProperty { get; set; }
    }
}
