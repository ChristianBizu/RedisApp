using Microsoft.AspNetCore.Mvc;
using MovieFlix.Application;
using MovieFlix.Application.Services;
using Microsoft.Extensions.Configuration;
using System;

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

        [HttpPost("Simulate")]
        public void Simulate()
        {
            Console.WriteLine("Generando visitas a peliculas");

            for (var i = 0; i < 1000; i++)
            {
                MovieVisualization movieVisualization = recommendationService.GetRandomMovieVisualization();

                recommendationService.UpdateRecommendations(movieVisualization);
            }
        }
    }
}
