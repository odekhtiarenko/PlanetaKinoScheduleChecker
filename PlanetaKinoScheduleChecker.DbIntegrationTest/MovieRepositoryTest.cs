using System;
using System.Linq;
using NUnit.Framework;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess;
using PlanetaKinoScheduleChecker.DataAccess.Abstract;
using PlanetaKinoScheduleChecker.DataAccess.Implementation;
using Ploeh.AutoFixture;

namespace PlanetaKinoScheduleChecker.DbIntegrationTest
{
    [TestFixture]
    public class MovieRepositoryTest
    {
        private readonly IMovieRepository _movieRepository = new MovieRepository();
        private readonly Fixture _fixture = new Fixture();

        [Test]
        public void Should_insert_movie()
        {

            var affectedRows = _movieRepository.AddMovie(
                new Movie()
                {
                    EndDate = DateTime.Now.AddDays(7),
                    CinemaMovieId = _fixture.Create<int>(),
                    StartDate = DateTime.Now,
                    Title = $"Movie Title {Guid.NewGuid()}"
                });

            Assert.That(affectedRows, Is.GreaterThan(0));

        }

        [Test]
        public void Should_GetAll_movies()
        {
            var movies = _movieRepository.GetMovies();
            Assert.That(movies, !Is.Null);
        }

        [Test]
        public void Should_Get_movie()
        {
            _movieRepository.AddMovie(
                new Movie()
                {
                    EndDate = DateTime.Now.AddDays(7),
                    CinemaMovieId = _fixture.Create<int>(),
                    StartDate = DateTime.Now,
                    Title = $"Movie Title {Guid.NewGuid()}"
                });

            _movieRepository.AddMovie(
                new Movie()
                {
                    EndDate = DateTime.Now.AddDays(7),
                    CinemaMovieId = _fixture.Create<int>(),
                    StartDate = DateTime.Now,
                    Title = $"Movie Title {Guid.NewGuid()}"
                });

            _movieRepository.AddMovie(
                new Movie()
                {
                    EndDate = DateTime.Now.AddDays(7),
                    CinemaMovieId = _fixture.Create<int>(),
                    StartDate = DateTime.Now,
                    Title = $"Movie Title {Guid.NewGuid()}"
                });

            var movies = _movieRepository.GetMovies();

            var expected = movies.ElementAt(new Random().Next(1, movies.Count()));

            var result = _movieRepository.GetMovieByExternalId(expected.CinemaMovieId);

            Assert.That(result.MovieId, Is.EqualTo(result.MovieId));
            Assert.That(result.EndDate, Is.EqualTo(result.EndDate));
            Assert.That(result.CinemaMovieId, Is.EqualTo(result.CinemaMovieId));
            Assert.That(result.StartDate, Is.EqualTo(result.StartDate));
            Assert.That(result.Title, Is.EqualTo(result.Title));
        }
    }
}
