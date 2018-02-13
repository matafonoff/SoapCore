using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SoapCore.Attributes;

namespace SoapCore
{
	public static class SoapEndpointExtensions
	{
		public static IApplicationBuilder UseSoapEndpoints(this IApplicationBuilder builder, MessageEncoder encoder, SoapSerializer serializer = SoapSerializer.DataContractSerializer)
		{
			return builder.UseMiddleware<SoapEndpointMiddleware>(encoder, serializer);
		}

		public static IApplicationBuilder UseSoapEndpoints(this IApplicationBuilder builder, SoapSerializer serializer = SoapSerializer.DataContractSerializer)
		{
			return builder.UseSoapEndpoints((Binding)null, serializer);
		}

		public static IApplicationBuilder UseSoapEndpoints(this IApplicationBuilder builder, Binding binding, SoapSerializer serializer = SoapSerializer.DataContractSerializer)
		{
			if (binding == null)
			{
				binding = new BasicHttpBinding();
			}

			return builder.UseSoapEndpoints(binding.GetMessageEncoder(), serializer);
		}

		public static IServiceCollection AddSoapExceptionTransformer(this IServiceCollection serviceCollection, Func<Exception, string> transformer)
		{
			serviceCollection.TryAddSingleton(new ExceptionTransformer(transformer));
			return serviceCollection;
		}

		public static IServiceCollection AddSoapServices(this IServiceCollection serviceCollection, bool singletone = false)
		{
			foreach (var service in SoapEndpointMiddleware.SoapServices.Select(x=>new ServiceDescription(x)))
			{
				var serviceBehavior = service.ServiceType.GetCustomAttribute<ServiceBehaviorAttribute>();

				if (serviceBehavior != null)
				{
					AddSoapServicesByAttribute(serviceCollection, serviceBehavior, service);
				}
				else
				{
					AddSoapServicesByNotation(serviceCollection, singletone, service);
				}
			}

			return serviceCollection;
		}

		private static void AddSoapServicesByNotation(IServiceCollection serviceCollection, bool singletone, ServiceDescription service)
		{
			if (singletone)
			{
				AddSingletones(serviceCollection, service);
			}
			else
			{
				AddScoped(serviceCollection, service);
			}
		}
		
		private static void AddSoapServicesByAttribute(IServiceCollection serviceCollection, ServiceBehaviorAttribute serviceBehavior, ServiceDescription service)
		{
			switch (serviceBehavior.InstanceContextMode)
			{
				case InstanceContextMode.PerCall:
					AddSingletones(serviceCollection, service);


					break;
				case InstanceContextMode.Singleton:
					AddScoped(serviceCollection, service);


					break;
			}
		}

		private static void AddScoped(IServiceCollection serviceCollection, ServiceDescription service)
		{
			serviceCollection.TryAddScoped(service.ServiceType);
			foreach (var contract in service.Contracts)
			{
				serviceCollection.TryAddScoped(contract.ContractType, service.ServiceType);
			}
		}

		private static void AddSingletones(IServiceCollection serviceCollection, ServiceDescription service)
		{
			serviceCollection.AddSingleton(service.ServiceType);
			foreach (var contract in service.Contracts)
			{
				serviceCollection.AddSingleton(contract.ContractType, service.ServiceType);
			}
		}
	}
}
