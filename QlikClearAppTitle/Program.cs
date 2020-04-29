using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace QlikClearAppTitle
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 1)
				PrintUsage();

			var location = Location.FromUri("http://localhost:4848");
			location.AsDirectConnectionToPersonalEdition();

			var appId = location.AppWithNameOrDefault(args[0]);
			if (appId == null)
			{
				Console.WriteLine("App not found: " + args[0]);
			}

			using (var app = location.App(appId, noData: true))
			{
				var appProps = app.GetAppProperties();
				if (!appProps.IsSet("qTitle"))
				{
					Console.WriteLine("Title not set. No change required.");
					return;
				}

				Console.WriteLine(appProps.PrintStructure(Formatting.Indented));
				var structure = JObject.Parse(appProps.PrintStructure());
				structure.Remove("qTitle");
				var newAppProps = app.Session.Deserialize<NxAppProperties>(structure);
				Console.WriteLine(newAppProps.PrintStructure(Formatting.Indented));
				app.SetAppProperties(newAppProps);
				app.DoSave();
			}
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Usage: QlikClearAppTitle.exe <appName>");
			Environment.Exit(1);
		}
	}
}
