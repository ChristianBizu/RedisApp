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

                //movieVisualization.GenreName = "Action";
                //movieVisualization.MovieId = 95;
                //movieVisualization.MovieName = "Broken Arrow (1996)";
                //movieVisualization.UserId = "bizu@correo.com";

                client.PostAsync("https://localhost:44317/movieflix/WatchMovie", new StringContent(JsonSerializer.Serialize(movieVisualization), Encoding.UTF8, "application/json"));
                Thread.Sleep(1000);
            }
        }

        private static MovieVisualization GetRandomPost()
        {
            DataTable movieTable = new DataTable();
            MovieVisualization movieVisualization = new MovieVisualization();

            string connectionString = "Server=DESKTOP-NL46CV2;Database=MovieFlix;Trusted_Connection=True;MultipleActiveResultSets=true";

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                string selectMovieQuery = "SELECT TOP 1 * FROM[MovieFlix].[dbo].[movies] AS A, (SELECT TOP 1 * FROM[MovieFlix].[dbo].[users] ORDER BY NEWID()) AS B ORDER BY NEWID()";

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
                movieVisualization.MovieId = int.Parse(row["idMovie"].ToString());
                movieVisualization.MovieName = row["nameMovie"].ToString();
                movieVisualization.GenreName = row["genreMovie"].ToString();
                movieVisualization.UserId = row["idUser"].ToString();
            }

            Console.WriteLine($"User: {movieVisualization.UserId} ha visto la película {movieVisualization.MovieId}: {movieVisualization.MovieName}");

            return movieVisualization;
        }
    }
}