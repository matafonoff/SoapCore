using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace SoapCore
{
	public static class Extensions
	{
		public static object CreateInstance(this IServiceProvider provider, Type type, params object[] arguments)
		{
			if (arguments.Length == 0)
			{
				var ctor = type.GetConstructor(Type.EmptyTypes);
				if (ctor != null)
				{
					return Activator.CreateInstance(type);
				}
			}

			var args = new List<object>();

			var argumentsList = arguments.ToList();

			foreach (var ctor in type.GetConstructors().Select(x =>
				                                                   new
				                                                   {
					                                                   Ctor = x,
					                                                   Parameters = x.GetParameters()
				                                                   }).Where(x => x.Parameters.Length >= arguments.Length).OrderBy(x => x.Parameters.Length))
			{
				args.Clear();
				var initialized = true;
				foreach (var param in ctor.Parameters)
				{
					if (TryGetArgument(argumentsList, param.ParameterType, out var value))
					{
						args.Add(value);
					}
					else
					{
						value = provider.GetService(param.ParameterType);
						if (value == null)
						{
							initialized = false;
							break;
						}

						args.Add(value);
					}
				}

				if (initialized)
				{
					return Activator.CreateInstance(type, args.ToArray());
				}
			}

			throw new MissingMemberException();
		}

		public static MessageEncoder GetMessageEncoder(this Binding binding)
		{
			if (binding == null)
			{
				return null;
			}

			var element = binding.CreateBindingElements().Find<MessageEncodingBindingElement>();
			var factory = element.CreateMessageEncoderFactory();
			var encoder = factory.Encoder;
			return encoder;
		}

		public static IEnumerable<Type> GetServiceContracts(this Type serviceImpl)
		{
			if (!serviceImpl.IsClass ||
			    serviceImpl.IsAbstract)
			{
				yield break;
			}

			if (serviceImpl.GetCustomAttribute<ServiceContractAttribute>() != null)
			{
				yield return serviceImpl;
			}

			foreach (var contract in serviceImpl.GetInterfaces().Where(x => x.GetCustomAttribute<ServiceContractAttribute>() != null))
			{
				yield return contract;
			}
		}

		private static bool TryGetArgument(List<object> args, Type type, out object value)
		{
			value = null;

			for (var i = 0; i < args.Count; i++)
			{
				if (type.IsInstanceOfType(args[i]))
				{
					value = args[i];
					args.RemoveAt(i);
					return true;
				}
			}

			return false;
		}
	}
}
