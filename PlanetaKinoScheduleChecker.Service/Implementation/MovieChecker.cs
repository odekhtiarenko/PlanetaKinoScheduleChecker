using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess.Abstract;
using PlanetaKinoScheduleChecker.DataAccess.Implementation;
using PlanetaKinoScheduleChecker.Service.Abstract;
using PlanetaKinoScheduleChecker.Service.Models;

namespace PlanetaKinoScheduleChecker.Service.Implementation
{
    public class MovieChecker : IMovieChecker
    {
        private readonly IMovieCheckerClient _movieCheckerClient;
        private readonly IMovieRepository _movieRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MovieChecker));

        public delegate void MovieReales(object sender, MoveRealesReleaseArgs args);

        public event MovieReales OnRelease;

        public MovieChecker(IMovieCheckerClient movieCheckerClient, IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
            _movieCheckerClient = movieCheckerClient;
        }

        public async void StartCheck()
        {
            _logger.Info("Started check for availiable tickets");
            while (true)
            {
                _logger.Info("Check");

                var movies = _movieRepository.GetAvailiableMovies();
                var enumerable = movies as Movie[] ?? movies.ToArray();
                if (enumerable.Length != 0)
                {
                    _logger.Info($"checking subs {enumerable.Count()}");
                    foreach (var movie in enumerable)
                    {
                        _logger.Info($"Start check for movie {movie.CinemaMovieId} {movie.Title}");

                        if (CheckIfTicketsAvailiable(movie.CinemaMovieId))
                        {
                            OnRelease?.Invoke(this, new MoveRealesReleaseArgs(movie.MovieId));
                            _logger.Info($"Movie {movie.Title} released");
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(0.3));
            }
        }

        public CinemaInfo GetCinemaInfo()
        {
            return _movieCheckerClient.GetCinemaInfo();
        }

        public int? GetMovieId(string movieTitle)
        {
            var cinemaInfo = _movieCheckerClient.GetCinemaInfo();

            if (cinemaInfo == null)
                throw new NullReferenceException();

            return cinemaInfo.Movies.SingleOrDefault(m => m.Title == movieTitle)?.CinemaMovieId;
        }

        public bool CheckIfTicketsAvailiable(string movieTitle)
        {
            var cinemaInfo = GetCinemaInfo();
            var movieId = cinemaInfo.Movies.SingleOrDefault(movie => movie.Title == movieTitle)?.CinemaMovieId;
            Console.WriteLine($"Movie title {movieTitle} with Id {movieId}");
            return cinemaInfo.ShowTimes.Any(show => show.MovieId == movieId);
        }

        private bool CheckIfTicketsAvailiable(int movieId)
        {
            var cinemaInfo = GetCinemaInfo();
            return cinemaInfo.ShowTimes.Any(show => show.MovieId == movieId);
        }
    }
}