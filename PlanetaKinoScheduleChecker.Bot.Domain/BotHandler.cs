using System;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess;
using PlanetaKinoScheduleChecker.Domain.Abstract;
using PlanetaKinoScheduleChecker.Domain.Implementation;
using RestSharp;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace PlanetaKinoScheduleChecker.Bot.Domain
{
    public class BotHandler
    {
        private static ITelegramBotClient _bot;
        private static readonly IMovieRepository _movieRepository = new MovieRepository();
        private static readonly IUserSubscriptionRepository _subscriptionRepository = new UserSubscriptionRepository();
        private static readonly IMovieChecker _movieChecker = new MovieChecker(new MovieCheckerClient(new RestClient(@"https://planetakino.ua"), new CinemaInfoParser()));
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BotHandler));

        const string intro = @"Welcome to PlanetaKino Schedule checker bot.
                              You can subscribe for checking when ticket sales start for your favorite movie.
                              Please use command /subscribe {movie  title from planetakino web}, you will automatically after you will be notified
                              /checksubscribtion - shows all your subscriptions
                              /unsubscribe  {movie  title from planetakino web} unsubscripbe you from you subscription";

        private static async void BotOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.TextMessage) return;

            _logger.Info($"Message recived {message.Chat.Id}");

            if (message.Text.StartsWith("/start"))
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                await _bot.SendTextMessageAsync(message.Chat.Id, intro);
            }
            else if (message.Text.StartsWith("/subscribe"))
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                var movieName = message.Text.Replace("/subscribe", "");

                if (String.IsNullOrWhiteSpace(movieName))
                    await _bot.SendTextMessageAsync(message.Chat.Id, $"Pick a Movie: {Environment.NewLine} {GetMovieList()}");
                else
                    await SubscribeUserForMovie(message.Chat.Id, movieName.Trim(' '));
            }
            else
            {
                await _bot.SendTextMessageAsync(message.Chat.Id, intro);
            }
        }

        private static string GetMovieList()
        {
            return string.Join($", {Environment.NewLine}", _movieChecker.GetCinemaInfo().Movies.Select(x => x.Title).ToArray());
        }

        private static Task SubscribeUserForMovie(long chatId, string trim)
        {
            var movieId = _movieChecker.GetMovieId(trim);
            if (movieId.HasValue)
            {
                var movie = _movieRepository.GetMovie(movieId.Value);
                if (movie == null)
                    _movieRepository.AddMovie(new Movie() { MovieId = movieId.Value, Title = trim, EndDate = DateTime.Now, StartDate = DateTime.Now });

                _subscriptionRepository.Add(new UserSubscription() { ChatId = chatId, MovieId = movieId.Value });
                _logger.Info($"Subsctiontion added for Movie { movieId.Value} {trim} and User {chatId}");
            }

            return Task.FromResult(0);
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await _bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }

        public static void InitalizeBot()
        {
            _bot = new TelegramBotClient(ConfigurationManager.AppSettings["token"]);
            _bot.OnMessage += BotOnOnMessage;
            _bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            _logger.Info($"Bot initialized {_bot.GetMeAsync().Result.Username}");
        }

        public static void MovieCheckerOnOnRelease(object sender, MoveRealesReleaseArgs args)
        {
            var subs = _subscriptionRepository.GetAllByMovieId(args.MovieId);

            _logger.Info($"Started sending notification for movie {args.MovieId} subs count {subs.Count()}");

            foreach (var userSubscription in subs)
            {
                SendNotification(userSubscription);
            }
            _logger.Info($"Finished sending notification for movie {args.MovieId} subs count {subs.Count()}");
        }

        private static void SendNotification(UserSubscription userSubscription)
        {
            _logger.Info($"Started sending notification for movie {userSubscription.MovieId} for user {userSubscription.ChatId}");

            _bot.SendTextMessageAsync(userSubscription.ChatId, GenerateText(userSubscription.MovieId));
            userSubscription.IsNotified = true;
            _subscriptionRepository.Update(userSubscription);

            _logger.Info($"Finished sending notification for movie {userSubscription.MovieId} for user {userSubscription.ChatId}");
        }

        private static string GenerateText(int movieId)
        {
            var sb = new StringBuilder();
            sb.Append($"Начался старт продаж на кино {_movieRepository.GetMovie(movieId).Title}");
            sb.Append(Environment.NewLine);
            sb.Append($"Доступные сеансы: /checkshowtime {movieId}");

            return sb.ToString();
        }

        public static string GetName()
        {
            return _bot.GetMeAsync().Result.Username;
        }

        public static void StartReceiving()
        {
            _logger.Debug("Started BOT");
            _bot.StartReceiving();
        }

        public static void StopReceiving()
        {
            _logger.Debug("Stoped BOT");
            _bot.StopReceiving();
        }
    }
}