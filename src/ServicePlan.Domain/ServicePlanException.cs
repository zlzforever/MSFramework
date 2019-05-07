namespace ServicePlan.Domain
{
	public class ServicePlanException : MSFramework.MSFrameworkException
	{
		public ServicePlanException(string message) : base(message)
		{
		}
	}
}