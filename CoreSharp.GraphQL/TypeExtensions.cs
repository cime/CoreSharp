#nullable disable

using System.ComponentModel;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class TypeExtensions
    {
        public static string Description(this MemberInfo memberInfo) => (memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute)?.Description;

        public static string ObsoleteMessage(this MemberInfo memberInfo) => (memberInfo.GetCustomAttributes(typeof(ObsoleteAttribute), false).FirstOrDefault() as ObsoleteAttribute)?.Message;
    }
}
