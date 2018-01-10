using System;

namespace PlanetaKinoScheduleChecker.Data
{
    public class Movie
    {
        public Movie()
        {
        }

        public Movie(int movieId, string trim)
        {
            MovieId = movieId;
            Title = trim;
        }

        public int Id { get; set; }
        public int MovieId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsSaleOpened { get; set; }
    }
}