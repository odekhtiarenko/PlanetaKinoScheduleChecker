using PlanetaKinoScheduleChecker.Service.Models;

namespace PlanetaKinoScheduleChecker.Service.Abstract
{
    public interface ICinemaInfoParser
    {
        bool TryParseCinemaInfo(string input, out CinemaInfo cinemaInfo);
    }
}