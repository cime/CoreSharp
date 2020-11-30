using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CoreSharp.Common.Reflection
{
    /// <summary>
    /// Type builder extensions
    /// </summary>
    public static class TypeBuilderExtensions
    {

        /// <summary>
        /// Create new module builder
        /// </summary>
        /// <param name="assemblyNameRoot"></param>
        /// <returns></returns>
        public static ModuleBuilder ToNewAssemblyModuleBuilder(this string assemblyNameRoot)
        {
            var assemblyName = assemblyNameRoot + "." + Guid.NewGuid().ToString("N");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
            return moduleBuilder;
        }

        /// <summary>
        /// Create new type from existing type
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="constructorsBindings"></param>
        /// <returns></returns>
        public static TypeBuilder CreateNewTypeFromExistingType(this ModuleBuilder builder, Type type, string name, BindingFlags? constructorsBindings = null)
        {
            // create 
            var typeBuilder = builder.DefineType(name, TypeAttributes.Public, type);

            // copy constructors
            var constructors = constructorsBindings.HasValue ? type.GetConstructors(constructorsBindings.Value) : type.GetConstructors();
            constructors.ToList().ForEach(c => typeBuilder.AddConstructor(c));

            // return
            return typeBuilder;
        }

        /// <summary>
        /// Creates new type
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TypeBuilder CreateNewType(this ModuleBuilder builder, string name)
        {
            // create 
            var typeBuilder = builder.DefineType(name, TypeAttributes.Public);

            // return
            return typeBuilder;
        }

        /// <summary>
        /// Add constructor to typebuidler
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="constructor"></param>
        /// <returns></returns>
        public static TypeBuilder AddConstructor(this TypeBuilder typeBuilder, ConstructorInfo constructor)
        {

            // get constructor parameters 
            var paramsTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();

            // setup constructor
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, paramsTypes);
            var il = constructorBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); //load this
            Enumerable.Range(1, paramsTypes.Length).ToList().ForEach(i => il.Emit(OpCodes.Ldarg_S, i));
            il.Emit(OpCodes.Call, constructor);
            il.Emit(OpCodes.Ret);

            // return builder
            return typeBuilder;
        }

        /// <summary>
        /// Adds attribute to type
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="attrType"></param>
        /// <param name="ctorParams"></param>
        /// <param name="props"></param>
        public static void AddAttribute(this TypeBuilder typeBuilder, Type attrType, object[] ctorParams = null, Dictionary<string, object> props = null)
        {
             var attrBuilder = CreateCustomAttributeBuilder(attrType, ctorParams, props);
            typeBuilder.SetCustomAttribute(attrBuilder);
        }

        public static PropertyBuilder AddProperty(this TypeBuilder typeBuilder, Type type, string name, bool get = true, bool set = true)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + name, type, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.None, CallingConventions.HasThis, type, null);
            var getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            if(get)
            {
                var getter = typeBuilder.DefineMethod("get_" + name, getSetAttr, type, Type.EmptyTypes);
                var getIL = getter.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, fieldBuilder);
                getIL.Emit(OpCodes.Ret);
                propertyBuilder.SetGetMethod(getter);
            }

            if(set)
            {
                var setter = typeBuilder.DefineMethod("set_" + name, getSetAttr, null, new[] { type });
                var setIL = setter.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, fieldBuilder);
                setIL.Emit(OpCodes.Ret);
                propertyBuilder.SetSetMethod(setter);
            }

            return propertyBuilder;

        }

        /// <summary>
        /// Adds attribute to property
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <param name="attrType"></param>
        /// <param name="ctorParams"></param>
        /// <param name="props"></param>
        public static void AddAttribute(this PropertyBuilder propertyBuilder, Type attrType, object[] ctorParams = null, Dictionary<string, object> props = null)
        {
            var attrBuilder = CreateCustomAttributeBuilder(attrType, ctorParams, props);
            propertyBuilder.SetCustomAttribute(attrBuilder);
        }

        private static CustomAttributeBuilder CreateCustomAttributeBuilder(Type attrType, object[] ctorParams = null, Dictionary<string, object> props = null)
        {

            // get constructor
            var ctorParamTypes = ctorParams?.Select(p => p.GetType()).ToArray() ?? new Type[0];
            var ctorInfo = attrType.GetConstructor(ctorParamTypes);
            if(ctorInfo == null)
            {
                throw new ArgumentException("Invalid constructor");
            }
            ctorParams = ctorParams ?? new object[0];

            if(props?.Count > 0)
            {
                // with properties
                var propsTypes = props.Keys.Select(attrType.GetProperty).ToArray();
                var attrBuilder = new CustomAttributeBuilder(ctorInfo, ctorParams, propsTypes, props.Values.ToArray());
                return attrBuilder;
            }
            else
            {
                var attrBuilder = new CustomAttributeBuilder(ctorInfo, ctorParams);
                return attrBuilder;
            }

        }

    }
}
