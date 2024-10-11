using RestSharp;

namespace Stationboard
{
    class Program
    {
        // default constant values (if no input is written)
        public const string DEST_PLACE = "Romanshorn";
        public const int LIMIT = 3;

        static void Main(string[] args)
        {
            Console.WriteLine("### Abfahrtstafel ###");
            
            // define station
            Console.WriteLine("Please enter the station: ");
            string inputDestPlace = Console.ReadLine();
     
            // define limit
            Console.WriteLine("Please enter a limit: ");
            int inputLimit = int.Parse(Console.ReadLine());

            Program program = new Program(inputDestPlace, inputLimit);
 
            if (args.Length > 0)
            {
                inputDestPlace = args[0];
                inputLimit = int.Parse(args[1]);
                StationboardResponse response = GetStationboard();

                string headingFormat = String.Format("{0, -20} {1, -22} {2, -22} {3}", "Departure Time", "Delay in Minutes", "Vehicle", "Destination");
                Console.WriteLine(headingFormat);

                // iterates over every object in stationboard
                foreach (Stationboard sb in response.stationboard)
                {
                    string departure = ExtractDepartureTime(sb);
                    string delay = ExtractDelay(sb.stop.delay);
                    string vehicle = ExtractVehicle(sb);

                    string strFormat = String.Format("{0, -20} {1, -22} {2, -22} {3}", departure, delay, vehicle, sb.to);
                    Console.WriteLine(strFormat);
                }
            } else
            {
                
            }
        }

        public static StationboardResponse GetStationboard()
        {
            var client = CreateClient();
            var requestStationboard = CreateRequestStationboard();
            var response = client.Get<StationboardResponse>(requestStationboard); // get json data
            return response;
        }

        public static RestClient CreateClient()
        {
            // TODO Max timeout?
            var options = new RestClientOptions("https://transport.opendata.ch/v1");
            return new RestClient(options);
        }

        public static RestRequest CreateRequestStationboard()
        {
            RestRequest requestStationboard = new RestRequest("stationboard");
            requestStationboard.AddParameter("station", DEST_PLACE);
            requestStationboard.AddParameter("limit", LIMIT);
            requestStationboard.AddParameter("fields[]", "stationboard/stop/departure");
            requestStationboard.AddParameter("fields[]", "stationboard/stop/delay");
            requestStationboard.AddParameter("fields[]", "stationboard/category");
            requestStationboard.AddParameter("fields[]", "stationboard/number");
            requestStationboard.AddParameter("fields[]", "stationboard/to");
            return requestStationboard;
        }

        public static string ExtractDepartureTime(Stationboard sb) => DateTime.Parse(sb.stop.departure).ToString("HH:mm");

        public static string ExtractVehicle(Stationboard sb) => sb.category + sb.number;

        // TODO Stimmt diese Methode?
        public static string ExtractDelay(int? delay) => delay == null || delay == 0 ? "" :  "+" + delay;

        public record Stationboard(StationboardStop stop, string to, string number, string category); // stop => nested inside stationboard
        public record StationboardStop(string departure, int? delay); // delay nullable value type
        public record StationboardResponse(Stationboard[] stationboard);
    }
}
