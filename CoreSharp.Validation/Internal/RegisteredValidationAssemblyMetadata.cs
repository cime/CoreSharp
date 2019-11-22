using System;
using System.Collections.Generic;
using System.Reflection;

namespace CoreSharp.Validation.Internal
{
    internal class RegisteredValidationAssemblyMetadata
    {
        public RegisteredValidationAssemblyMetadata(
            Assembly assembly,
            List<Type> genericDomainValidatorTypes
            )
        {
            Assembly = assembly;
            GenericDomainValidatorTypes = genericDomainValidatorTypes.AsReadOnly();
        }

        public Assembly Assembly { get; }

        public IReadOnlyList<Type> GenericDomainValidatorTypes { get; }
    }
}
