namespace CoreSharp.Validation
{
    public class ValidationErrorResponse
    {
        public enum ErrorSeverity
        {
            Error = 0,
            Warning = 1,
            Info = 2
        }

        public class ValidationError
        {
            public string ErrorName { get; set; }

            public string EntityTypeName { get; set; }

            public object[] KeyValues { get; set; }

            public string PropertyName { get; set; }

            public string ErrorMessage { get; set; }

            public object AttemptedValue { get; set; }

            public string ErrorCode { get; set; }

            public ErrorSeverity Severity { get; set; }
        }

        public string ErrorMessage { get; set; }

        public ValidationError[] Errors { get; set; }
    }
}
