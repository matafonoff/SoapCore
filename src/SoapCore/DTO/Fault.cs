﻿using System.Xml.Serialization;

namespace SoapCore
{
	public class Fault
	{
		public Fault()
		{
			Details = string.Empty;
			FaultCode = "s:Client";
		}

		[XmlElement(ElementName = "faultcode")]
		public string FaultCode { get; set; }

		[XmlElement(ElementName = "faultstring")]
		public string FaultString { get; set; }

		[XmlElement(ElementName = "detail")]
		public string Details { get; set; }
	}
}
