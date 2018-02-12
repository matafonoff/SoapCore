using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

using Bacup.DESS.Abstraction;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SoapCore.SoapHelpers;

namespace SoapCore
{
	public class SoapEndpointMiddleware
	{
		private static readonly Lazy<TypeInfo[]> _soapServices = new Lazy<TypeInfo[]>(() =>
		                                                                              {
			                                                                              return AssemblyHelper.AssembliesWithinWorkingDir.SelectMany(x => x.DefinedTypes).Where(x => x.IsClass && !x.IsAbstract && !x.IsSealed).Where(type => type.GetServiceContracts().Any()).ToArray();
		                                                                              });

		private readonly Dictionary<string, ISoapEndpoint> _endpoints;
		private readonly MessageEncoder _messageEncoder;
		private readonly RequestDelegate _next;
		private readonly SoapSerializer _serializer;

		public SoapEndpointMiddleware(IServiceProvider serviceProvider, RequestDelegate next, MessageEncoder encoder, SoapSerializer serializer)
		{
			_next = next;
			_messageEncoder = encoder;
			_serializer = serializer;

			_endpoints = CollectSoapServices(serviceProvider).ToDictionary(x => x.EndpointPath, StringComparer.OrdinalIgnoreCase);
		}

		public static TypeInfo[] SoapServices => _soapServices.Value;

		public async Task Invoke(HttpContext httpContext, IServiceProvider serviceProvider)
		{
			httpContext.Request.EnableRewind();

			if (_endpoints.TryGetValue(httpContext.Request.Path.Value.TrimStart('/'), out var endpoint))
			{
				await endpoint.ProcessRequestAsync(httpContext, serviceProvider);
			}
			else
			{
				await _next(httpContext);
			}
		}

		private IEnumerable<ISoapEndpoint> CollectSoapServices(IServiceProvider serviceProvider)
		{
			var log = serviceProvider.GetRequiredService<ILogger<SoapEndpointMiddleware>>();

			log.LogInformation("SOAP endpoints:");

			var generic = typeof(SoapEndpoint<>);
			foreach (var serviceType in SoapServices)
			{
				var serviceEndpointType = generic.MakeGenericType(serviceType);

				var encoder = _messageEncoder;
				var serializer = _serializer;

				var endpoint = (ISoapEndpoint) serviceProvider.CreateInstance(serviceEndpointType, encoder, serializer);
				log.LogInformation($"    {endpoint.EndpointPath}");

				yield return endpoint;
			}
		}
	}
}
