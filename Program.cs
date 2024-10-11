using RestSharp;

using System.Security.Cryptography.X509Certificates;
//using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Stationboard
{
    class Program
    {
        public const string DEST_PLACE = "Zurich";
        public const int LIMIT = 3;

        static void Main(string[] args)
        {
            Console.WriteLine("Anzahl Argumente: {0}", args.Length);

            string station = DEST_PLACE;
            int limit = LIMIT;

            // TODO kürzer oder anders schreiben (unnötig viele Zeilen) + check if valid station angegeben !!
            if (args.Length > 0)
            {
                // check if any of the parsed arguments is of data type int
                bool isNumberOne = int.TryParse(args[0], out int valueOne);
                bool isNumberTwo = int.TryParse(args[1], out int valueTwo);

                if (isNumberOne)
                {
                    limit = Convert.ToInt32(args[0]);
                    Console.WriteLine(limit);
                }
                else if (isNumberTwo)
                {
                    limit = Convert.ToInt32(args[1]);
                    Console.WriteLine(limit);
                }

                // check if any of the parsed arguments is of data type string
                if (IsString(args[0]))
                {
                    station = args[0];
                }
                else if (IsString(args[1]))
                {
                    station = args[1];
                }
            }

            // TODO kürzer oder anders schreiben (unnötig viele Zeilen) 
                static bool IsString(string input)
                {
                    // Check if the input can be parsed to an integer
                    if (int.TryParse(input, out _))
                    {
                        return false;
                    }

                    // Check if the input can be parsed to a double
                    if (double.TryParse(input, out _))
                    {
                        return false;
                    }

                    // If all checks fail, the input is likely a string
                    return true;
                }

            Console.WriteLine("Station: {0}, Display {1} departues", station, limit);

            Console.WriteLine("### Abfahrtstafel ###");

            StationboardResponse response = GetStationboard(station, limit);

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
            // TODO Max timeout?
            var options = new RestClientOptions("https://transport.opendata.ch/v1");
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

        // TODO Stimmt diese Methode?
        public static string ExtractDelay(int? delay) => delay == null || delay == 0 ? "" : "+" + delay;

        public record Stationboard(StationboardStop stop, string to, string number, string category); // stop => nested inside stationboard
        public record StationboardStop(string departure, int? delay); // delay nullable value type
        public record StationboardResponse(Stationboard[] stationboard);
    }
}