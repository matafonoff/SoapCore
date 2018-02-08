using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
			return builder.UseSoapEndpoints((Binding) null, serializer);
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
			foreach (var service in SoapEndpointMiddleware.SoapServices)
			{
				if (singletone)
				{
					serviceCollection.TryAddSingleton(service);
				}
				else
				{
					serviceCollection.TryAddScoped(service);
				}
			}

			return serviceCollection;
		}
	}
}
