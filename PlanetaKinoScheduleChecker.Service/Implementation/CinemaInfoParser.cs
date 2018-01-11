using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.Service.Abstract;
using PlanetaKinoScheduleChecker.Service.Models;

namespace PlanetaKinoScheduleChecker.Service.Implementation
{
    public class CinemaInfoParser : ICinemaInfoParser
    {
        public bool TryParseCinemaInfo(string input, out CinemaInfo cinemaInfo)
        {
            var parseResult = true;
            var root = XElement.Parse(input);
            var elements = root.Elements();
            var city = elements.SingleOrDefault(x => x.Name == "city");
            var language = elements.SingleOrDefault(x => x.Name == "lang");
            var movieElements = elements.SingleOrDefault(x => x.Name == "movies").Elements();
            var showTimeElements = elements.SingleOrDefault(x => x.Name == "showtimes").Elements();
            var movies = ParseMovies(movieElements);
            var showTimes = ParseShowTimes(showTimeElements);
            cinemaInfo = new CinemaInfo() { City = city.Value, Language = language.Value, Movies = movies, ShowTimes = showTimes };
            return parseResult;
        }

        private IEnumerable<ShowTime> ParseShowTimes(IEnumerable<XElement> showTimeElements)
        {
            var lst = new List<ShowTime>();
            foreach (var element in showTimeElements)
            {
                foreach (var showElemens in element.Elements().Where(x=>x.Name=="show"))
                {
                    var fullDate = DateTime.Parse(showElemens.Attribute("full-date").Value);
                    var cinemaName = showElemens.Attribute("theatre-id").Value;
                    var movieId = int.Parse(showElemens.Attribute("movie-id").Value);
                    var hallId = int.Parse(showElemens.Attribute("hall-id").Value);
                    var technology = showElemens.Attribute("technology").Value;
                    var orderUrl = showElemens.Attribute("order-url").Value;

                    lst.Add(new ShowTime() { FullDate = fullDate, HallId = hallId, MovieId = movieId, OrderUrl = orderUrl, Technology = technology, Theatre = cinemaName });
                }
            }
            return lst;
        }

        private IEnumerable<Movie> ParseMovies(IEnumerable<XElement> movieElements)
        {
            foreach (var element in movieElements)
            {
                var id = Int32.Parse(element.Attribute("id").Value);
                var title = element.Elements().SingleOrDefault(e => e.Name == "title").Value;
                var start = DateTime.Parse(element.Elements().SingleOrDefault(e => e.Name == "dt-start").Value);
                var end = DateTime.Parse(element.Elements().SingleOrDefault(e => e.Name == "dt-end").Value);
                var movie = new Movie
                {
                    CinemaMovieId = id,
                    Title = title,
                    EndDate = end,
                    StartDate = start
                };
                yield return movie;
            }
        }
    }
}