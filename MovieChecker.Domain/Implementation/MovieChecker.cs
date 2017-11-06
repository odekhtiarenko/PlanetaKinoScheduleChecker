using System;
using System.Linq;
using PlanetaKinoScheduleChecker.Domain.Abstract;
using PlanetaKinoScheduleChecker.Domain.Models;

namespace PlanetaKinoScheduleChecker.Domain.Implementation
{
    public class MovieChecker : IMovieChecker
    {
        private readonly IMovieCheckerClient _movieCheckerClient;

        public MovieChecker(IMovieCheckerClient movieCheckerClient)
        {
            _movieCheckerClient = movieCheckerClient;
        }
        public CinemaInfo GetCinemaInfo()
        {
            return _movieCheckerClient.GetCinemaInfo();
        }

        public int? GetMovieId(string movieTitle)
        {
            var cinemaInfo = _movieCheckerClient.GetCinemaInfo();

            return cinemaInfo.Movies.SingleOrDefault(m => m.Title == movieTitle).Id;
        }

        public bool CheckIfTicketsAvailiable(string movieTitle)
        {
            var cinemaInfo = GetCinemaInfo();
            var movieId = cinemaInfo.Movies.SingleOrDefault(movie => movie.Title == movieTitle).Id;
            Console.WriteLine($"Movie title {movieTitle} with Id {movieId}");
            return cinemaInfo.ShowTimes.Any(show => show.MovieId == movieId);
        }

        public bool CheckIfTicketsAvailiable(int movieId)
        {
            var cinemaInfo = GetCinemaInfo();
            return cinemaInfo.ShowTimes.Any(show => show.MovieId == movieId);
        }
    }
}