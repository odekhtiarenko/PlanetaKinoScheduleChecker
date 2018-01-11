using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using PlanetaKinoScheduleChecker.DataAccess.Abstract;
using PlanetaKinoScheduleChecker.Service.Abstract;
using PlanetaKinoScheduleChecker.Service.Implementation;
using PlanetaKinoScheduleChecker.Service.Models;

namespace PlanetaKinoScheduleChecker.Tests
{
    [TestFixture]
    public class MovieCheckerTest
    {
        private IMovieChecker _movieChecker;
        private Mock<IMovieCheckerClient> _movieCheckerClientMock;
        private Mock<IMovieRepository> _movieRepositoryMock;
                                                       
        [SetUp]
        public void SetUp()
        {
            _movieCheckerClientMock = new Mock<IMovieCheckerClient>();
            _movieRepositoryMock = new Mock<IMovieRepository>();
_movieChecker = new MovieChecker(_movieCheckerClientMock.Object, _movieRepositoryMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void MovieChecker_shoul_return_cinema_entity()
        {
            _movieCheckerClientMock.Setup(client => client.GetCinemaInfo()).Returns(new CinemaInfo() { City = "Kiev" });
            var res = _movieChecker.GetCinemaInfo();
            Assert.IsNotNull(res, "Cinema info shouldnt be null");
            Assert.That(res.City, Is.EqualTo("Kiev"));
        }

        [Test]
        public void MovieChecker_shoul_return_true_if_tickets_for_movie_is_availiable()
        {
            CinemaInfo cinemaInfo;
            var moviename = "Голем";
            var directory = TestContext.CurrentContext.TestDirectory;
            var input = File.ReadAllText($@"{directory}\XmlInput.txt");
            new CinemaInfoParser().TryParseCinemaInfo(input, out cinemaInfo);
            _movieCheckerClientMock.Setup(client => client.GetCinemaInfo()).Returns(cinemaInfo);
            var res = _movieChecker.CheckIfTicketsAvailiable(moviename);
            Assert.AreEqual(res, false);
            ((IList<ShowTime>)cinemaInfo.ShowTimes).Add(new ShowTime(){FullDate = DateTime.Now.AddDays(2), MovieId = cinemaInfo.Movies.First(x=>x.Title==moviename).CinemaMovieId });
            _movieCheckerClientMock.Setup(client => client.GetCinemaInfo()).Returns(cinemaInfo);

            res = _movieChecker.CheckIfTicketsAvailiable(moviename);
            Assert.AreEqual(res, true);

        }
    }
}
