using RestSharp;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;


namespace Stationboard
{
    class Program
    {

        // NEW CLASS FOR COMMANDS
        public sealed class StationCommand : Command<StationCommand.Settings>
        {

            // CLASS FOR SETTINGS
            public sealed class Settings : CommandSettings
            {
                [CommandOption("-s|--station <Station>")]
                [Description("Specifies the location of which a stationboard should be returned. Defaults to '" + DEFAULT_STATION + "'")]
                public string? Station { get; init; }

                [CommandOption("-l|--limit <Limit>")]
                [Description("Number of departing connections to return")]
                public int? Limit { get; init; }
            }

            // CLASS OR CLASS ??? FOR EXECUTING COMMANDS
            public override int Execute(CommandContext context, Settings settings)
            {
                // default values (if no arguments are parsed)
                string station = settings.Station ?? DEFAULT_STATION;
                int limit = settings.Limit ?? DEFAULT_LIMIT;

                Console.WriteLine($"### Station: {station} ###\r\n");

                AnsiConsole.Status()
                    .Start("Fetching data...", ctx =>
                    {
                        Thread.Sleep(1000);

                        // Update the status and spinner
                        ctx.Spinner(Spinner.Known.Dots3);
                        ctx.SpinnerStyle(Style.Parse("green"));

                        Thread.Sleep(1000);
                    });

                
                // keep spinning until data is fetched

                // call method GetStationboard to fetch stationboard data
                StationboardResponse response = GetStationboard(station, limit);



                // display results
                DisplayResults(response, station);

                return 0;
            }
        }

        public const string DEFAULT_STATION = "Zurich";
        public const int DEFAULT_LIMIT = 10;

        // MAIN METHOD
        static int Main(string[] args)
        {
            var app = new CommandApp<StationCommand>();
            return app.Run(args);
        }

        // METHOD GET STATION BOARD
        public static StationboardResponse GetStationboard(string getStation, int getLimit)
        {
            var client = CreateClient();
            var requestStationboard = CreateRequestStationboard(getStation, getLimit);

            // Sleep while fetching

            return client.Get<StationboardResponse>(requestStationboard);
        }

        // CREATE CLIENT METHOD
        public static RestClient CreateClient()
        {
            var options = new RestClientOptions("https://transport.opendata.ch/v1")
            {
                Timeout = TimeSpan.FromSeconds(10),
            };
            return new RestClient(options);
        }

        // CREATE REQUEST STATIONBOARD METHOD
        public static RestRequest CreateRequestStationboard(string requestStation, int requestLimit)
        {
            RestRequest requestStationboard = new RestRequest("stationboard");
            requestStationboard.AddParameter("station", requestStation);
            requestStationboard.AddParameter("limit", requestLimit);
            requestStationboard.AddParameter("fields[]", "stationboard/stop/departure");
            requestStationboard.AddParameter("fields[]", "stationboard/stop/delay");
            requestStationboard.AddParameter("fields[]", "stationboard/category");
            requestStationboard.AddParameter("fields[]", "stationboard/number");
            requestStationboard.AddParameter("fields[]", "stationboard/to");
            return requestStationboard;
        }

        // DISPLAY RESULTS METHOD
        public static void DisplayResults(StationboardResponse response, string station)
        {
            var table = new Table();
            table.Border = TableBorder.AsciiDoubleHead;

            table.AddColumn("[bold yellow]Departure[/]");
            table.AddColumn("[bold yellow]Vehicle[/]");
            table.AddColumn("[bold yellow]Destination[/]");

            // check for response data
            if (response.stationboard == null || response.stationboard.Length == 0)
            {
                Console.WriteLine("No data available for the specified station.");
                return;
            }

            // iterates over every object in stationboard
            foreach (Stationboard sb in response.stationboard)
            {
                string departure = ExtractDepartureTime(sb);
                string delay = ExtractDelay(sb.stop.delay);
                string vehicle = ExtractVehicle(sb);

                table.AddRow(departure + " [red]" + delay + "[/]", vehicle, sb.to);
            }

            AnsiConsole.Write(table);
            Console.ReadLine();
        }

        // NO METHOD NOR CLASS 
        public static string ExtractDepartureTime(Stationboard sb) => DateTime.Parse(sb.stop.departure).ToString("HH:mm");

        public static string ExtractVehicle(Stationboard sb) => sb.category + sb.number;

        public static string ExtractDelay(int? delay) => delay == null || delay == 0 ? "" : "+" + delay;

        public record Stationboard(StationboardStop stop, string to, string number, string category); // stop => nested inside stationboard
        public record StationboardStop(string departure, int? delay); // delay nullable value type
        public record StationboardResponse(Stationboard[] stationboard);
    }
}