using Microsoft.Extensions.Configuration;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MovieFlix.Application.Services
{
    public class RecommendationService
    {
        private readonly IConfiguration configuration;
        private readonly RedisManagerPool manager;

        public RecommendationService(IConfiguration configuration)
        {
            this.configuration = configuration;
            manager = new RedisManagerPool(this.configuration.GetSection("RedisServerConnection").Value);
        }

        public string GetTopPelis()
        {
            using (var client = manager.GetClient())
            {
                var listado = client.GetRangeWithScoresFromSortedSetDesc(this.configuration.GetSection("TopPelisSortedSet").Value, 0, 9);
                return String.Join(configuration.GetSection("MovieSeparator").Value, listado);
            }
        }

        public string GetUserRecommendedMovies(string userId)
        {
            using (var client = manager.GetClient())
            {
                return client.GetValueFromHash(this.configuration.GetSection("UsersRecommendationsHash").Value, userId);
            }
        }

        public void UpdateRecommendations(MovieVisualization movieVisualization)
        {
            using (var client = manager.GetClient())
            {
                //Incremento en TOP Pelis
                client.IncrementItemInSortedSet(configuration.GetSection("TopPelisSortedSet").Value, movieVisualization.MovieName, 1);

                //Incremento en TOP Pelis por género
                client.IncrementItemInSortedSet(movieVisualization.GenreName, movieVisualization.MovieName, 1);
            }

            SaveViewsDb(movieVisualization);
            UpdateCustomUserRecommendations(movieVisualization.UserId);
        }

        private void SaveViewsDb(MovieVisualization movieVisualization)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                string insertQuery = "INSERT INTO [MovieFlix].[dbo].[views] values ( @UserID , @MovieID , @Date )";

                using (SqlCommand sqlCmd = new SqlCommand(insertQuery, sqlConn))
                {

                    sqlCmd.Parameters.Add("@UserID", SqlDbType.NVarChar, 24).Value = movieVisualization.UserId;
                    sqlCmd.Parameters.Add("@MovieID", SqlDbType.Int).Value = movieVisualization.MovieId;
                    sqlCmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = DateTime.Now;

                    sqlCmd.CommandType = CommandType.Text;
                    sqlConn.Open();

                    var response = sqlCmd.ExecuteNonQuery();

                    if (response > 0)
                        return;
                }
            }
        }

        private void UpdateCustomUserRecommendations(string userId)
        {
            //1. Recuperar el total de visualizaciones del usuario agrupadas por genero de BD
            Dictionary<string, int> userMovieVisualizationsGroupedByGenre = new Dictionary<string, int>() { { "Action", 247 }, { "Love", 60 }, { "Comedy", 10 }, };



            var customUserRecommendationsCountByGenre = CalculateCustomUserRecommendations(userMovieVisualizationsGroupedByGenre);

            using (var client = manager.GetClient())
            {
                var customUserRecommendations = new List<string>();

                foreach (var genreRecommendationsCount in customUserRecommendationsCountByGenre)
                {
                    //La dejo para depurar por si queremos ver que vienen ordenadas de mayor a menor
                    //var genreRecommendations = client.GetRangeWithScoresFromSortedSetDesc(genreRecommendationsCount.Key, 0, genreRecommendationsCount.Value);
                    var genreRecommendations = client.GetRangeFromSortedSetDesc(genreRecommendationsCount.Key, 0, genreRecommendationsCount.Value);
                    
                    //Si al final lo queremos hacer, aqui habría que limpiar las pelis que ya haya visto el usuario
                    
                    
                    customUserRecommendations.AddRange(genreRecommendations);
                }

                client.SetEntryInHash(configuration.GetSection("UsersRecommendationsHash").Value, userId, String.Join(configuration.GetSection("MovieSeparator").Value, customUserRecommendations));
            }
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
            }
            while (recommendationsCount < customUserRecommendationsLimit)
            {
                var firstKey = genreScores.Keys.First();
                genreScores[firstKey] += 1;
            }

            return genreScores;
        }
    }
}
