using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PlanetaKinoScheduleChecker.Domain.Abstract;
using PlanetaKinoScheduleChecker.Domain.Implementation;
using PlanetaKinoScheduleChecker.Domain.Models;

namespace PlanetaKinoScheduleChecker.Tests
{
    [TestFixture()]
    public class CinemaInfoParserTest
    {
        private readonly ICinemaInfoParser _cinemaInfoParser = new CinemaInfoParser();
        private CinemaInfo _cinemaInfo;

        [Test]
        public void CinemaInfoParser_should_parse()
        {
            var directory = TestContext.CurrentContext.TestDirectory;
            var input = File.ReadAllText($@"{directory}\XmlInput.txt");

            var res = _cinemaInfoParser.TryParseCinemaInfo(input, out _cinemaInfo);
            Assert.True(res);
            Assert.That(_cinemaInfo.City, Is.EqualTo("kiev"));
            Assert.That(_cinemaInfo.Language, Is.EqualTo("ua"));
            Assert.That(_cinemaInfo.Movies, !Is.Null);
            Assert.That(_cinemaInfo.Movies.Count(), Is.EqualTo(Regex.Matches(input, "</movie>").Count));
            Assert.That(_cinemaInfo.Movies.Any(x => x.MovieId == 2640), Is.True);
            Assert.That(_cinemaInfo.Movies.Any(x => x.Title == "Валеріан та місто тисячі планет (12+)"), Is.True);
            Assert.That(_cinemaInfo.ShowTimes, !Is.Null);
            Assert.That(_cinemaInfo.ShowTimes.Any(x => x.MovieId == 2737), Is.True);
            Assert.That(_cinemaInfo.ShowTimes.Any(x => x.MovieId == 2640), Is.True);
            Assert.That(_cinemaInfo.ShowTimes.Any(x => x.MovieId == 2769), Is.True);
            Assert.That(_cinemaInfo.ShowTimes.Any(x => x.HallId == 218), Is.True);
            Assert.That(_cinemaInfo.ShowTimes.Any(x => x.HallId == 216), Is.True);
            Assert.That(_cinemaInfo.ShowTimes.Any(x => x.HallId == 16), Is.True);
            Assert.That(_cinemaInfo.ShowTimes.Any(x => x.Technology == "imax-3d"), Is.True);
            Assert.That(_cinemaInfo.ShowTimes.Any(x => x.FullDate == DateTime.Parse("2017-10-17 10:00:00")), Is.True);
            Assert.That(_cinemaInfo.ShowTimes.Any(x => x.FullDate == DateTime.Parse("2017-10-19 20:00:00")), Is.True);
            Assert.That(_cinemaInfo.ShowTimes.Count(), Is.EqualTo(Regex.Matches(input, "<show full-date").Count));
        }
    }
}