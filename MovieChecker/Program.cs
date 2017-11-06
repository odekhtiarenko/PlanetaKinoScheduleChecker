using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using PlanetaKinoScheduleChecker.Bot.Domain.Models;
using PlanetaKinoScheduleChecker.Domain.Abstract;
using PlanetaKinoScheduleChecker.Domain.Implementation;
using RestSharp;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace PlanetaKinoScheduleChecker
{
    class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("311548579:AAGmk7_rSoA6dl95F1BQWdVaQV1Qgjp-lU0");
        const string intro = @"Welcome to PlanetaKino Schedule checker bot.
                              You can subscribe for checking when ticket sales start for your favorite movie.
                              Please use command /subscribe {movie  title from planetakino web}, you will automatically after you will be notified
                              /checksubscribtion - shows all your subscriptions
                              /unsubscribe  {movie  title from planetakino web} unsubscripbe you from you subscription";

        private static readonly List<Subscription> _userSubscriptions = new List<Subscription>();
        private static readonly List<Subscription> _movieSubscriptions = new List<Subscription>();
        private static readonly IMovieChecker _movieChecker = new MovieChecker(new MovieCheckerClient(new RestClient(@"https://planetakino.ua"), new CinemaInfoParser()));


        static void Main(string[] args)
        {
            Bot.OnMessage += BotOnOnMessage;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;

            var me = Bot.GetMeAsync().Result;


            Console.Title = me.Username;

            Task.Factory.StartNew(StartCheck);
            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }

        private static async void StartCheck()
        {
            Console.WriteLine("start");
            while (true)
            {
                Console.WriteLine($"check {DateTime.Now}");
                if (_movieSubscriptions.Count != 0)
                {
                    Console.WriteLine($"checking subs {_movieSubscriptions.Count}");
                    foreach (var sub in _movieSubscriptions.Where(x => !x.IsNotyfied))
                    {
                        if (_movieChecker.CheckIfTicketsAvailiable(sub.MovieId))
                        {
                            Console.WriteLine($"Have subs for {sub.MovieId}");
                            sub.IsReleased = true;
                            await NotifyUsersThatMovieStarted(sub.MovieId);
                            sub.IsNotyfied = true;

                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(15));
            }
        }

        private static async Task NotifyUsersThatMovieStarted(int subMovieId)
        {
            await Task.Yield();

            foreach (var userSubscription in _userSubscriptions.Where(u => u.MovieId == subMovieId && !u.IsNotyfied))
            {
                await Bot.SendTextMessageAsync(userSubscription.ChatId, GenerateBody(userSubscription.MovieId));
                userSubscription.IsNotyfied = true;
                userSubscription.ShouldNotify = false;
            }
        }

        private static async Task NotifyUsersThatMovieStarted(int subMovieId, long chatId)
        {
            var userSubscription =
                _userSubscriptions.FirstOrDefault(u => u.ChatId == chatId && u.MovieId == subMovieId && !u.IsNotyfied);
            await Bot.SendTextMessageAsync(userSubscription.ChatId, GenerateBody(userSubscription.MovieId));
            userSubscription.IsNotyfied = true;
            userSubscription.ShouldNotify = false;
        }

        private static string GenerateBody(int movieId)
        {
            var cinemaInfo = _movieChecker.GetCinemaInfo();
            var sb = new StringBuilder();
            sb.Append($"Начался старт продаж на кино {cinemaInfo.Movies.SingleOrDefault(x => x.Id == movieId).Title}");
            sb.Append(Environment.NewLine);
            sb.Append($"Доступные сеансы: /checkshowtime ${movieId}");
            //foreach (var showTime in cinemaInfo.ShowTimes.OrderBy(x => x.FullDate).Where(c => c.MovieId == movieId).ToList())
            //{
            //    sb.Append(
            //        $"Дата:{showTime.FullDate.ToShortDateString()} Время: {showTime.FullDate.ToShortTimeString()} Ccылка для покупки билета: {showTime.OrderUrl} {Environment.NewLine}");
            //}

            return sb.ToString();
        }

        private static async void BotOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.TextMessage) return;

            if (message.Text.StartsWith("/start"))
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                await Bot.SendTextMessageAsync(message.Chat.Id, intro);
            }
            else if (message.Text.StartsWith("/subscribe"))
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                var movieName = message.Text.Replace("/subscribe", "");

                if (string.IsNullOrWhiteSpace(movieName))
                    await Bot.SendTextMessageAsync(message.Chat.Id, $"Pick a Movie: {GetMovieList()}");
                else
                    await SubscribeUserForMovie(message.Chat.Id, movieName.Trim(' '));
            }
            else
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, intro);
            }
        }

        private static async Task SubscribeUserForMovie(long chatId, string movieName)
        {
            var movieId = _movieChecker.GetMovieId(movieName);
            var subscription = new Subscription() { ChatId = chatId, MovieId = movieId.Value };
            if (_movieSubscriptions.All(x => x.MovieId != movieId))
                _movieSubscriptions.Add(subscription);

            _userSubscriptions.Add(subscription);

            await Bot.SendTextMessageAsync(chatId, $"You successfuly subscribed for movie {movieName}");
            await CheckIfMovieAlreadyRelesed(chatId, movieId.Value);
        }

        private static async Task CheckIfMovieAlreadyRelesed(long chatId, int movieIdValue)
        {
            if (_movieSubscriptions.Any(x => x.MovieId == movieIdValue && x.IsReleased))
                await NotifyUsersThatMovieStarted(movieIdValue, chatId);
        }

        private static string GetMovieList()
        {
            var cinemaInfo = _movieChecker.GetCinemaInfo();
            return string.Join($", {Environment.NewLine}", cinemaInfo.Movies.Select(m => m.Title));
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
