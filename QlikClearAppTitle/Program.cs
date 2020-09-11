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
			if (!args.Any())
				PrintUsage();

			var location = Location.FromUri("http://localhost:4848");
			location.AsDirectConnectionToPersonalEdition();

            var appIds = location.GetAppIdentifiers();
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
                var appId = appIds.SingleOrDefault(appId => appId.AppId.EndsWith(arg));
                if (appId == null)
                {
                    Console.WriteLine("App not found: " + arg);
                }
                else
                {
                    Console.WriteLine("App found: "+ appId.AppId);
                }

                using (var app = location.App(appId))
                {
                    var appProps = app.GetAppProperties();
                    if (!appProps.IsSet("qTitle"))
                    {
                        Console.WriteLine("Title not set. No change required.");
                        continue;
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
        }

		private static void PrintUsage()
		{
			Console.WriteLine("Usage: QlikClearAppTitle.exe <qvfFile> [<qvfFile>*]");
			Environment.Exit(1);
		}
	}
}
