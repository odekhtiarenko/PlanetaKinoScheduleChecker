using System;
using System.Threading.Tasks;
using PlanetaKinoScheduleChecker.Bot;
using PlanetaKinoScheduleChecker.DataAccess.Implementation;
using PlanetaKinoScheduleChecker.Service.Abstract;
using PlanetaKinoScheduleChecker.Service.Implementation;
using RestSharp;

namespace PlanetaKinoScheduleChecker
{
    internal class Program
    {
        private static readonly IMovieChecker _movieChecker = new MovieChecker(new MovieCheckerClient(new RestClient(@"https://planetakino.ua"), new CinemaInfoParser()), new MovieRepository());
        private static readonly IMovieCheckerBot _bot = new MovieCheckerBot();
        private static void Main()
        {
            _bot.InitalizeBot();

            Console.Title = _bot.GetName();

            _movieChecker.OnRelease += _bot.MovieCheckerOnOnRelease;
            Task.Factory.StartNew(_movieChecker.StartCheck);

            Console.ReadLine();
            _bot.StopReceiving();
        }
    }
}
