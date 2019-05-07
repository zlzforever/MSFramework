using System;
using System.Collections.Generic;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	/// <summary>
	/// 数据报告
	/// </summary>
	public class DataReport : ValueObject
	{
		public string ReportTitle { get; }

		public string Abstract { get; }

		public DataReport(string reportTitle, string @abstract)
		{
			ReportTitle = reportTitle;
			Abstract = @abstract;
		}

		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return ReportTitle;
			yield return Abstract;
		}
	}
}