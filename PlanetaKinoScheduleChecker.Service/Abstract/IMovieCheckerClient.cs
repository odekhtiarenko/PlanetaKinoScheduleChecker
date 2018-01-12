using PlanetaKinoScheduleChecker.Service.Models;
using System.ComponentModel.Composition;

namespace PlanetaKinoScheduleChecker.Service.Abstract
{
    [InheritedExport]
    public interface IMovieCheckerClient
    {
        CinemaInfo GetCinemaInfo();
    }
}