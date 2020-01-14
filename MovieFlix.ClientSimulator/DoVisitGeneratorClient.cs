using MovieFlix.Application;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace UserActionsSimulator
{
    internal class UserVisitsGenerator
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Generando visitas a peliculas");

            var client = new HttpClient();
            for (var i = 0; i < 1; i++)
            {
                MovieVisualization movieVisualization = GetRandomPost();
                client.PostAsync("https://localhost:44317/movieflix/WatchMovie", new StringContent(JsonSerializer.Serialize(movieVisualization), Encoding.UTF8, "application/json"));
                Thread.Sleep(100);
            }

            Console.WriteLine("Termina de generar visitas a peliculas");
        }

        private static MovieVisualization GetRandomPost()
        {
            DataTable movieTable = new DataTable();
            MovieVisualization movieVisualization = new MovieVisualization();

            string connectionString = "Server=localhost;Database=MovieFlix;Trusted_Connection=True;MultipleActiveResultSets=true";

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                string selectMovieQuery = "SELECT TOP 1 * FROM [MovieFlix].[dbo].[movies] AS A, (SELECT TOP 1 * FROM [MovieFlix].[dbo].[users] ORDER BY NEWID()) AS B ORDER BY NEWID()";
                using (SqlCommand sqlCmd = new SqlCommand(selectMovieQuery, sqlConn))
                {
                    sqlCmd.CommandType = CommandType.Text;
                    sqlConn.Open();
                    using (SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd))
                    {
                        sqlAdapter.Fill(movieTable);
                    }
                }
            }

            foreach (DataRow row in movieTable.Rows)
            {
                movieVisualization.MovieId = int.Parse(row["MovieId"].ToString());
                movieVisualization.MovieName = row["MovieName"].ToString();
                movieVisualization.GenreName = row["GenreName"].ToString();
                movieVisualization.UserId = row["UserId"].ToString();
            }

            Console.WriteLine($"User: {movieVisualization.UserId} ha visto la película {movieVisualization.MovieId}: {movieVisualization.MovieName}");

            return movieVisualization;
        }
    }
}