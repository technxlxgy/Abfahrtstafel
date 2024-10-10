using RestSharp;

namespace Stationboard
{
    class Program
    {
        const string destPlace = "Zurich";
        const int limitNr = 3;

        static void Main(string[] args)
        {
            Console.WriteLine("### Abfahrtstafel ###");

            StationboardResponse response = GetStationboard();

            //string headingFormat = String.Format("{0, -20} {1, -20} {2, -20} {3, -20}", "Departure Time", "Dealy in Minutes", "Vehicle", "Destination");
            //Console.WriteLine(headingFormat);

            // iterates over every object in stationboard
            foreach (Stationboard sb in response.stationboard)
            {
                string departure = ExtractDepartureTime(sb);
                string delay = ExtractDelay(sb.stop.delay);
                string vehicle = ExtractVehicle(sb);

                string strFormat = String.Format("{0} {1, -5} {2, -10} {3, -20}", departure, delay, vehicle, sb.to);
                Console.WriteLine(strFormat);
            }
        }

        private static StationboardResponse GetStationboard()
        {
            var client = CreateClient();
            var request = CreateRequest();
            var response = client.Get<StationboardResponse>(request); // get json data
            return response;
        }

        private static RestClient CreateClient()
        {
            // TODO Max timeout?
            var options = new RestClientOptions("https://transport.opendata.ch/v1");
            return new RestClient(options);
        }

        private static RestRequest CreateRequest() {
            RestRequest request = new RestRequest("stationboard");
            request.AddParameter("station", destPlace); // TODO In Konstante speichern
            request.AddParameter("limit", limitNr); // TODO In Konstante speichern
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

        private record Stationboard(StationboardStop stop, string to, string number, string category); // stop => nested inside stationboard
        private record StationboardStop(string departure, int? delay); // delay nullable value type
        private record StationboardResponse(Stationboard[] stationboard);
    }
}
