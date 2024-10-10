using RestSharp;

namespace Stationboard
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("### Abfahrtstabelle ###");

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

            var response = client.Get<StationboardResponse>(request);

            foreach (Stationboard sb in response.stationboard) {              
                DateTime dt = DateTime.Parse(sb.stop.departure);
                string delay = ExtractDelay(sb.stop.delay);
                Console.WriteLine("{0} {1} {2}{3} {4}", dt.ToString("HH:mm"), delay, sb.category, sb.number, sb.to);
            }
        }

        private static string ExtractDelay(int? delay)
        {
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
        }

        private record Stationboard(StationboardStop stop, string to, int number, string category);
        private record StationboardStop(string departure, int? delay);
        private record StationboardResponse(Stationboard[] stationboard);
    }
}
