using System;
using System.Dynamic;
using System.Reflection;

namespace MSFramework.Common
{
	public static class DynamicExtensions
	{
		public static dynamic ToDynamic(this object o)
		{
			if (o == null || o.GetType().IsPrimitive || o is string)
				return o;

			return new PrivateReflectionDynamicObject(o);
		}

		private class PrivateReflectionDynamicObject : DynamicObject
		{
			private readonly object _realObject;

			private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance |
			                                          System.Reflection.BindingFlags.Public |
			                                          System.Reflection.BindingFlags.NonPublic;

			public PrivateReflectionDynamicObject(object o)
			{
				_realObject = o;
			}

			// Called when a method is called
			public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
			{
				result = InvokeMemberOnType(_realObject.GetType(), _realObject, binder.Name, args);

				// Wrap the sub object if necessary. This allows nested anonymous objects to work.
				result = result?.ToDynamic();

				return true;
			}

			private static object InvokeMemberOnType(Type type, object target, string name, object[] args)
			{
				try
				{
					// Try to incoke the method
					return type.InvokeMember(
						name,
						BindingFlags.InvokeMethod | BindingFlags,
						null,
						target,
						args);
				}
				catch (MissingMethodException)
				{
					// If we couldn't find the method, try on the base class
					if (type.BaseType != null)
					{
						return InvokeMemberOnType(type.BaseType, target, name, args);
					}

					//Don't care if the method don't exist.
					return null;
				}
			}
		}
	}
}