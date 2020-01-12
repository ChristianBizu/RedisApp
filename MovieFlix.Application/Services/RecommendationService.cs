using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovieFlix.Application.Services
{
    public class RecommendationService
    {
        private const string REDIS_SERVER_CONNECTION = "localhost:6379";
        private const string TOP_PELIS_SORTEDSET = "TOP10";

        private readonly RedisManagerPool Manager = new RedisManagerPool(REDIS_SERVER_CONNECTION);

        public void UpdateRecommendations(MovieVisualization movieVisualization)
        {
            using (var client = Manager.GetClient())
            {
                //Incremento en TOP Pelis
                client.IncrementItemInSortedSet(TOP_PELIS_SORTEDSET, movieVisualization.MovieName, 1);

                //Incremento en TOP Pelis por su género
                client.IncrementItemInSortedSet(movieVisualization.GenreName, movieVisualization.MovieName, 1);
            }

            UpdateCustomUserRecommendations(movieVisualization.UserId);
        }

        public void SaveViewsDb(MovieVisualization view)
        {
            /*
                HOLA JAMIE DEL FUTURO. 

                --------- ESTA FUNCION ME LA HABIA AÑADIDO EN UN DATA EN MovieFlix.API.
                            
                public const string USP_LP = "USP_LISTA_PELICULAS";

                //USP PARA DEVUELVER UNA LISTA CON TODOS LOS AEROPUERTOS.
                public string DevuelveListaPeliculas() 
                {            
                    DataTable dt = new DataTable();
                    using (SqlConnection sqlConn = new SqlConnection(Startup.SqlConnectionString))
                    {
                        using SqlCommand sqlCmd = new SqlCommand(USP_LP, sqlConn);
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlConn.Open();
                        using SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd);
                        sqlAdapter.Fill(dt);
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        string p = row["title"].ToString();
                    }

                    return null;
                }

                --------- ESTO LO DECLARABA EN EL CONTROLLER Y LO LLAMABA EN EL GET.

                public readonly PeliculasDbContext _peliculasContext;

                public MovieFlixController () {
                    _peliculasContext = new PeliculasDbContext();
                }



                _peliculasContext.DevuelveListaPeliculas();
            */
        }

        public Dictionary<string, int> UpdateCustomUserRecommendations(string userId)
        {
            Dictionary<string, int> genreViews = new Dictionary<string, int>() { { "Action", 247 }, { "Love", 60 }, { "Comedy", 10 }, }; // get genre views from bd

            int viewsCount = genreViews.Sum(x => x.Value);

            int totalRecomendations = 20;

            Dictionary<string, int> genreScores = new Dictionary<string, int>();

            foreach (var genre in genreViews.Keys)
            {
                var genreRecomedation = ((genreViews[genre] * (double)100 / viewsCount) * totalRecomendations) / 100;
                genreScores.Add(genre, (int)Math.Round(genreRecomedation, 0));
            }

            var recoCount = genreScores.Sum(x => x.Value);

            if (recoCount > totalRecomendations)
            {
                var lastKey = genreScores.Keys.Last();

                if (genreScores[lastKey] - 1 == 0)
                    genreScores.Remove(lastKey);
                else
                    genreScores[lastKey] -= 1;
            }

            return genreScores;
        }
    }
}
