using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace SoapCore.SoapHelpers
{
	interface ISoapEndpoint
	{
		string EndpointPath { get; }
		Task ProcessRequestAsync(HttpContext httpContext, IServiceProvider serviceProvider);
	}
}
