using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class StringExtensions
    {
        public static string GetSha1Hash(this string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("String is empty and not hashable.");
            var ascii = Encoding.ASCII.GetBytes(value);
            var bytes = SHA1.Create().ComputeHash(ascii);
            return bytes.Aggregate("", (current, b) => current + string.Format("{0:x2}", b));
        }

        /// <summary>
        /// Capitalize first letter of <paramref name="value"/>
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>String with first letter capitalized</returns>
        public static string ToUpperFirstChar(this string value)
        {
            return char.ToUpper(value[0]) + value.Substring(1);
        }

        public static string ToLowerFirstChar(this string value)
        {
            return char.ToLower(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// Get MemoryStream from <paramref name="value"/>
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>MemoryStream containing <paramref name="value"/></returns>
        public static Stream ToStream(this string value)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(value);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static Type GetTypeFromSimpleName(this string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            bool isArray = false, isNullable = false;

            if (typeName.IndexOf("[]") != -1)
            {
                isArray = true;
                typeName = typeName.Remove(typeName.IndexOf("[]"), 2);
            }

            if (typeName.IndexOf("?") != -1)
            {
                isNullable = true;
                typeName = typeName.Remove(typeName.IndexOf("?"), 1);
            }

            typeName = typeName.ToLower();

            string? parsedTypeName = null;

            switch (typeName)
            {
                case "bool":
                case "boolean":
                    parsedTypeName = "System.Boolean";
                    break;
                case "byte":
                    parsedTypeName = "System.Byte";
                    break;
                case "char":
                    parsedTypeName = "System.Char";
                    break;
                case "datetime":
                    parsedTypeName = "System.DateTime";
                    break;
                case "datetimeoffset":
                    parsedTypeName = "System.DateTimeOffset";
                    break;
                case "decimal":
                    parsedTypeName = "System.Decimal";
                    break;
                case "double":
                    parsedTypeName = "System.Double";
                    break;
                case "float":
                    parsedTypeName = "System.Single";
                    break;
                case "int16":
                case "short":
                    parsedTypeName = "System.Int16";
                    break;
                case "int32":
                case "int":
                    parsedTypeName = "System.Int32";
                    break;
                case "int64":
                case "long":
                    parsedTypeName = "System.Int64";
                    break;
                case "object":
                    parsedTypeName = "System.Object";
                    break;
                case "sbyte":
                    parsedTypeName = "System.SByte";
                    break;
                case "string":
                    parsedTypeName = "System.String";
                    break;
                case "timespan":
                    parsedTypeName = "System.TimeSpan";
                    break;
                case "uint16":
                case "ushort":
                    parsedTypeName = "System.UInt16";
                    break;
                case "uint32":
                case "uint":
                    parsedTypeName = "System.UInt32";
                    break;
                case "uint64":
                case "ulong":
                    parsedTypeName = "System.UInt64";
                    break;
            }

            if (parsedTypeName != null)
            {
                if (isArray)
                    parsedTypeName = parsedTypeName + "[]";

                if (isNullable)
                    parsedTypeName = string.Concat("System.Nullable`1[", parsedTypeName, "]");
            }
            else
                parsedTypeName = typeName;

            // Expected to throw an exception in case the type has not been recognized.
            return Type.GetType(parsedTypeName);
        }

        public static bool In(this string source, params string[] items)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return items.Contains(source, StringComparer.OrdinalIgnoreCase);
        }

        public static string SafeSubstring(this string value, int length)
        {
            return value.Substring(0, (int)Math.Min(value.Length, length));
        }

        public static string ToPascalCase(this string name)
        {
            var resultBuilder = new System.Text.StringBuilder();

            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                var cp = i > 0 ? (char?)name[i - 1] : null;

                if (!char.IsLetterOrDigit(c))
                {
                    resultBuilder.Append(" ");
                }
                else if (char.IsUpper(c) && (cp == null || !char.IsUpper(cp.Value)))
                {
                    resultBuilder.Append(" " + c);
                }
                else
                {
                    resultBuilder.Append(c);
                }
            }

            var result = resultBuilder.ToString();
            result = result.ToLower();
            var ti = CultureInfo.InvariantCulture.TextInfo;

            return ti.ToTitleCase(result).Replace(" ", string.Empty);
        }
    }
}
