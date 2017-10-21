using System;

namespace PlanetaKinoScheduleChecker.Domain.Models
{
    public class ShowTime
    {
        public DateTime FullDate { get; set; }
        public string Theatre { get; set; }
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public string Technology { get; set; }
        public string OrderUrl { get; set; }
    }
}