using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ServiceStack.Redis;
using System.Collections.Generic;
using MovieFlix.Application;

namespace MovieFlix.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieFlixController : ControllerBase
    { 
        private const string REDIS_SERVER_CONNECTION = "localhost:6379";
        private const string TOP_PELIS_SORTEDSET = "TOP10";

        private readonly RedisManagerPool Manager = new RedisManagerPool(REDIS_SERVER_CONNECTION);

        [HttpGet]
        public string Get()
        {
            using (var client = Manager.GetClient())
            {
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "3", 3);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "1", 1);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "7", 7);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "5", 5);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "11", 11);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "0", 0);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "17", 17);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "27", 27);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "54", 54);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "56", 56);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "55", 55);
                client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "60", 60);

                var listado = client.GetAllItemsFromSortedSet(TOP_PELIS_SORTEDSET);

                return "Filled";
            }
        }

        [HttpGet("GetTopPelis")]
        public string GetTopPelis()
        {            
            using (var client = Manager.GetClient())
            {
                var listado = client.GetRangeWithScoresFromSortedSetDesc(TOP_PELIS_SORTEDSET, 0, 9);
                var primer_listado = String.Join('\n', listado);

                return primer_listado;
            }
        }

        [HttpPost("WatchMovie")]
        public void WatchMovie([FromBody] MovieVisualization movieVisualization)
        {
            UpdateRecommendations(movieVisualization);
        }

        #region Private Methods
        private void UpdateRecommendations(MovieVisualization movieVisualization)
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

        private void SaveViewsDb(MovieVisualization view)
        {
            //insertamos la visita en SQL
        }

        private Dictionary<string, int> UpdateCustomUserRecommendations(string userId) 
        {
            Dictionary<string, int> genreViews = new Dictionary<string, int>() { { "Action", 247 }, { "Love", 60 }, { "Comedy", 10 }, }; // get genre views from bd

            int viewsCount = genreViews.Sum(x => x.Value);

            int totalRecomendations = 20;

            Dictionary<string, int> genreScores = new Dictionary<string, int>();

            foreach (var genre in genreViews.Keys) 
            {
                var genreRecomedation = ((genreViews[genre] * (double) 100 / viewsCount) * totalRecomendations) / 100;
                genreScores.Add(genre, (int) Math.Round(genreRecomedation, 0));
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
        #endregion
    }
}
