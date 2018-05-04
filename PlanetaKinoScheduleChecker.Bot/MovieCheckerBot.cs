using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
                await SendIntro(message.Chat.Id);
            }
            else
            {
                await _bot.SendTextMessageAsync(message.Chat.Id, "Try again");
            }
        }

        private async Task SendIntro(long chatId)
        {
            var markup = new InlineKeyboardMarkup(new[] {
                InlineKeyboardButton.WithCallbackData("Movies", ConvertToJson(new BotCallBack(){Action = BotAction.GetMovies})),
                InlineKeyboardButton.WithCallbackData("Schedule", ConvertToJson(new BotCallBack(){Action = BotAction.GetSchedule})),
                InlineKeyboardButton.WithCallbackData("My subscriptions", ConvertToJson(new BotCallBack(){Action = BotAction.GetSubscriptions}))
            });

            await _bot.SendTextMessageAsync(chatId, "Pick an action", replyMarkup: markup);
        }

        private async Task SendMovieSuggestions(long chatId, BotAction action, int skip = 0, int take = 20)
        {
            var keyboard = CreateKeyboard(action, skip, take);

            await _bot.SendTextMessageAsync(chatId, "Pick a Movie:",
                replyMarkup: new InlineKeyboardMarkup(keyboard));
        }

        private InlineKeyboardButton[][] CreateKeyboard(BotAction action, int skip = 0, int take = 20)
        {
            var keyboard = new List<InlineKeyboardButton[]>();

            InlineKeyboardButton[] el = new InlineKeyboardButton[3];
            var i = 0;
            var movies = _movieChecker.GetCinemaInfo().Movies;
            foreach (var movie in movies.Skip(skip).Take(take))
            {
                if (i == el.Length)
                {
                    i = 0;
                    keyboard.Add(el);
                    el = new InlineKeyboardButton[3];
                }
                el[i] = InlineKeyboardButton.WithCallbackData(movie.Title, ConvertToJson(new BotCallBack() { Action = action, MoveiId = movie.CinemaMovieId.ToString() }));
                i++;
            }

            keyboard.Add(new[]{ InlineKeyboardButton.WithCallbackData(skip==0 ?"|":"Previous", ConvertToJson(new BotCallBack(){ Action = BotAction.GetPreviousSubscriptionMoviePage, Skip = skip-take})),
                InlineKeyboardButton.WithCallbackData(skip+take > movies.Count() ? "|": "Next", ConvertToJson(new BotCallBack(){Action = BotAction.GetNextSubscriptionMoviesPage, Skip = skip+take}))});

            return keyboard.ToArray();
        }

        private string ConvertToJson(BotCallBack botCallBack)
        {
            return JsonConvert.SerializeObject(botCallBack, Formatting.None,
                new JsonSerializerSettings() {NullValueHandling = NullValueHandling.Ignore});
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var botCallBack =
                JsonConvert.DeserializeObject<BotCallBack>(callbackQueryEventArgs.CallbackQuery.Data);
            var chatId = callbackQueryEventArgs.CallbackQuery.From.Id;

            switch (botCallBack.Action)
            {
                case BotAction.GetMovies:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await SendMovieSuggestions(chatId, BotAction.SubscribeForMovie);
                    break;
                case BotAction.SubscribeForMovie:
                    await SubscribeUserForMovie(chatId, botCallBack.MoveiId);
                    await _bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                        $"Subscribed");
                    break;
                case BotAction.GetNextSubscriptionMoviesPage:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await UpdateMovieSuggestions(chatId, callbackQueryEventArgs.CallbackQuery.Message.MessageId, botCallBack.Skip.Value, BotAction.SubscribeForMovie);
                    break;
                case BotAction.GetPreviousSubscriptionMoviePage:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await UpdateMovieSuggestions(chatId, callbackQueryEventArgs.CallbackQuery.Message.MessageId, botCallBack.Skip.Value, BotAction.SubscribeForMovie);
                    break;
                case BotAction.GetSubscriptions:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await SendSubscriptions(chatId);
                    break;
                case BotAction.GetSchedule:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await SendSchedule(chatId);
                    break;
                case BotAction.GetScheduleByMovie:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await SendMovieSuggestions(chatId, BotAction.GetMovieSchedule);
                    break;
                case BotAction.GetScheduleByDate:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await SendSchedule(chatId);
                    break;
                case BotAction.GetNextScheduleMoviesPage:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await UpdateMovieSuggestions(chatId, callbackQueryEventArgs.CallbackQuery.Message.MessageId, botCallBack.Skip.Value, BotAction.GetMovieSchedule);
                    break;
                case BotAction.GetPreviousScheduleMoviePage:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await UpdateMovieSuggestions(chatId, callbackQueryEventArgs.CallbackQuery.Message.MessageId, botCallBack.Skip.Value, BotAction.GetMovieSchedule);
                    break;
                case BotAction.GetMovieSchedule:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await SendMovieSchedule(chatId, botCallBack.MoveiId);
                    break;
                default:
                    await _bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await SendIntro(chatId);
                    break;
            }
        }

        private async Task SendMovieSchedule(int chatId, string moveiId)
        {
            var id = int.Parse(moveiId);
            var movieSchedule = _movieChecker.GetCinemaInfo().ShowTimes.Where(x=>x.MovieId== id);
            var sb = new StringBuilder();

            foreach (var x in movieSchedule)
            {
                sb.Append(
                    $"{x.Theatre} {x.Technology} {x.FullDate.ToShortDateString()} {x.FullDate.ToShortTimeString()}");
            }

            await _bot.SendTextMessageAsync(chatId, sb.ToString());
        }

        private async Task SendSchedule(int chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new []
            {
                InlineKeyboardButton.WithCallbackData("By Movie", ConvertToJson(new BotCallBack(){Action = BotAction.GetScheduleByMovie})),
                InlineKeyboardButton.WithCallbackData("By Date", ConvertToJson(new BotCallBack(){Action = BotAction.GetScheduleByDate})),
            });

            await _bot.SendTextMessageAsync(chatId, "Pick:", replyMarkup: keyboard);
        }

        private InlineKeyboardButton[] GetDates()
        {
            var t = _movieChecker.GetCinemaInfo();
            var dateTimes = t.ShowTimes.Select(x => x.FullDate).Distinct().ToArray();

            return dateTimes.Select(x => InlineKeyboardButton.WithCallbackData($"{x.Day} {x.Month}",
                ConvertToJson(
                    new BotCallBack() {Action = BotAction.GetMoviesGetScheduleByDay, Date = x}))).ToArray();
        }

        private async Task SendSubscriptions(int chatId)
        {
            var subscriptions = _subscriptionRepository.GetAllByChatId(chatId);
            InlineKeyboardButton[] t = subscriptions.Select(x => InlineKeyboardButton.WithCallbackData(x.MovieId.ToString(),
                    ConvertToJson(
                        new BotCallBack() { Action = BotAction.DeleteSubscription, SubscriptionMovieId = x.MovieId })))
                .ToArray();
            var keyboard = new InlineKeyboardMarkup(t);

            await _bot.SendTextMessageAsync(chatId, "Your subscriptions", replyMarkup: keyboard);
        }

        private async Task UpdateMovieSuggestions(int chatId, int messageId, int skip, BotAction action)
        {
            var keyboard = CreateKeyboard(action, skip);

            await _bot.EditMessageReplyMarkupAsync(chatId, messageId, replyMarkup: new InlineKeyboardMarkup(keyboard));
        }

        private async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            await _bot.SendTextMessageAsync(inlineQueryEventArgs.InlineQuery.From.Id, "Pick a Movie:");
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
            _bot.OnInlineQuery += BotOnInlineQueryReceived;
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