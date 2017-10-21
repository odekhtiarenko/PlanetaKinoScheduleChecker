using PlanetaKinoScheduleChecker.Domain.Abstract;
using PlanetaKinoScheduleChecker.Domain.Models;
using RestSharp;

namespace PlanetaKinoScheduleChecker.Domain.Implementation
{
    public class MovieCheckerClient : IMovieCheckerClient
    {
        private readonly IRestClient _client;
        private readonly ICinemaInfoParser _cinemaInfoParser;

        public MovieCheckerClient(IRestClient client, ICinemaInfoParser cinemaInfoParser)
        {
            _client = client;
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