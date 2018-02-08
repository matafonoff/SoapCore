using System.Reflection;
using System.ServiceModel;

namespace SoapCore
{
	public class OperationDescription
	{
		public OperationDescription(ContractDescription contract, MethodInfo operationMethod, OperationContractAttribute contractAttribute)
		{
			Contract = contract;
			Name = contractAttribute.Name ?? operationMethod.Name;
			SoapAction = contractAttribute.Action ?? $"{contract.Namespace.TrimEnd('/')}/{contract.Name}/{Name}";
			IsOneWay = contractAttribute.IsOneWay;
			ReplyAction = contractAttribute.ReplyAction;
			DispatchMethod = operationMethod;
		}

		public ContractDescription Contract { get; }
		public string SoapAction { get; }
		public string ReplyAction { get; }
		public string Name { get; }
		public MethodInfo DispatchMethod { get; }
		public bool IsOneWay { get; }
	}
}
