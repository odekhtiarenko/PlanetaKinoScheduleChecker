using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess;
using Ploeh.AutoFixture;

namespace PlanetaKinoScheduleChecker.DbIntegrationTest
{
    [TestFixture]
    public class MovieRepositoryTest
    {
        private readonly IMovieRepository MovieRepository = new MovieRepository();
        private Fixture _fixture = new Fixture();

        [Test]
        public void Should_insert_movie()
        {

            var affectedRows = MovieRepository.AddMovie(
                new Movie()
                {
                    EndDate = DateTime.Now.AddDays(7),
                    MovieId = _fixture.Create<int>(),
                    StartDate = DateTime.Now,
                    Title = $"Movie Title {Guid.NewGuid()}"
                });

            Assert.That(affectedRows, Is.GreaterThan(0));

        }

        [Test]
        public void Should_GetAll_movies()
        {
            var movies = MovieRepository.GetMovies();

            Assert.That(movies, !Is.Null);
        }

        [Test]
        public void Should_Get_movie()
        {
            MovieRepository.AddMovie(
                new Movie()
                {
                    EndDate = DateTime.Now.AddDays(7),
                    MovieId = _fixture.Create<int>(),
                    StartDate = DateTime.Now,
                    Title = $"Movie Title {Guid.NewGuid()}"
                });

            MovieRepository.AddMovie(
                new Movie()
                {
                    EndDate = DateTime.Now.AddDays(7),
                    MovieId = _fixture.Create<int>(),
                    StartDate = DateTime.Now,
                    Title = $"Movie Title {Guid.NewGuid()}"
                });

            MovieRepository.AddMovie(
                new Movie()
                {
                    EndDate = DateTime.Now.AddDays(7),
                    MovieId = _fixture.Create<int>(),
                    StartDate = DateTime.Now,
                    Title = $"Movie Title {Guid.NewGuid()}"
                });

            var movies = MovieRepository.GetMovies();

            var expected = movies.ElementAt(new Random().Next(1, movies.Count()));

            var result = MovieRepository.GetMovie(expected.MovieId);

            Assert.That(result.Id, Is.EqualTo(result.Id));
            Assert.That(result.EndDate, Is.EqualTo(result.EndDate));
            Assert.That(result.MovieId, Is.EqualTo(result.MovieId));
            Assert.That(result.StartDate, Is.EqualTo(result.StartDate));
            Assert.That(result.Title, Is.EqualTo(result.Title));
        }
    }
}
