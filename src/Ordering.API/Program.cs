using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Ordering.API
{
	public class Evt
	{
		public string AggregateId { get; }
		public int Version { get; }
	}

	public class Evt1 : Evt
	{
	}

	public class Evt2 : Evt
	{
	}

	class Root
	{
		private readonly List<Evt> _unCommittedEvts = new List<Evt>();

		public string Id { get; private set; }

		public int Version { get; private set; }

		private void Appy(Evt1 e)
		{
			// do something
		}

		private void Appy(Evt2 e)
		{
			// do something
		}

		public void Change1()
		{
			ApplyAggregateEvent(new Evt1());
		}

		public void Change2()
		{
			ApplyAggregateEvent(new Evt2());
		}

		protected void ApplyAggregateEvent(Evt e)
		{
			ApplyAggregateEvent(e, true);
		}

		private void ApplyAggregateEvent(Evt e, bool isNew)
		{
			lock (_unCommittedEvts)
			{
				((dynamic) this).Apply(e);
				if (isNew)
				{
					_unCommittedEvts.Add(e);
				}
				else
				{
					Id = e.AggregateId;
					Version++;
				}
			}
		}
	}


	public class Program
	{
		public static void Main(string[] args)
		{
			var configure = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Verbose()
#else
            				.MinimumLevel.Information()
#endif
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo
				.RollingFile("ordering.log");
			Log.Logger = configure.CreateLogger();

			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>().UseSerilog().UseUrls("http://0.0.0.0:5000");
	}
}