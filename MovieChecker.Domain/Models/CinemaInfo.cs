using System.Collections.Generic;

namespace PlanetaKinoScheduleChecker.Domain.Models
{
    public class CinemaInfo
    {
        public string City { get; set; }
        public string Language { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
        public IEnumerable<ShowTime> ShowTimes { get; set; }
    }
}