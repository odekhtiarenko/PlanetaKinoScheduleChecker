using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using PlanetaKinoScheduleChecker.Bot.Domain;
using PlanetaKinoScheduleChecker.Bot.Domain.Models;
using PlanetaKinoScheduleChecker.Domain.Abstract;
using PlanetaKinoScheduleChecker.Domain.Implementation;
using RestSharp;

namespace PlanetaKinoScheduleChecker
{
    internal class Program
    {
        private static readonly IMovieChecker _movieChecker = new MovieChecker(new MovieCheckerClient(new RestClient(@"https://planetakino.ua"), new CinemaInfoParser()));

        private static void Main(string[] args)
        {
            BotHandler.InitalizeBot();

            Console.Title = BotHandler.GetName();

            _movieChecker.OnRelease += BotHandler.MovieCheckerOnOnRelease;
            Task.Factory.StartNew(_movieChecker.StartCheck);

            BotHandler.StartReceiving();
            Console.ReadLine();
            BotHandler.StopReceiving();
        }
    }
}
