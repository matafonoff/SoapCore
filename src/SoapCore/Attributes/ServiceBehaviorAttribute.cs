using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoapCore.Attributes
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class ServiceBehaviorAttribute : Attribute
	{
		public InstanceContextMode InstanceContextMode { get; set; } = InstanceContextMode.PerCall;
	}
}
