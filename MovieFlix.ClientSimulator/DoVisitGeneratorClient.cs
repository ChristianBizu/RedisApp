using MovieFlix.Application;
using System;
using System.Data;
using System.Data.SqlClient;
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

            MovieVisualization visitModel = GetRandomPost();

            client.PostAsync("https://localhost:44317/movieflix/WatchMovie", new StringContent(JsonSerializer.Serialize(visitModel), Encoding.UTF8, "application/json"));

            Console.ReadLine();
        }

        private static MovieVisualization GetRandomPost()
        {
            DataTable movieTable = new DataTable();
            MovieVisualization movieVisualization = new MovieVisualization();

            string connectionString = "Server=DESKTOP-3O1UP4M\\SQLEXPRESS;Database=MovieFlix;Trusted_Connection=True;MultipleActiveResultSets=true";

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

            return movieVisualization;
        }        
    }
}
