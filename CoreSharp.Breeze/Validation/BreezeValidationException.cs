using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace CoreSharp.Breeze.Validation
{
    [Serializable]
    public class BreezeValidationException : Exception
    {
        public Dictionary<EntityInfo, ValidationResult> ValidationResults { get; }

        public BreezeValidationException(Dictionary<EntityInfo, ValidationResult> validationResults)
        {
            ValidationResults = validationResults;
        }
    }
}
