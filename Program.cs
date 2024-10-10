using RestSharp;
using System.Reflection.Metadata.Ecma335;

namespace Stationboard
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("### Abfahrtstafel ###");

            var options = new RestClientOptions("https://transport.opendata.ch/v1");
            var client = new RestClient(options);
            var request = new RestRequest("stationboard");
            request.AddParameter("station", "Romanshorn");
            request.AddParameter("limit", 5);
            request.AddParameter("fields[]", "stationboard/stop/departure");
            request.AddParameter("fields[]", "stationboard/stop/delay");
            request.AddParameter("fields[]", "stationboard/category");
            request.AddParameter("fields[]", "stationboard/number");
            request.AddParameter("fields[]", "stationboard/to");

            var response = client.Get<StationboardResponse>(request); // get json data

            // iterates over every object in stationboard
            foreach (Stationboard sb in response.stationboard) {              
                DateTime dt = DateTime.Parse(sb.stop.departure);
                
                string departure = dt.ToString("HH:mm");
                string delay = ExtractDelay(sb.stop.delay);
                string vehicle = sb.category + sb.number;
                string destination = sb.to;

                Console.WriteLine("{0} {1} {2} {3}", departure, delay, vehicle, destination);
            }
        }

        // methode verbessern (einfacher, kürzer?)
        private static string ExtractDelay(int? delay) => delay == null || delay == 0 ? "" :  "+" + delay;
        /*{
            if (delay == null)
            {
                return "";
            } else if (delay == 0)
            {
                return "";
            } else
            {
                return "+" + delay;
            }
        }*/

        private record Stationboard(StationboardStop stop, string to, int number, string category); // stop => nested inside stationboard
        private record StationboardStop(string departure, int? delay); // delay nullable value type
        private record StationboardResponse(Stationboard[] stationboard);
    }
}
