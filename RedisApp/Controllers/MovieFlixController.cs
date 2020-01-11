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
        [HttpGet]
        public string Get()
        {
            var manager = new RedisManagerPool("localhost:6379");
            using (var client = manager.GetClient())
            {
                string[] lis = { "Pepe", "Mera", "JimmyPuta" };

                client.Set("foo", lis);

                var res = client.Get<string[]>("foo");

                client.AddItemToSortedSet("top100", "3", 3);
                client.AddItemToSortedSet("top100", "1", 1);
                client.AddItemToSortedSet("top100", "7", 7);

                client.AddItemToSortedSet("top100", "5", 5);


                client.AddItemToSortedSet("top100", "11", 11);
                client.AddItemToSortedSet("top100", "0", 0);
                client.AddItemToSortedSet("top100", "17", 17);
                client.AddItemToSortedSet("top100", "27", 27);
                client.AddItemToSortedSet("top100", "54", 54);
                //pepe
                client.AddItemToSortedSet("top100", "56", 56);
                client.AddItemToSortedSet("top100", "55", 55);
                client.AddItemToSortedSet("top100", "60", 60);

                var listado = client.GetAllItemsFromSortedSet("top100");

                return "vuestra madre";
            }
        }

        [HttpGet("GetTopPelis")]
        public string GetTopPelis()
        {
            var manager = new RedisManagerPool("localhost:6379");
            using (var client = manager.GetClient())
            {
                var listado = client.GetRangeWithScoresFromSortedSetDesc("top100", 0, 9);
                var primer_listado = String.Join('\n', listado);

                return primer_listado;
            }
        }


        [HttpPost("ViewMovie")]
        public void ViewMovie([FromBody] string movieId)
        {
            if (string.IsNullOrEmpty(movieId)) return;

            var manager = new RedisManagerPool("localhost:6379");
            using (var client = manager.GetClient())
            {
                var listado = client.IncrementItemInSortedSet("top100", movieId, 1);
            }
        }
    }

}
