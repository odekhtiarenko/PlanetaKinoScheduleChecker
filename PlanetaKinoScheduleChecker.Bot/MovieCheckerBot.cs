using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess;
using PlanetaKinoScheduleChecker.DataAccess.Abstract;
using PlanetaKinoScheduleChecker.DataAccess.Implementation;
using PlanetaKinoScheduleChecker.Service.Abstract;
using PlanetaKinoScheduleChecker.Service.Implementation;
using RestSharp;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace PlanetaKinoScheduleChecker.Bot
{
    public class MovieCheckerBot : IMovieCheckerBot
    {
        private ITelegramBotClient _bot;
        private readonly IMovieRepository MovieRepository = new MovieRepository();
        private readonly IUserSubscriptionRepository SubscriptionRepository = new UserSubscriptionRepository();
        private readonly IMovieChecker MovieChecker = new MovieChecker(new MovieCheckerClient(new RestClient(@"https://planetakino.ua"), new CinemaInfoParser()), new MovieRepository());
        private readonly ILog Logger = LogManager.GetLogger(typeof(MovieCheckerBot));

        const string Intro = @"Welcome to PlanetaKino Schedule checker bot.
                              You can subscribe for checking when ticket sales start for your favorite movie.
                              Please use command /subscribe {movie  title from planetakino web}, you will automatically after you will be notified
                              /checksubscribtion - shows all your subscriptions
                              /unsubscribe  {movie  title from planetakino web} unsubscripbe you from you subscription";

        private  async void BotOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.TextMessage) return;

            Logger.Info($"Message recived {message.Chat.Id}");

            if (message.Text.StartsWith("/start"))
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                await _bot.SendTextMessageAsync(message.Chat.Id, Intro);
            }
            else if (message.Text.StartsWith("/showlist"))
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                await _bot.SendTextMessageAsync(message.Chat.Id,
                    $"Pick a Movie: {Environment.NewLine} {GetMovieList()}");
            }
            else if (message.Text.StartsWith("/find"))
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                var movieName = message.Text.Replace("/find", "").Trim(' ');

                var movies = MovieChecker.GetCinemaInfo().Movies.Where(x => x.Title.Contains(movieName));
                await SendMovieSuggestions(message.Chat.Id, movies);
            }
            else
            {
                await _bot.SendTextMessageAsync(message.Chat.Id, Intro);
            }
        }

        private  async Task SendMovieSuggestions(long chatId, IEnumerable<Movie> movies)
        {
            await _bot.SendTextMessageAsync(chatId, "Pick a Movie:",
                replyMarkup: new InlineKeyboardMarkup(movies.Select(x => InlineKeyboardButton.WithCallbackData(x.Title, x.CinemaMovieId.ToString())).ToArray()));
        }

        private  string GetMovieList()
        {
            return string.Join($", {Environment.NewLine}", MovieChecker.GetCinemaInfo().Movies.Select(x => x.Title).ToArray());
        }

        private  async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await SubscribeUserForMovie(callbackQueryEventArgs.CallbackQuery.From.Id, callbackQueryEventArgs.CallbackQuery.Data);

            await _bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id,
                $"Subscribed");
        }



        private  Task SubscribeUserForMovie(long chatId, string idStr)
        {
            var id = Int32.Parse(idStr);
            var movieId = MovieChecker.GetCinemaInfo().Movies.FirstOrDefault(x => x.CinemaMovieId == id);
            if (movieId != null)
            {
                var movie = MovieRepository.GetMovieByExternalId(movieId.CinemaMovieId);
                int idM = 0;
                if (movie == null)
                    idM = MovieRepository.AddMovie(new Movie
                    {
                        CinemaMovieId = movieId.CinemaMovieId,
                        Title = movieId.Title,
                        EndDate = movieId.EndDate,
                        StartDate = movieId.StartDate
                    });
                try
                {
                    SubscriptionRepository.Add(new UserSubscription { ChatId = chatId, MovieId = movie?.MovieId ?? idM });
                    Logger.Info($"Subsciption added for Movie {movieId} {idStr} and User {chatId}");
                }
                catch (DuplicateUserSubscriptionError e)
                {

                }
            }

            return Task.FromResult(0);
        }

        private  void SendNotification(UserSubscription userSubscription)
        {
            Logger.Info($"Started sending notification for movie {userSubscription.MovieId} for user {userSubscription.ChatId}");

            _bot.SendTextMessageAsync(userSubscription.ChatId, GenerateText(userSubscription.MovieId));
            userSubscription.IsNotified = true;
            SubscriptionRepository.Update(userSubscription);

            Logger.Info($"Finished sending notification for movie {userSubscription.MovieId} for user {userSubscription.ChatId}");
        }

        private  string GenerateText(int movieId)
        {
            var sb = new StringBuilder();
            sb.Append($"Начался старт продаж на кино {MovieRepository.GetMovieById(movieId).Title}");
            sb.Append(Environment.NewLine);
            sb.Append($"Доступные сеансы: /checkshowtime {movieId}");

            return sb.ToString();
        }

        public  void InitalizeBot()
        {
            _bot = new TelegramBotClient(ConfigurationManager.AppSettings["token"]);
            _bot.OnMessage += BotOnOnMessage;
            _bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Logger.Info($"Bot initialized {_bot.GetMeAsync().Result.Username}");
            _bot.StartReceiving();
        }

        public  void MovieCheckerOnOnRelease(object sender, MoveRealesReleaseArgs args)
        {
            var subs = SubscriptionRepository.GetAllByMovieId(args.MovieId);

            var userSubscriptions = subs as IList<UserSubscription> ?? subs.ToList();

            Logger.Info($"Started sending notification for movie {args.MovieId} subs count {userSubscriptions.Count()}");

            foreach (var userSubscription in userSubscriptions)
            {
                SendNotification(userSubscription);
            }

            Logger.Info($"Finished sending notification for movie {args.MovieId} subs count {userSubscriptions.Count()}");
        }

        public  string GetName()
        {
            return _bot.GetMeAsync().Result.Username;
        }

        public  void StopReceiving()
        {
            Logger.Debug("Stoped BOT");
            _bot.StopReceiving();
        }
    }
}