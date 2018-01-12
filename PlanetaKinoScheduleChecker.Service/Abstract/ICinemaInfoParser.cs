using System.ComponentModel.Composition;
using PlanetaKinoScheduleChecker.Service.Models;

namespace PlanetaKinoScheduleChecker.Service.Abstract
{
    [InheritedExport]
    public interface ICinemaInfoParser
    {
        bool TryParseCinemaInfo(string input, out CinemaInfo cinemaInfo);
    }
}