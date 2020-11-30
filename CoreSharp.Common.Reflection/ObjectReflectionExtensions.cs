using System.Linq;
using System.Reflection;

namespace CoreSharp.Common.Reflection
{
    public static class ObjectReflectionExtensions
    {

        /// <summary>
        /// Imports properties values from srcObj to obj
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="srcObj"></param>
        public static void ImportProperties<T>(this T obj, object srcObj)
        {

            // todo: use cached delegates instead of reflection

            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite && x.CanRead)
                .ToList().ForEach(target => {
                    var src = srcObj?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(x => x.CanWrite && x.CanRead && x.PropertyType == target.PropertyType && x.Name == target.Name);
                    if (src != null)
                    {
                        var val = src.GetValue(srcObj);
                        target.SetValue(obj, val);
                    }
                });
        }
    }
}
