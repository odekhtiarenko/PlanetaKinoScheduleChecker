using System;
using System.Collections.Generic;

namespace PlanetaKinoScheduleChecker.Data
{
    public class Movie
    {
        public int MovieId { get; set; }
        public int CinemaMovieId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsSaleOpened { get; set; }
        public IList<UserSubscription> Subscriptions { get; set; }
    }
}