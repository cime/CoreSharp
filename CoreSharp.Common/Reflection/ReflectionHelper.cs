using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

// https://github.com/nhibernate/nhibernate-core/blob/master/src/NHibernate/Util/ReflectHelper.cs
namespace CoreSharp.Common.Reflection
{
    /// <summary>
    /// Helper class for Reflection related code.
    /// </summary>
    public static  class ReflectionHelper
    {
        /// <summary>
		/// Extract the <see cref="MethodInfo"/> from a given expression.
		/// </summary>
		/// <typeparam name="TSource">The declaring-type of the method.</typeparam>
		/// <param name="method">The method.</param>
		/// <returns>The <see cref="MethodInfo"/> of the no-generic method or the generic-definition for a generic-method.</returns>
		/// <seealso cref="MethodInfo.GetGenericMethodDefinition"/>
		public static MethodInfo GetMethodDefinition<TSource>(Expression<Action<TSource>> method)
		{
			MethodInfo methodInfo = GetMethod(method);
			return methodInfo.IsGenericMethod ? methodInfo.GetGenericMethodDefinition() : methodInfo;
		}

		/// <summary>
		/// Extract the <see cref="MethodInfo"/> from a given expression.
		/// </summary>
		/// <typeparam name="TSource">The declaring-type of the method.</typeparam>
		/// <param name="method">The method.</param>
		/// <returns>The <see cref="MethodInfo"/> of the method.</returns>
		public static MethodInfo GetMethod<TSource>(Expression<Action<TSource>> method)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			return ((MethodCallExpression)method.Body).Method;
		}

		/// <summary>
		/// Extract the <see cref="MethodInfo"/> from a given expression.
		/// </summary>
		/// <typeparam name="TSource">The declaring-type of the method.</typeparam>
		/// <typeparam name="TResult">The return type of the method.</typeparam>
		/// <param name="method">The method.</param>
		/// <returns>The <see cref="MethodInfo"/> of the method.</returns>
		public static MethodInfo GetMethod<TSource, TResult>(Expression<Func<TSource, TResult>> method)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			return ((MethodCallExpression) method.Body).Method;
		}

		/// <summary>
		/// Extract the <see cref="MethodInfo"/> from a given expression.
		/// </summary>
		/// <param name="method">The method.</param>
		/// <returns>The <see cref="MethodInfo"/> of the no-generic method or the generic-definition for a generic-method.</returns>
		/// <seealso cref="MethodInfo.GetGenericMethodDefinition"/>
		public static MethodInfo GetMethodDefinition(Expression<System.Action> method)
		{
			MethodInfo methodInfo = GetMethod(method);
			return methodInfo.IsGenericMethod ? methodInfo.GetGenericMethodDefinition() : methodInfo;
		}

		/// <summary>
		/// Extract the <see cref="MethodInfo"/> from a given expression.
		/// </summary>
		/// <param name="method">The method.</param>
		/// <returns>The <see cref="MethodInfo"/> of the method.</returns>
		public static MethodInfo GetMethod(Expression<System.Action> method)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			return ((MethodCallExpression)method.Body).Method;
		}

		/// <summary>
		/// Get the <see cref="MethodInfo"/> for a public overload of a given method if the method does not match
		/// given parameter types, otherwise directly yield the given method.
		/// </summary>
		/// <param name="method">The method for which finding an overload.</param>
		/// <param name="parameterTypes">The arguments types of the overload to get.</param>
		/// <returns>The <see cref="MethodInfo"/> of the method.</returns>
		/// <remarks>Whenever possible, use GetMethod() instead for performance reasons.</remarks>
		public static MethodInfo GetMethodOverload(MethodInfo method, params System.Type[] parameterTypes)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));
			if (parameterTypes == null)
				throw new ArgumentNullException(nameof(parameterTypes));

			if (ParameterTypesMatch(method.GetParameters(), parameterTypes))
				return method;

			var overload = method.DeclaringType.GetMethod(method.Name,
				(method.IsStatic ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.Public,
				null, parameterTypes, null);

			if (overload == null)
				throw new InvalidOperationException(
					$"No overload found for method '{method.DeclaringType.Name}.{method.Name}' and parameter types '{string.Join(", ", parameterTypes.Select(t => t.Name))}'");

			return overload;
		}

        internal static bool ParameterTypesMatch(ParameterInfo[] parameters, System.Type[] types)
        {
            if (parameters.Length != types.Length)
            {
                return false;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == types[i])
                {
                    continue;
                }

                if (parameters[i].ParameterType.ContainsGenericParameters && types[i].ContainsGenericParameters &&
                    parameters[i].ParameterType.GetGenericArguments().Length == types[i].GetGenericArguments().Length)
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
