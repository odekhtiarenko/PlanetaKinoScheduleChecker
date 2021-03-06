﻿using System.ComponentModel.Composition;
using PlanetaKinoScheduleChecker.Service.Implementation;
using PlanetaKinoScheduleChecker.Service.Models;

namespace PlanetaKinoScheduleChecker.Service.Abstract
{
    [InheritedExport]
    public interface IMovieChecker
    {
        event MovieChecker.MovieReales OnRelease;
        void StartCheck();
        CinemaInfo GetCinemaInfo();
        int? GetMovieId(string movieTitle);
        bool CheckIfTicketsAvailiable(string movieTitle);
    }
}