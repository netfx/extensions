#region BSD License
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

namespace System.Dynamic
{
	internal class PrivateDynamicObject : DynamicObject
	{
		private static readonly BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
		private object target;

		public PrivateDynamicObject(object target)
		{
			this.target = target;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			if (!base.TryInvokeMember(binder, args, out result))
			{
				var method = FindBestMatch(binder, this.target.GetType()
					.GetMethods(flags)
					.Where(x => x.Name == binder.Name && x.GetParameters().Length == args.Length),
					args);

				if (method == null)
				{
					// Try explicit interface implemetation next. 
					// Non-explicit have higher priority.
					method = FindBestMatch(binder, this.target.GetType()
						.GetInterfaces()
						.SelectMany(
							iface => this.target.GetType()
								.GetInterfaceMap(iface)
								.TargetMethods.Select(x => new { Interface = iface, Method = x }))
						.Where(x => x.Method.Name.Replace(x.Interface.FullName.Replace('+', '.') + ".", "") == binder.Name &&
							x.Method.GetParameters().Length == args.Length)
						.Select(x => x.Method)
						.Distinct(),
						args);
				}

				if (method != null)
				{
					result = AsDynamicIfNecessary(method.Invoke(this.target, args));
					return true;
				}
			}

			result = default(object);
			return false;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (!base.TryGetMember(binder, out result))
			{
				var property = target.GetType().GetProperty(binder.Name, flags);

				if (property != null)
				{
					result = AsDynamicIfNecessary(property.GetValue(target, null));
					return true;
				}

				var field = target.GetType().GetField(binder.Name, flags);
				if (field != null)
				{
					result = AsDynamicIfNecessary(field.GetValue(target));
					return true;
				}

				var explicitGetter = this.target.GetType()
					.GetInterfaces()
					.SelectMany(
						iface => this.target.GetType()
							.GetInterfaceMap(iface)
							.TargetMethods
							.Where(method => method.Name.StartsWith(iface.FullName.Replace('+', '.')) &&
								method.Name.Replace(iface.FullName.Replace('+', '.') + ".get_", "") == binder.Name))
					.FirstOrDefault();

				if (explicitGetter != null)
				{
					result = AsDynamicIfNecessary(explicitGetter.Invoke(this.target, null));
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
				var property = target.GetType().GetProperty(binder.Name, flags);

				if (property != null)
				{
					property.SetValue(target, value, null);
					return true;
				}

				var field = target.GetType().GetField(binder.Name, flags);
				if (field != null)
				{
					field.SetValue(target, value);
					return true;
				}

				var explicitSetter = this.target.GetType()
					.GetInterfaces()
					.SelectMany(
						iface => this.target.GetType()
							.GetInterfaceMap(iface)
							.TargetMethods
							.Where(method => method.Name.StartsWith(iface.FullName.Replace('+', '.')) &&
								method.Name.Replace(iface.FullName.Replace('+', '.') + ".set_", "") == binder.Name))
					.FirstOrDefault();

				if (explicitSetter != null)
				{
					explicitSetter.Invoke(this.target, new[] { value });
					return true;
				}
			}

			return false;
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			if (!base.TryGetIndex(binder, indexes, out result))
			{
				var indexers = this.target.GetType()
					.GetMethods(flags)
					.Where(x => x.Name == "get_Item" && x.GetParameters().Length == indexes.Length);

				var method = FindBestMatch(binder, indexers, indexes);
				if (method != null)
				{
					result = AsDynamicIfNecessary(method.Invoke(this.target, indexes));
					return true;
				}
			}

			result = default(object);
			return false;
		}

		private MethodInfo FindBestMatch(DynamicMetaObjectBinder binder, IEnumerable<MethodInfo> candidates, object[] args)
		{
			dynamic dynamicBinder = binder.AsPrivateDynamic();
			for (int i = 0; i < args.Length; i++)
			{
				var index = i;
				if (args[i] != null)
					candidates = candidates.Where(x => x.GetParameters()[index].ParameterType == args[index].GetType());
				else if (dynamicBinder.IsStandardBinder && dynamicBinder.ArgumentInfo[i + 1].IsByRef)
					candidates = candidates.Where(x => x.GetParameters()[index].ParameterType.IsByRef);
			}

			return candidates.FirstOrDefault();
		}

		private object AsDynamicIfNecessary(object value)
		{
			if (value == null)
				return value;

			var type = value.GetType();
			if (type.IsClass && type != typeof(string))
				return value.AsPrivateDynamic();

			return value;
		}
	}
}