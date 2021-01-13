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
            
            List<Tuple<Type, Type>> nullablesMap = new List<Tuple<Type, Type>>();

            var mapperConfiguration = new MapperConfiguration(cfg => {
                chContracts.ForEach(x => {

                    var srcType = x.Key;
                    var dstType = x.Value;
                    var srcProps = srcType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(y => y.CanRead && y.CanWrite);
                    var dstProps = dstType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(y => y.CanRead && y.CanWrite);

                    // mapping nullables
                    srcProps.Where(p => dstProps.Select(s => s.Name).Contains(p.Name)).ForEach(p => {
                        var destProp = dstProps.First(d => d.Name == p.Name);
                        if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                            && destProp.PropertyType.IsGenericType && destProp.PropertyType.GetGenericTypeDefinition() == typeof(ProtoNullable<>))
                        {
                            var s = p.PropertyType.GetGenericArguments()[0];
                            var d = destProp.PropertyType.GetGenericArguments()[0];
                            var key = Tuple.Create(s, d);
                            if(!nullablesMap.Contains(key))
                            {
                                NullableTypeMap(cfg, s, d);
                                nullablesMap.Add(key);
                            }
                        }
                    });

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

                DateTimeTypesMap(cfg);

            });
            var mapper = mapperConfiguration.CreateMapper();
            return mapper;
        }

        private static void ToChannelPropertyMap(IMemberConfigurationExpression opt, PropertyInfo property, IEnumerable<PropertyInfo> srcProps, IEnumerable<PropertyInfo> dstProps)
        {

        }

        private static void FromChannelPropertyMap(IMemberConfigurationExpression opt, PropertyInfo property, IEnumerable<PropertyInfo> srcProps, IEnumerable<PropertyInfo> dstProps)
        {

        }

        private static void NullableTypeMap(IMapperConfigurationExpression opt, Type valueType, Type chValueType)
        {
            var nullableBase = typeof(Nullable<>);
            var chNullableBase = typeof(ProtoNullable<>);

            var valueObjType = nullableBase.MakeGenericType(valueType);
            var chValueObjType = chNullableBase.MakeGenericType(chValueType);

            var toChConverter = typeof(ProtoNullableToChannelConverter<,>).MakeGenericType(valueType, chValueType);
            opt.CreateMap(valueObjType, chValueObjType).ConvertUsing(toChConverter);

            var fromChConverter = typeof(ProtoNullableFromChannelConverter<,>).MakeGenericType(valueType, chValueType);
            opt.CreateMap(chValueObjType, valueObjType).ConvertUsing(fromChConverter);

        }

        private static void DateTimeTypesMap(IMapperConfigurationExpression opt)
        {
            opt.CreateMap<DateTime, long>().ConvertUsing<DateTimeToChannelConverter>();
            opt.CreateMap<long, DateTime>().ConvertUsing<DateTimeFromChannelConverter>();
            opt.CreateMap<DateTimeOffset, ProtoDateTimeOffset>().ConvertUsing<ProtoDateTimeOffsetToChannelConverter>();
            opt.CreateMap<ProtoDateTimeOffset, DateTimeOffset>().ConvertUsing<ProtoDateTimeOffsetFromChannelConverter>();
        }

    }
}
