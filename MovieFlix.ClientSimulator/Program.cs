using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace UserActionsSimulator
{
    class UserVisitsGenerator
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Generando película vista");

            var client = new HttpClient();
            string movieId = "60";
            client.PostAsync("https://localhost:44317/movieflix/ViewMovie", new StringContent(JsonSerializer.Serialize(movieId), Encoding.UTF8, "application/json"));

            Console.ReadLine();
        }
    }
}
