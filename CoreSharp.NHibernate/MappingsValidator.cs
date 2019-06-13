using System;
using System.Linq;
using NHibernate;
using SimpleInjector;
using Environment = System.Environment;

namespace CoreSharp.NHibernate
{
    public class MappingsValidatorOptions
    {
        // TODO: make configurable
        private const byte MAX_TABLE_NAME_LENGTH = 50;
        private const byte MAX_PROPERTY_NAME_LENGTH = 50;

        public byte MaxTableNameLength { get; set; } = MAX_TABLE_NAME_LENGTH;
        public byte MaxPropertyNameLength { get; set; } = MAX_PROPERTY_NAME_LENGTH;
        public bool AllowMultipleMappingsToSameTable { get; set; }
    }

    public interface IMappingsValidator
    {
        void ValidateMappingNames(Container container, MappingsValidatorOptions options);
    }

    public class MappingsValidator : IMappingsValidator
    {
        public virtual void ValidateMappingNames(Container container, MappingsValidatorOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var configuration = container.GetInstance<global::NHibernate.Cfg.Configuration>();

            if (!options.AllowMultipleMappingsToSameTable)
            {
                var duplicatedTables = configuration.ClassMappings.Where(o => o.Table != null)
            .GroupBy(o => o.Table.Name)
            .Where(o => o.Count() > 1)
            .ToList();
                if (duplicatedTables.Any())
                {
                    throw new MappingException($"There are multiple types mapped to the same table:{Environment.NewLine}" +
                                               $"{string.Join(Environment.NewLine, duplicatedTables.Select(o => $"Table: {o.Key}, Mapped classes: {string.Join(",", o.Select(c => c.MappedClass.FullName))}"))}");
                }
            }

            var nameTooLongTables = configuration.ClassMappings.Where(o => o.Table != null)
                .Where(o => o.Table.Name.Length > options.MaxTableNameLength)
                .ToList();
            if (nameTooLongTables.Any())
            {
                throw new MappingException($"Table names of the following mapped types are too long (> {options.MaxTableNameLength} characters):{Environment.NewLine}" +
                                           $"{string.Join(Environment.NewLine, nameTooLongTables.Select(o => $"Table: {o.Key}, Mapped classes: {string.Join(",", o.MappedClass.FullName)}"))}");
            }

            var nameTooLongProperties = configuration.ClassMappings
                .Where(o => o.MappedClass != null)
                .Select(o => o.MappedClass)
                .Where(o => o.GetProperties().Any(p => p.Name.Length > options.MaxPropertyNameLength))
                .ToList();
            if (nameTooLongProperties.Any())
            {
                throw new MappingException($"Property names of the following mapped types are too long (> {options.MaxPropertyNameLength} characters):{Environment.NewLine}" +
                                           string.Join(Environment.NewLine, nameTooLongProperties.Select(o => o.FullName)));
            }

            var invalidTableNamingTypes = configuration.ClassMappings
                .Where(o => o.Table != null)
                .Where(o => o.Table.Name != o.Table.Name.ToPascalCase())
                .ToList();
            if (invalidTableNamingTypes.Any())
            {
                throw new MappingException($"Table names of the following mapped types are not in a PascalCase format:{Environment.NewLine}" +
                                           string.Join(Environment.NewLine, invalidTableNamingTypes.Select(o => o.Table.Name)));
            }

            var invalidPropertyNamingTypes = configuration.ClassMappings
                .Where(o => o.MappedClass != null)
                .Select(o => o.MappedClass)
                .Where(o => o.GetProperties().Any(p => p.Name != p.Name.ToPascalCase()))
                .ToList();
            if (invalidPropertyNamingTypes.Any())
            {
                throw new MappingException($"Property names of the following mapped types are not in a PascalCase format:{Environment.NewLine}" +
                                           string.Join(Environment.NewLine, invalidPropertyNamingTypes.Select(o => o.FullName)));
            }
        }
    }
}
