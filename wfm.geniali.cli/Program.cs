using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Serilog;

using wfm.geniali.cli.Commands;
using wfm.geniali.rest;

namespace wfm.geniali.cli
{
    public class Program
    {
        private static string _Promt = "wfm> ";

        public string Platform
        {
            get;
            set;
        } = "pc";

        public string Region
        {
            get;
            set;
        } = "en";

        public string Status
        {
            get;
            set;
        } = "ingame";

        private static void Main(string[] args)
        {
            new Program().Run(args);
        }

        private void Run(string[] args)
        {
            IConfigurationRoot config = InitializeConfiguration();
            InitializeLogger(config);

            if(Directory.Exists("cache") == false)
            {
                Directory.CreateDirectory("cache");
            }

            WfmClient client = new WfmClient();

            string input = string.Empty;
            bool   exit  = false;

            PrintWelcome();

            do
            {
                input = ReadInput().ToLower();

                switch(input)
                {
                    case "e":
                    case "exit":
                        exit = true;

                        break;
                    case "help":
                    case "h":
                        PrintHelp();

                        break;
                    case "update":
                    case "u":
                        new Update().Execute(this, client);

                        break;
                    case "list":
                    case "l":
                        new List().Execute(this, client);

                        break;
                    default:
                        continue;
                }

                CWL();
            } while(exit == false);
        }

        private void PrintHelp()
        {
            CWL("h | help \t für Hilfe");
            CWL("e | exit \t für beenden");
            CWL("l | list \t für Listenmodus");
        }

        private void PrintWelcome()
        {
            Console.Clear();
            
            CWL("Welcome to the Warframe Market CLI.");
            CWL("h | help \t für Hilfe");
            CWL();
        }

        public void CWL(string message = "")
        {
            Console.WriteLine(message);
        }

        public void CW(string message)
        {
            Console.Write(message);
        }

        public string ReadInput(string prompt = "")
        {
            Console.Write(string.IsNullOrEmpty(prompt) ? _Promt : prompt);
            return Console.ReadLine();
        }

        private void InitializeLogger(IConfigurationRoot config)
        {
            Log.Logger = new LoggerConfiguration()
                         .ReadFrom.Configuration(config)
                         .Enrich.FromLogContext()
                         .CreateLogger();
        }

        private IConfigurationRoot InitializeConfiguration()
        {
            return new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                             .AddJsonFile(AppDomain.CurrentDomain.BaseDirectory + "\\appsettings.json", optional: true, reloadOnChange: true)
                                             .AddEnvironmentVariables()
                                             .Build();
        }
    }
}
