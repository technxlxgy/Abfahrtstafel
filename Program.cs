using RestSharp;
using System.Reflection.Metadata.Ecma335;

namespace Stationboard
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("### Abfahrtstafel ###");

            StationboardResponse response = GetStationboard();

            // iterates over every object in stationboard
            foreach (Stationboard sb in response.stationboard)
            {
                string departure = ExtractDepartureTime(sb);
                string delay = ExtractDelay(sb.stop.delay);
                string vehicle = ExtractVehicle(sb);

                // TODO Ausgabe formatieren (String.format)
                Console.WriteLine("{0} {1} {2} {3}", departure, delay, vehicle, sb.to);
            }
        }

        private static StationboardResponse GetStationboard()
        {
            var client = CreateClient();
            var request = CreateRequest();
            var response = client.Get<StationboardResponse>(request); // get json data
            // Fehler abfangen wenn 'response' null
            return response;
        }

        private static RestClient CreateClient()
        {
            var options = new RestClientOptions("https://transport.opendata.ch/v1");
            return new RestClient(options);
        }

        private static RestRequest CreateRequest() {
            RestRequest request = new RestRequest("stationboard");
            request.AddParameter("station", "Zurich");
            request.AddParameter("limit", 50);
            request.AddParameter("fields[]", "stationboard/stop/departure");
            request.AddParameter("fields[]", "stationboard/stop/delay");
            request.AddParameter("fields[]", "stationboard/category");
            request.AddParameter("fields[]", "stationboard/number");
            request.AddParameter("fields[]", "stationboard/to");
            return request;
        }

        private static string ExtractDepartureTime(Stationboard sb) => DateTime.Parse(sb.stop.departure).ToString("HH:mm");

        private static string ExtractVehicle(Stationboard sb) => sb.category + sb.number;

        // TODO Stimmt diese Methode?
        private static string ExtractDelay(int? delay) => delay == null || delay == 0 ? "" :  "+" + delay;

        private record Stationboard(StationboardStop stop, string to, int number, string category); // stop => nested inside stationboard
        private record StationboardStop(string departure, int? delay); // delay nullable value type
        private record StationboardResponse(Stationboard[] stationboard);
    }
}
