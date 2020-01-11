using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ServiceStack.Redis;

namespace RedisApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieFlixController : ControllerBase
    { 
        private const string REDIS_SERVER_CONNECTION = "localhost:6379";
        private const string TOP_PELIS_SORTEDSET = "TOP10";

        [HttpGet]
        public string Get()
        {
            var manager = new RedisManagerPool(REDIS_SERVER_CONNECTION);

            using (var client = manager.GetClient())
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
            var manager = new RedisManagerPool(REDIS_SERVER_CONNECTION);
            using (var client = manager.GetClient())
            {
                var listado = client.GetRangeWithScoresFromSortedSetDesc(TOP_PELIS_SORTEDSET, 0, 9);
                var primer_listado = String.Join('\n', listado);

                return primer_listado;
            }
        }


        [HttpPost("ViewMovie")]
        public void ViewMovie([FromBody] string movieId)
        {
            if (string.IsNullOrEmpty(movieId)) return;

            var manager = new RedisManagerPool(REDIS_SERVER_CONNECTION);
            using (var client = manager.GetClient())
            {
                var listado = client.IncrementItemInSortedSet(TOP_PELIS_SORTEDSET, movieId, 1);
            }
        }
    }

}
