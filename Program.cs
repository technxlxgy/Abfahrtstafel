using RestSharp;
using System;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Threading;

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
            request.AddParameter("fields[]", "stationboard/stop/to");
            request.AddParameter("fileds[]", "stationboard/stop/name");
            request.AddParameter("fileds[]", "stationboard/stop/category");
            request.AddParameter("fileds[]", "stationboard/stop/number");

            var response = client.Get(request);

            Console.WriteLine("Type: {0}", response.ContentType);
            Console.WriteLine("Content: {0}", response.Content);
        }
    }
}
