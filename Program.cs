using RestSharp;
using Spectre.Console;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;


namespace Stationboard
{
    class Program
    {
        public const string DEFAULT_STATION = "Zurich";
        public const int DEFAULT_LIMIT = 10;

        static void Main(string[] args)
        {
            string station = DEFAULT_STATION;
            int limit = DEFAULT_LIMIT;

            foreach (var arg in args)
            {
                // Use the ternary operator for inline assignment
                if (isNumber(arg)) limit = Convert.ToInt32(arg); else station = arg;
            }

            static bool isNumber(string arg)
            {
                return int.TryParse(arg, out int value);
            }

            var table = new Table();
            table.Border = TableBorder.Rounded;

            Console.WriteLine("### Station: {0} ###\r\n", station);

            StationboardResponse response = GetStationboard(station, limit);

            table.AddColumn("[bold yellow]Departure[/]");
            table.AddColumn("[bold yellow]Vehicle[/]");
            table.AddColumn("[bold yellow]Destination[/]");

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

        public static StationboardResponse GetStationboard(string getStation, int getLimit)
        {
            var client = CreateClient();
            var requestStationboard = CreateRequestStationboard(getStation, getLimit);
            var response = client.Get<StationboardResponse>(requestStationboard); // get json data
            return response;
        }

        public static RestClient CreateClient()
        {
            var options = new RestClientOptions("https://transport.opendata.ch/v1")
            {
                Timeout = TimeSpan.FromSeconds(10),
            };
            return new RestClient(options);
        }

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

        public static string ExtractDepartureTime(Stationboard sb) => DateTime.Parse(sb.stop.departure).ToString("HH:mm");

        public static string ExtractVehicle(Stationboard sb) => sb.category + sb.number;

        public static string ExtractDelay(int? delay) => delay == null || delay == 0 ? "" : "+" + delay;

        public record Stationboard(StationboardStop stop, string to, string number, string category); // stop => nested inside stationboard
        public record StationboardStop(string departure, int? delay); // delay nullable value type
        public record StationboardResponse(Stationboard[] stationboard);
    }
}