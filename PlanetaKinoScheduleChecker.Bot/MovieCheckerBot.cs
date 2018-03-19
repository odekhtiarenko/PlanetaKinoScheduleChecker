using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess;
using PlanetaKinoScheduleChecker.DataAccess.Abstract;
using PlanetaKinoScheduleChecker.Service.Abstract;
using PlanetaKinoScheduleChecker.Service.Implementation;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace PlanetaKinoScheduleChecker.Bot
{
    public class MovieCheckerBot : IMovieCheckerBot
    {
        private ITelegramBotClient _bot;
        private readonly IMovieRepository _movieRepository;
        private readonly IUserSubscriptionRepository _subscriptionRepository;
        private readonly IMovieChecker _movieChecker;

        private static readonly ILog _logger = LogManager.GetLogger(typeof(MovieCheckerBot));

        public MovieCheckerBot(IMovieRepository movieRepository, IUserSubscriptionRepository subscriptionRepository, IMovieChecker movieChecker)
        {
            _movieRepository = movieRepository;
            _subscriptionRepository = subscriptionRepository;
            _movieChecker = movieChecker;
        }

        private async void BotOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {

            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.TextMessage) return;

            _logger.Info($"Message recived {message.Chat.Id}");

            if (message.Text.StartsWith("/start"))
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                var movies = _movieChecker.GetCinemaInfo().Movies;
                await SendMovieSuggestions(message.Chat.Id, movies);
            }
            else if (message.Text.StartsWith("/find"))
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                var movieName = message.Text.Replace("/find", "").Trim(' ');

                var movies = _movieChecker.GetCinemaInfo().Movies.Where(x => x.Title.Contains(movieName));
                await SendMovieSuggestions(message.Chat.Id, movies);
            }
            else
            {
                await _bot.SendTextMessageAsync(message.Chat.Id, "Try again");
            }
        }

        private async Task SendMovieSuggestions(long chatId, IEnumerable<Movie> movies, int skip = 0, int take = 20)
        {
            InlineKeyboardButton[][] keyboard = CreateKeyboard(movies, skip, take);

            await _bot.SendTextMessageAsync(chatId, "Pick a Movie:",
                replyMarkup: new InlineKeyboardMarkup(keyboard));
        }

        private InlineKeyboardButton[][] CreateKeyboard(IEnumerable<Movie> movies, int skip = 0, int take = 20)
        {
            var keyboard = new List<InlineKeyboardButton[]>();

            InlineKeyboardButton[] el = new InlineKeyboardButton[3];

            var i = 0;
            foreach (var movie in movies.Skip(skip).Take(take))
            {
                if (i == el.Length)
                {
                    i = 0;
                    keyboard.Add(el);
                    el = new InlineKeyboardButton[3];
                }
                el[i] = InlineKeyboardButton.WithCallbackData(movie.Title, JsonConvert.SerializeObject(new SelectMovieCallBack() { MoveiId = movie.CinemaMovieId.ToString() }));
                i++;
            }

            keyboard.Add(new[]{ InlineKeyboardButton.WithCallbackData(skip==0 ?"|":"Previous",JsonConvert.SerializeObject(new SelectMovieCallBack(){Skip = skip-take, Take = take})),
                InlineKeyboardButton.WithCallbackData(skip+take > movies.Count() ? "|": "Next",JsonConvert.SerializeObject(new SelectMovieCallBack(){Skip = skip+take, Take = take}))});

            return keyboard.ToArray();
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var selectMovieCallBack =
                JsonConvert.DeserializeObject<SelectMovieCallBack>(callbackQueryEventArgs.CallbackQuery.Data);

            if (selectMovieCallBack.MoveiId != null)
            {
                await SubscribeUserForMovie(callbackQueryEventArgs.CallbackQuery.From.Id, selectMovieCallBack.MoveiId);

                await _bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id,
                    $"Subscribed");
            }
            else
            {
                await SendUpdateMovieSuggestions(callbackQueryEventArgs.CallbackQuery.Message.MessageId, callbackQueryEventArgs.CallbackQuery.From.Id,
                    _movieChecker.GetCinemaInfo().Movies, selectMovieCallBack.Skip, selectMovieCallBack.Take);
            }
        }

        private async Task SendUpdateMovieSuggestions(int messageId, int fromId, IEnumerable<Movie> movies, int skip, int take)
        {
            InlineKeyboardButton[][] keyboard = CreateKeyboard(movies, skip, take);

            await _bot.EditMessageReplyMarkupAsync(fromId, messageId, replyMarkup: new InlineKeyboardMarkup(keyboard));
        }

        private Task SubscribeUserForMovie(long chatId, string idStr)
        {
            var id = Int32.Parse(idStr);
            var movieId = _movieChecker.GetCinemaInfo().Movies.FirstOrDefault(x => x.CinemaMovieId == id);
            if (movieId != null)
            {
                var movie = _movieRepository.GetMovieByExternalId(movieId.CinemaMovieId);
                int idM = 0;
                if (movie == null)
                    idM = _movieRepository.AddMovie(new Movie
                    {
                        CinemaMovieId = movieId.CinemaMovieId,
                        Title = movieId.Title,
                        EndDate = movieId.EndDate,
                        StartDate = movieId.StartDate
                    });
                try
                {
                    _subscriptionRepository.Add(new UserSubscription { ChatId = chatId, MovieId = movie?.MovieId ?? idM });
                    _logger.Info($"Subsciption added for Movie {movieId} {idStr} and User {chatId}");
                }
                catch (DuplicateUserSubscriptionError e)
                {
                    _bot.SendTextMessageAsync(chatId, "You've already subscribed for this movie");
                }
            }

            return Task.FromResult(0);
        }

        private void SendNotification(UserSubscription userSubscription)
        {
            _logger.Info($"Started sending notification for movie {userSubscription.MovieId} for user {userSubscription.ChatId}");

            _bot.SendTextMessageAsync(userSubscription.ChatId, GenerateText(userSubscription.MovieId));
            userSubscription.IsNotified = true;
            _subscriptionRepository.Update(userSubscription);

            _logger.Info($"Finished sending notification for movie {userSubscription.MovieId} for user {userSubscription.ChatId}");
        }

        private string GenerateText(int movieId)
        {
            var sb = new StringBuilder();
            sb.Append($"Начался старт продаж на кино {_movieRepository.GetMovieById(movieId).Title}");
            sb.Append(Environment.NewLine);
            sb.Append($"Доступные сеансы: /checkshowtime {movieId}");

            return sb.ToString();
        }

        public void InitalizeBot()
        {
            _bot = new TelegramBotClient(ConfigurationManager.AppSettings["token"]);
            _bot.OnMessage += BotOnOnMessage;
            _bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            _logger.Info($"Bot initialized {_bot.GetMeAsync().Result.Username}");
            _bot.StartReceiving();
        }

        public void MovieCheckerOnOnRelease(object sender, MoveRealesReleaseArgs args)
        {
            var subs = _subscriptionRepository.GetAllByMovieId(args.MovieId);

            var userSubscriptions = subs as IList<UserSubscription> ?? subs.ToList();

            _logger.Info($"Started sending notification for movie {args.MovieId} subs count {userSubscriptions.Count()}");

            foreach (var userSubscription in userSubscriptions)
            {
                SendNotification(userSubscription);
            }

            _logger.Info($"Finished sending notification for movie {args.MovieId} subs count {userSubscriptions.Count()}");
        }

        public string GetName()
        {
            return _bot.GetMeAsync().Result.Username;
        }

        public void StopReceiving()
        {
            _logger.Debug("Stoped BOT");
            _bot.StopReceiving();
        }
    }
}