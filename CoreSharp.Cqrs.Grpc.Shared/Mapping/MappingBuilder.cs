using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CoreSharp.Cqrs.Grpc.Common;
namespace CoreSharp.Cqrs.Grpc.Mapping
{
    public static class MappingBuilder
    {

        public static IMapper CreateMapper(IReadOnlyDictionary<Type, Type> chContracts, IPropertyMapValidator validator = null)
        {
            var mapperConfiguration = new MapperConfiguration(cfg => {
                chContracts.ForEach(x => {

                    var srcType = x.Key;
                    var dstType = x.Value;
                    var srcProps = srcType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(y => y.CanRead && y.CanWrite);
                    var dstProps = dstType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(y => y.CanRead && y.CanWrite);

                    // mapping src => dest
                    var srcMap = cfg.CreateMap(srcType, dstType);
                    srcProps.Where(p => dstProps.Select(s => s.Name).Contains(p.Name)).ForEach(p => {

                        srcMap.ForMember(p.Name, opt => {

                            // check if map is required (prevent lazy load)
                            opt.PreCondition(obj => validator?.MapProperty(obj, p.Name) ?? true);

                            // special maps 
                            ToChannelPropertyMap(opt, p, srcProps, dstProps);
                        });
                    });

                    // mapping dest => src
                    var dstMap = cfg.CreateMap(dstType, srcType, MemberList.None);
                    dstProps.Where(p => srcProps.Select(s => s.Name).Contains(p.Name)).ForEach(p => {
                        dstMap.ForMember(p.Name, opt => {

                            // check if map is required (prevent lazy load)
                            opt.PreCondition(obj => validator?.MapProperty(obj, p.Name) ?? true);

                            // special maps 
                            FromChannelPropertyMap(opt, p, dstProps, srcProps);

                        });
                    });

                });
            });
            var mapper = mapperConfiguration.CreateMapper();
            return mapper;
        }

        public static void ToChannelPropertyMap(IMemberConfigurationExpression opt, PropertyInfo property, IEnumerable<PropertyInfo> srcProps, IEnumerable<PropertyInfo> dstProps)
        {
            // nullable types
            var dstProp = dstProps.First(d => d.Name == property.Name);
            if (dstProp.PropertyType.IsGenericType && dstProp.PropertyType.GetGenericTypeDefinition() == typeof(ProtoNullable<>))
            {
                var valueType = dstProp.PropertyType.GetGenericArguments().First();
                var converterType = typeof(ProtoNullableToChannelConverter<>).MakeGenericType(valueType);
                opt.ConvertUsing(converterType);
            }
        }

        public static void FromChannelPropertyMap(IMemberConfigurationExpression opt, PropertyInfo property, IEnumerable<PropertyInfo> srcProps, IEnumerable<PropertyInfo> dstProps)
        {

            // nullable types
            var srcProp = srcProps.First(d => d.Name == property.Name);
            if (srcProp.PropertyType.IsGenericType && srcProp.PropertyType.GetGenericTypeDefinition() == typeof(ProtoNullable<>))
            {
                var valueType = srcProp.PropertyType.GetGenericArguments().First();
                var converterType = typeof(ProtoNullableFromChannelConverter<>).MakeGenericType(valueType);
                opt.ConvertUsing(converterType);
            }
        }
    }
}
