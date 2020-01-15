using Microsoft.AspNetCore.Mvc;
using MovieFlix.Application;
using MovieFlix.Application.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System;
using System.IO;

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
            //MovieVisualization movieVisualization = new MovieVisualization()
            //{
            //    UserId = "bizu@correo.com",
            //    GenreName = "Action",
            //    MovieId = 592,
            //    MovieName = "Batman (1989)"
            //};

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < 1000000; i++)
            {
                MovieVisualization movieVisualization = recommendationService.GetRandomMovieVisualization();
                recommendationService.UpdateRecommendations(movieVisualization);
            }

            stopwatch.Stop();

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(Environment.CurrentDirectory + "\\Lib\\Redis64", "Tiempo.txt")))
            {
                var a = stopwatch.Elapsed.ToString();
                outputFile.WriteLine(a);
            }

            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
        }
    }
}
