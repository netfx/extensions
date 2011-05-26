﻿#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Dynamic;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Dynamic
{
	/// <summary>
	/// Provides reflection-based dynamic syntax for objects and types. 
	/// This class provides the extension methods <see cref="AsReflectionDynamic{object}"/> 
	/// and <see cref="AsReflectionDynamic{Type}"/> as entry points.
	/// </summary>
	internal static class ReflectionDynamic
	{
		/// <summary>
		/// Provides dynamic syntax for accessing the given object members.
		/// </summary>
		public static dynamic AsReflectionDynamic(this object obj)
		{
			if (obj == null)
				return null;

			return new ReflectionDynamicObject(obj);
		}

		/// <summary>
		/// Provides dynamic syntax for accessing the given type members.
		/// </summary>
		public static dynamic AsReflectionDynamic(this Type type)
		{
			if (type == null)
				return null;

			return new ReflectionDynamicObject(type);
		}

		/// <summary>
		/// Converts the type to a <see cref="TypeParameter"/> that 
		/// the reflection dynamic must use to make a generic 
		/// method invocation.
		/// </summary>
		public static TypeParameter AsGenericParameter(this Type type)
		{
			return new TypeParameter(type);
		}

		private class ReflectionDynamicObject : DynamicObject
		{
			private static readonly BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
			private static readonly MethodInfo castMethod = typeof(ReflectionDynamicObject).GetMethod("Cast", BindingFlags.Static | BindingFlags.NonPublic);
			private object target;
			private Type targetType;

			public ReflectionDynamicObject(object target)
			{
				this.target = target;
				this.targetType = target.GetType();
			}

			public ReflectionDynamicObject(Type type)
			{
				this.target = null;
				this.targetType = type;
			}

			public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
			{
				if (!base.TryInvokeMember(binder, args, out result))
				{
					var memberName = (binder.Name == "ctor" || binder.Name == "cctor") ? "." + binder.Name : binder.Name;
					var method = FindBestMatch(binder, memberName, args);
					if (method != null)
					{
						if (binder.Name == "ctor")
						{
							var instance = this.target;
							if (instance == null)
								instance = FormatterServices.GetSafeUninitializedObject(this.targetType);

							result = Invoke(method, instance, args);
							result = instance.AsReflectionDynamic();
						}
						else
						{
							result = AsDynamicIfNecessary(Invoke(method, this.target, args));
						}

						return true;
					}
				}

				result = default(object);
				return false;
			}

			private static object Invoke(IInvocable method, object instance, object[] args)
			{
				var refArgs = new Dictionary<int, RefValue>();
				var outArgs = new Dictionary<int, OutValue>();
				for (int i = 0; i < method.Parameters.Count; i++)
				{
					if (method.Parameters[i].ParameterType.IsByRef)
					{
						var refArg = args[i] as RefValue;
						var outArg = args[i] as OutValue;
						if (refArg != null)
							refArgs[i] = refArg;
						else if (outArg != null)
							outArgs[i] = outArg;
					}
				}

				foreach (var refArg in refArgs)
				{
					args[refArg.Key] = refArg.Value.Value;
				}
				foreach (var outArg in outArgs)
				{
					args[outArg.Key] = null;
				}

				var result = method.Invoke(instance, args.Where(x => !(x is TypeParameter)).ToArray());

				foreach (var refArg in refArgs)
				{
					refArg.Value.Value = args[refArg.Key];
				}
				foreach (var outArg in outArgs)
				{
					outArg.Value.Value = args[outArg.Key];
				}

				return result;
			}

			public override bool TryGetMember(GetMemberBinder binder, out object result)
			{
				if (!base.TryGetMember(binder, out result))
				{
					var field = targetType.GetField(binder.Name, flags);
					if (field != null)
					{
						result = AsDynamicIfNecessary(field.GetValue(target));
						return true;
					}

					var getter = FindBestMatch(binder, "get_" + binder.Name, new object[0]);
					if (getter != null)
					{
						result = AsDynamicIfNecessary(getter.Invoke(this.target, null));
						return true;
					}
				}

				result = default(object);
				return false;
			}

			public override bool TrySetMember(SetMemberBinder binder, object value)
			{
				if (!base.TrySetMember(binder, value))
				{
					var field = targetType.GetField(binder.Name, flags);
					if (field != null)
					{
						field.SetValue(target, value);
						return true;
					}

					var setter = FindBestMatch(binder, "set_" + binder.Name, new[] { value });
					if (setter != null)
					{
						setter.Invoke(this.target, new[] { value });
						return true;
					}
				}

				return false;
			}

			public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
			{
				if (!base.TryGetIndex(binder, indexes, out result))
				{
					var indexer = FindBestMatch(binder, "get_Item", indexes);
					if (indexer != null)
					{
						result = AsDynamicIfNecessary(indexer.Invoke(this.target, indexes));
						return true;
					}
				}

				result = default(object);
				return false;
			}

			public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
			{
				if (!base.TrySetIndex(binder, indexes, value))
				{
					var args = indexes.Concat(new[] { value }).ToArray();
					var indexer = FindBestMatch(binder, "set_Item", args);
					if (indexer != null)
					{
						indexer.Invoke(this.target, args);
						return true;
					}
				}

				return false;
			}

			public override bool TryConvert(ConvertBinder binder, out object result)
			{
				try
				{
					result = castMethod.MakeGenericMethod(binder.Type).Invoke(null, new[] { this.target });
					return true;
				}
				catch (Exception) { }

				var convertible = this.target as IConvertible;
				if (convertible != null)
				{
					try
					{
						result = Convert.ChangeType(convertible, binder.Type);
						return true;
					}
					catch (Exception) { }
				}


				result = default(object);
				return false;
			}

			private IInvocable FindBestMatch(DynamicMetaObjectBinder binder, string memberName, object[] args)
			{
				var finalArgs = args.Where(x => !(x is TypeParameter)).ToArray();
				var genericTypeArgs = new List<Type>();

				if (binder is InvokeBinder || binder is InvokeMemberBinder)
				{
					IEnumerable typeArgs = binder.AsReflectionDynamic().TypeArguments;
					genericTypeArgs.AddRange(typeArgs.Cast<Type>());
					genericTypeArgs.AddRange(args.OfType<TypeParameter>().Select(x => x.Type));
				}

				var method = FindBestMatchImpl(binder, finalArgs, genericTypeArgs.Count, this.targetType
					.GetMethods(flags)
					.Where(x => x.Name == memberName && x.GetParameters().Length == finalArgs.Length)
					.Select(x => new MethodInvocable(x)));

				if (method == null)
				{
					// Fallback to explicitly implemented members.
					method = FindBestMatchImpl(binder, finalArgs, genericTypeArgs.Count, this.targetType
						.GetInterfaces()
						.SelectMany(
							iface => this.targetType
								.GetInterfaceMap(iface)
								.TargetMethods.Select(x => new { Interface = iface, Method = x }))
						.Where(x =>
							x.Method.GetParameters().Length == finalArgs.Length &&
							x.Method.Name.Replace(x.Interface.FullName.Replace('+', '.') + ".", "") == memberName)
						.Select(x => (IInvocable)new MethodInvocable(x.Method))
						.Concat(this.targetType.GetConstructors(flags)
							.Where(x => x.Name == memberName && x.GetParameters().Length == finalArgs.Length)
							.Select(x => new ConstructorInvocable(x)))
						.Distinct());
				}

				var methodInvocable = method as MethodInvocable;
				if (method != null && methodInvocable != null && methodInvocable.Method.IsGenericMethodDefinition)
				{
					method = new MethodInvocable(methodInvocable.Method.MakeGenericMethod(genericTypeArgs.ToArray()));
				}

				return method;
			}

			private IInvocable FindBestMatchImpl(DynamicMetaObjectBinder binder, object[] args, int genericArgs, IEnumerable<IInvocable> candidates)
			{
				dynamic dynamicBinder = binder.AsReflectionDynamic();
				for (int i = 0; i < args.Length; i++)
				{
					var index = i;
					if (args[index] != null)
						candidates = candidates.Where(x => x.Parameters[index].ParameterType == GetArgumentType(args[index]));

					IEnumerable enumerable = dynamicBinder.ArgumentInfo;
					// The binder has the extra argument info for the "this" parameter at the beginning.
					if (enumerable.Cast<object>().ToList()[index + 1].AsReflectionDynamic().IsByRef)
						candidates = candidates.Where(x => x.Parameters[index].ParameterType.IsByRef);

					if (genericArgs > 0)
						candidates = candidates.Where(x => x.IsGeneric && x.GenericParameters == genericArgs);
				}

				return candidates.FirstOrDefault();
			}

			private static Type GetArgumentType(object arg)
			{
				if (arg is RefValue || arg is OutValue)
					return arg.GetType().GetGenericArguments()[0].MakeByRefType();
				//else if (arg is RefValue)
				//    return arg.GetType().GetGenericArguments()[0].MakeByRefType();

				return arg.GetType();
			}

			private object AsDynamicIfNecessary(object value)
			{
				if (value == null)
					return value;

				var type = value.GetType();
				if (type.IsClass && type != typeof(string))
					return value.AsReflectionDynamic();

				return value;
			}

			private static T Cast<T>(object target)
			{
				return (T)target;
			}

			private interface IInvocable
			{
				bool IsGeneric { get; }
				int GenericParameters { get; }
				IList<ParameterInfo> Parameters { get; }
				object Invoke(object obj, object[] parameters);
			}

			private class MethodInvocable : IInvocable
			{
				private MethodInfo method;
				private Lazy<IList<ParameterInfo>> parameters;

				public MethodInvocable(MethodInfo method)
				{
					this.method = method;
					this.parameters = new Lazy<IList<ParameterInfo>>(() => this.method.GetParameters());
				}

				public object Invoke(object obj, object[] parameters)
				{
					return this.method.Invoke(obj, parameters);
				}

				public IList<ParameterInfo> Parameters
				{
					get { return this.parameters.Value; }
				}

				public MethodInfo Method { get { return this.method; } }

				public bool IsGeneric { get { return this.method.IsGenericMethodDefinition; } }

				public int GenericParameters { get { return this.method.GetGenericArguments().Length; } }
			}

			private class ConstructorInvocable : IInvocable
			{
				private ConstructorInfo ctor;
				private Lazy<IList<ParameterInfo>> parameters;

				public ConstructorInvocable(ConstructorInfo ctor)
				{
					this.ctor = ctor;
					this.parameters = new Lazy<IList<ParameterInfo>>(() => this.ctor.GetParameters());
				}

				public object Invoke(object obj, object[] parameters)
				{
					return this.ctor.Invoke(obj, parameters);
				}

				public IList<ParameterInfo> Parameters
				{
					get { return this.parameters.Value; }
				}

				public bool IsGeneric { get { return false; } }

				public int GenericParameters { get { return 0; } }
			}
		}
	}
}