using System.Configuration;
using PlanetaKinoScheduleChecker.Service.Abstract;
using PlanetaKinoScheduleChecker.Service.Models;
using RestSharp;

namespace PlanetaKinoScheduleChecker.Service.Implementation
{
    public class MovieCheckerClient : IMovieCheckerClient
    {
        private readonly IRestClient _client;
        private readonly ICinemaInfoParser _cinemaInfoParser;
        private readonly string _url = ConfigurationManager.AppSettings["planetakinoUrl"];

        public MovieCheckerClient(ICinemaInfoParser cinemaInfoParser)
        {
            _client = new RestClient(_url);
            _cinemaInfoParser = cinemaInfoParser;
        }

        public CinemaInfo GetCinemaInfo()
        {
            var request = new RestRequest(@"/showtimes/xml/", Method.GET);
            var infoString = _client.Get(request).Content;
            CinemaInfo cinemaInfo;
            _cinemaInfoParser.TryParseCinemaInfo(infoString, out cinemaInfo);
            return cinemaInfo;
        }
    }
}