using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ServiceStack.Redis;
using System.Collections.Generic;
using MovieFlix.Application;
using MovieFlix.Application.Services;

namespace MovieFlix.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieFlixController : ControllerBase
    {
        private RecommendationService recommendationService;

        private const string REDIS_SERVER_CONNECTION = "localhost:6379";
        private const string TOP_PELIS_SORTEDSET = "TOP10";

        private readonly RedisManagerPool Manager = new RedisManagerPool(REDIS_SERVER_CONNECTION);

        public MovieFlixController()
        {
            recommendationService = new RecommendationService();
        }

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
            recommendationService.UpdateRecommendations(movieVisualization);
        }
    }
}
