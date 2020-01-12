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
        private const string REDIS_SERVER_CONNECTION = "localhost:6379";
        private const string TOP_PELIS_SORTEDSET = "TOP10";
        private const int CUSTOM_USER_RECOMMENDATIONS_LIMIT = 20;
        private const string USERS_RECOMMENDATIONS_HASH = "USERS_RECOMMENDATIONS";
        public const string USP_LP = "USP_LISTA_PELICULAS";
        private readonly RedisManagerPool Manager = new RedisManagerPool(REDIS_SERVER_CONNECTION);

        public void UpdateRecommendations(MovieVisualization movieVisualization)
        {
            using (var client = Manager.GetClient())
            {
                //Incremento en TOP Pelis
                client.IncrementItemInSortedSet(TOP_PELIS_SORTEDSET, movieVisualization.MovieName, 1);

                //Incremento en TOP Pelis por género
                client.IncrementItemInSortedSet(movieVisualization.GenreName, movieVisualization.MovieName, 1);
            }

            UpdateCustomUserRecommendations(movieVisualization.UserId);
        }

        public void SaveViewsDb()
        {
            DataTable dt = new DataTable();
            using (SqlConnection sqlConn = new SqlConnection("Server = CD - TOSH - 020419; Database = MovieFlix; Trusted_Connection = True; MultipleActiveResultSets = true"))
            {
                using (SqlCommand sqlCmd = new SqlCommand(USP_LP, sqlConn))
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlConn.Open();
                    using (SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd))
                    {
                        sqlAdapter.Fill(dt);
                    }
                }
            }

            List<string> listaP = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                listaP.Add(row["title"].ToString());
            }
        }

        public void UpdateCustomUserRecommendations(string userId)
        {
            //1. Recuperar el total de visualizaciones del usuario agrupadas por genero de BD
            Dictionary<string, int> userMovieVisualizationsGroupedByGenre = new Dictionary<string, int>() { { "Action", 247 }, { "Love", 60 }, { "Comedy", 10 }, };

            var customUserRecommendationsCountByGenre = CalculateCustomUserRecommendations(userMovieVisualizationsGroupedByGenre);

            using (var client = Manager.GetClient())
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

                client.SetEntryInHash(USERS_RECOMMENDATIONS_HASH, userId, String.Join(" ||| ", customUserRecommendations));
            }
        }

        private Dictionary<string, int> CalculateCustomUserRecommendations(Dictionary<string, int> userMovieVisualizationsGroupedByGenre)
        {
            int vizualizationsCount = userMovieVisualizationsGroupedByGenre.Sum(x => x.Value);

            Dictionary<string, int> genreScores = new Dictionary<string, int>();

            var recommendationsCount = 0;
            foreach (var genre in userMovieVisualizationsGroupedByGenre.Keys)
            {
                if (recommendationsCount >= CUSTOM_USER_RECOMMENDATIONS_LIMIT)
                    break;

                var genreRecommendation = ((userMovieVisualizationsGroupedByGenre[genre] * (double)100 / vizualizationsCount) * CUSTOM_USER_RECOMMENDATIONS_LIMIT) / 100;
                var genreRecommendationRounded = (int)Math.Round(genreRecommendation, 0);
                recommendationsCount += genreRecommendationRounded;

                if (genreRecommendationRounded > 0)
                    genreScores.Add(genre, genreRecommendationRounded);
            }

            if (recommendationsCount > CUSTOM_USER_RECOMMENDATIONS_LIMIT)
            {
                while (recommendationsCount > CUSTOM_USER_RECOMMENDATIONS_LIMIT)
                {
                    var lastKey = genreScores.Keys.Last();

                    if (genreScores[lastKey] - 1 == 0)
                        genreScores.Remove(lastKey);
                    else
                        genreScores[lastKey] -= 1;
                }
            }
            else if (recommendationsCount < CUSTOM_USER_RECOMMENDATIONS_LIMIT)
            {
                while (recommendationsCount < CUSTOM_USER_RECOMMENDATIONS_LIMIT)
                {
                    var firstKey = genreScores.Keys.First();
                    genreScores[firstKey] += 1;
                }
            }

            return genreScores;
        }
    }
}
