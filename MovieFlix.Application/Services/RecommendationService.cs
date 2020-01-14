using Microsoft.Extensions.Configuration;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace MovieFlix.Application.Services
{
    public class RecommendationService
    {
        private readonly IConfiguration configuration;

        public RecommendationService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetTopPelis()
        {
            using (var client = new RedisNativeClient(this.configuration.GetSection("RedisServerConnection").Value))
            {
                var bytesArray = client.ZRevRange(this.configuration.GetSection("TopPelisSortedSet").Value, 0, 9);
                List<string> listado = new List<string>();
                foreach (var bytes in bytesArray)
                {
                    String str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    listado.Add(str);
                }

                return String.Join(configuration.GetSection("MovieSeparator").Value, listado);
            }
        }

        public string GetUserRecommendedMovies(string userId)
        {
            using (var client = new RedisNativeClient(this.configuration.GetSection("RedisServerConnection").Value))
            {
                var userRecommendations = client.HGet(this.configuration.GetSection("UsersRecommendationsHash").Value, Encoding.UTF8.GetBytes(userId));
                return Encoding.UTF8.GetString(userRecommendations, 0, userRecommendations.Length);
            }
        }

        public void UpdateRecommendations(MovieVisualization movieVisualization)
        {
            using (var client = new RedisNativeClient(this.configuration.GetSection("RedisServerConnection").Value))
            {
                //Incremento en TOP Pelis
                client.ZIncrBy(configuration.GetSection("TopPelisSortedSet").Value, 1, Encoding.UTF8.GetBytes(movieVisualization.MovieName));
                //client.IncrementItemInSortedSet(configuration.GetSection("TopPelisSortedSet").Value, movieVisualization.MovieName, 1);

                //Incremento en TOP Pelis por género
                client.ZIncrBy(movieVisualization.GenreName, 1, Encoding.UTF8.GetBytes(movieVisualization.MovieName));
                //client.IncrementItemInSortedSet(movieVisualization.GenreName, movieVisualization.MovieName, 1);
            }

            InsertVisualization(movieVisualization);
            UpdateCustomUserRecommendations(movieVisualization.UserId);
        }

        public MovieVisualization GetRandomMovieVisualization()
        {
            DataTable movieTable = new DataTable();
            MovieVisualization movieVisualization = new MovieVisualization();

            string connectionString = configuration.GetConnectionString("DefaultConnection");

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

        private void InsertVisualization(MovieVisualization movieVisualization)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                string insertQuery = "INSERT INTO [MovieFlix].[dbo].[visualizations] values ( @UserID , @MovieID , @Date )";

                using (SqlCommand sqlCmd = new SqlCommand(insertQuery, sqlConn))
                {
                    sqlCmd.Parameters.Add("@UserID", SqlDbType.NVarChar, 24).Value = movieVisualization.UserId;
                    sqlCmd.Parameters.Add("@MovieID", SqlDbType.Int).Value = movieVisualization.MovieId;
                    sqlCmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = DateTime.Now;

                    sqlCmd.CommandType = CommandType.Text;
                    sqlConn.Open();
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateCustomUserRecommendations(string userId)
        {
            Dictionary<string, int> userMovieVisualizationsGroupedByGenre = GetUserMovieVisualizationsGroupedByGenre(userId);
            var customUserRecommendationsCountByGenre = CalculateCustomUserRecommendations(userMovieVisualizationsGroupedByGenre);

            using (var client = new RedisNativeClient(this.configuration.GetSection("RedisServerConnection").Value))
            {
                var customUserRecommendations = new List<string>();

                foreach (var genreRecommendationsCount in customUserRecommendationsCountByGenre)
                {
                    var bytesArray = client.ZRevRange(genreRecommendationsCount.Key, 0, -1);
                    //var genreRecommendations = client.GetAllItemsFromSortedSetDesc(genreRecommendationsCount.Key);


                    List<string> genreRecommendations = new List<string>();
                    foreach (var bytes in bytesArray)
                    {
                        String str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                        genreRecommendations.Add(str);
                    }


                    var userMovieVisualizations = GetUserMovieVisualizations(userId);
                    var count = 0;
                    foreach (var movie in genreRecommendations)
                    {
                        if (!userMovieVisualizations.Contains(movie))
                        {
                            customUserRecommendations.Add(movie);
                            count++;
                        }

                        if (count == genreRecommendationsCount.Value)
                            break;
                    }
                }


                var result = client.HSet(configuration.GetSection("UsersRecommendationsHash").Value, Encoding.UTF8.GetBytes(userId), Encoding.UTF8.GetBytes(String.Join(configuration.GetSection("MovieSeparator").Value, customUserRecommendations)));
                //client.SetEntryInHash(configuration.GetSection("UsersRecommendationsHash").Value, userId, String.Join(configuration.GetSection("MovieSeparator").Value, customUserRecommendations));
            }
        }

        private Dictionary<string, int> GetUserMovieVisualizationsGroupedByGenre(string userId)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            DataTable userMovieVisualizationsGroupedByGenreTable = new DataTable();

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                string selectQuery = "SELECT M.[GenreName], COUNT(*) AS [GenreCount] FROM [MovieFlix].[dbo].[visualizations] AS V LEFT JOIN [MovieFlix].[dbo].[movies] AS M ON V.[MovieId] = M.[MovieId] WHERE V.[UserId] = @UserID GROUP BY M.[GenreName] ORDER BY 2 DESC";                

                using (SqlCommand sqlCmd = new SqlCommand(selectQuery, sqlConn))
                {
                    sqlCmd.Parameters.Add("@UserID", SqlDbType.NVarChar, 24).Value = userId;

                    sqlCmd.CommandType = CommandType.Text;
                    sqlConn.Open();
                    using (SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd))
                    {
                        sqlAdapter.Fill(userMovieVisualizationsGroupedByGenreTable);
                    }
                }
            }

            Dictionary<string, int> userMovieVisualizationsGroupedByGenre = new Dictionary<string, int>();
            foreach (DataRow row in userMovieVisualizationsGroupedByGenreTable.Rows)
            {
                userMovieVisualizationsGroupedByGenre.Add(row["GenreName"].ToString(), int.Parse(row["GenreCount"].ToString()));
            }

            return userMovieVisualizationsGroupedByGenre;
        }

        private HashSet<string> GetUserMovieVisualizations(string userId)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            DataTable userMovieVisualizationsTable = new DataTable();

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                string selectQuery = "SELECT DISTINCT(M.[MovieName]) FROM [MovieFlix].[dbo].[visualizations] AS V LEFT JOIN [MovieFlix].[dbo].[movies] AS M ON V.[MovieId] = M.[MovieId] WHERE V.[UserId] = @UserID";

                using (SqlCommand sqlCmd = new SqlCommand(selectQuery, sqlConn))
                {
                    sqlCmd.Parameters.Add("@UserID", SqlDbType.NVarChar, 24).Value = userId;

                    sqlCmd.CommandType = CommandType.Text;
                    sqlConn.Open();
                    using (SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd))
                    {
                        sqlAdapter.Fill(userMovieVisualizationsTable);
                    }
                }
            }

            HashSet<string> userMovieVisualizations = new HashSet<string>();
            foreach (DataRow row in userMovieVisualizationsTable.Rows)
            {
                userMovieVisualizations.Add(row["MovieName"].ToString());
            }

            return userMovieVisualizations;
        }

        private Dictionary<string, int> CalculateCustomUserRecommendations(Dictionary<string, int> userMovieVisualizationsGroupedByGenre)
        {
            int vizualizationsCount = userMovieVisualizationsGroupedByGenre.Sum(x => x.Value);

            Dictionary<string, int> genreScores = new Dictionary<string, int>();

            var customUserRecommendationsLimit = int.Parse(configuration.GetSection("CustomUserRecommendationsLimit").Value);
            var recommendationsCount = 0;
            foreach (var genre in userMovieVisualizationsGroupedByGenre.Keys)
            {
                if (recommendationsCount >= customUserRecommendationsLimit)
                    break;

                var genreRecommendation = ((userMovieVisualizationsGroupedByGenre[genre] * (double)100 / vizualizationsCount) * customUserRecommendationsLimit) / 100;
                var genreRecommendationRounded = (int)Math.Round(genreRecommendation, 0);
                recommendationsCount += genreRecommendationRounded;

                if (genreRecommendationRounded > 0)
                    genreScores.Add(genre, genreRecommendationRounded);
            }

            while (recommendationsCount > customUserRecommendationsLimit)
            {
                var lastKey = genreScores.Keys.Last();

                if (genreScores[lastKey] - 1 == 0)
                    genreScores.Remove(lastKey);
                else
                    genreScores[lastKey] -= 1;

                recommendationsCount--;
            }
            while (recommendationsCount < customUserRecommendationsLimit)
            {
                var firstKey = genreScores.Keys.First();
                genreScores[firstKey] += 1;

                recommendationsCount++;
            }

            return genreScores;
        }
    }
}
