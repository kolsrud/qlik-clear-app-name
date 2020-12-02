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
                Console.Write(arg);
                var appId = appIds.SingleOrDefault(appId => appId.AppId.EndsWith(arg));
                if (appId == null)
                {
                    Console.WriteLine(" - App not found.");
                    continue;
                }

                Console.Write(" - App found: "+ appId.AppId + " Opening... ");
                
                using (var app = location.AppAsync(appId).Result)
                {
                    var appProps = app.GetAppProperties();
                    if (!appProps.IsSet("qTitle"))
                    {
                        Console.WriteLine("Title not set. No change required.");
                        continue;
                    }

                    var structure = JObject.Parse(appProps.PrintStructure());
                    structure.Remove("qTitle");
                    var newAppProps = app.Session.Deserialize<NxAppProperties>(structure);
                    Console.WriteLine("Title cleared. Saving...");
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
