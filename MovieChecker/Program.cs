using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using PlanetaKinoScheduleChecker.Bot.Domain.Models;
using PlanetaKinoScheduleChecker.Domain.Implementation;
using PlanetaKinoScheduleChecker.Domain.Models;
using RestSharp;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace PlanetaKinoScheduleChecker
{
    class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("311548579:AAGmk7_rSoA6dl95F1BQWdVaQV1Qgjp-lU0");
        private static readonly ConcurrentDictionary<int, Cinema> _dictionary = new ConcurrentDictionary<int, Cinema>();

        static void Main(string[] args)
        {
            Bot.OnMessage += BotOnOnMessage;

            var me = Bot.GetMeAsync().Result;


            Console.Title = me.Username;

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();

            //var movieChecker = new MovieChecker(new MovieCheckerClient(new RestClient(@"https://planetakino.ua"), new CinemaInfoParser()));
            //var movieTitle = "Тор: Раґнарок";
            //while (true)
            //{
            //    var res = movieChecker.CheckIfTicketsAvailiable(movieTitle);
            //    Console.WriteLine($"Searchin movie {movieTitle} and result is {res} at {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");

            //    if (res)
            //    {
            //        string body = GenerateBody(movieTitle, movieChecker.GetCinemaInfo());
            //        SendEmail("beloded87@gmail.com", body, movieTitle);
            //        SendEmail("devili4enko@gmail.com", body, movieTitle);
            //        break;
            //    }

            //    Thread.Sleep(TimeSpan.FromHours(1));
            //}
        }

        private static async void BotOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.TextMessage) return;

            if (message.Text.StartsWith("/start"))
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[] // first row
                    {
                       new KeyboardButton("Odessa"), 
                       new KeyboardButton("Lviv"), 
                       new KeyboardButton("Kiev"), 
                    }
                });

                await Bot.SendTextMessageAsync(message.Chat.Id, "Pick theatre", replyMarkup: keyboard);
            }


          
        }

        private static string GenerateBody(string movieTitle, CinemaInfo cinemaInfo)
        {
            var movieId = cinemaInfo.Movies.FirstOrDefault(x => x.Title == movieTitle).Id;
            var sb = new StringBuilder();
            sb.Append($"Начался старт продаж на кино {movieTitle}");
            sb.Append(Environment.NewLine);
            sb.Append($"Доступные сеансы: {Environment.NewLine}");
            foreach (var showTime in cinemaInfo.ShowTimes.OrderBy(x=>x.FullDate).Where(c=>c.MovieId== movieId).ToList())
            {
                sb.Append(
                    $"Дата:{showTime.FullDate.ToShortDateString()} Время: {showTime.FullDate.ToShortTimeString()} Ccылка для покупки билета: {showTime.OrderUrl} {Environment.NewLine}");
            }

            return sb.ToString();
        }

        private static void SendEmail(string email, string emailBody, string movieTitle)
        {
            var fromAddress = new MailAddress("oleg.dekhtiarenko@gmail.com", "From Name");
            var toAddress = new MailAddress(email);
            string fromPassword = ConfigurationManager.AppSettings["gmail"];
            string subject = $"Старт продаж билетов на {movieTitle} начался";
            string body = emailBody;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
