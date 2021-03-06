﻿using System.Collections.Generic;
using PlanetaKinoScheduleChecker.Data;

namespace PlanetaKinoScheduleChecker.Service.Models
{
    public class CinemaInfo
    {
        public string City { get; set; }
        public string Language { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
        public IEnumerable<ShowTime> ShowTimes { get; set; }
    }
}