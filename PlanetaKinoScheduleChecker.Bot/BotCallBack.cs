using System;

namespace PlanetaKinoScheduleChecker.Bot
{
    internal class BotCallBack
    {
        public BotAction Action { get; set; }
        public int? Skip { get; set; }
        public string MoveiId { get; set; }
        public int? SubscriptionMovieId { get; set; }
        public DateTime? Date { get; set; }
    }

    internal enum BotAction
    {
        GetMovies,
        GetNextSubscriptionMoviesPage,
        GetPreviousSubscriptionMoviePage,
        GetSchedule,
        GetSubscriptions,
        SubscribeForMovie,
        DeleteSubscription,
        GetMoviesGetScheduleByDay,
        GetScheduleByMovie,
        GetScheduleByDate,
        GetNextScheduleMoviesPage,
        GetPreviousScheduleMoviePage,
        GetMovieSchedule
    }
}