﻿using System;
using Microsoft.AspNetCore.Mvc;
using ServiceStack.Redis;
using MovieFlix.Application;
using MovieFlix.Application.Services;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MovieFlix.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieFlixController : ControllerBase
    {
        private readonly RecommendationService recommendationService;

        public MovieFlixController(IConfiguration config)
        {
            recommendationService = new RecommendationService(config);
        }

        [HttpGet]
        public string Get()
        {
            //using (var client = Manager.GetClient())
            //{
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "3", 3);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "1", 1);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "7", 7);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "5", 5);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "11", 11);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "0", 0);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "17", 17);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "27", 27);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "54", 54);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "56", 56);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "55", 55);
            //    client.AddItemToSortedSet(TOP_PELIS_SORTEDSET, "60", 60);

            //    var listado = client.GetAllItemsFromSortedSet(TOP_PELIS_SORTEDSET);

            //    return "Filled";
            //}

            return "Filled";
        }

        [HttpGet("GetTopMovies")]
        public string GetTopMovies()
        {
            return recommendationService.GetTopPelis();
        }

        [HttpGet("GetUserRecommendedMovies/{userId}")]
        public string GetUserRecommendedMovies(string userId)
        {
            return recommendationService.GetUserRecommendedMovies(userId);
        }

        [HttpPost("WatchMovie")]
        public void WatchMovie([FromBody] MovieVisualization movieVisualization)
        {
            recommendationService.UpdateRecommendations(movieVisualization);
        }
    }
}
