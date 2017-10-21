using PlanetaKinoScheduleChecker.Domain.Models;

namespace PlanetaKinoScheduleChecker.Domain.Abstract
{
    public interface ICinemaInfoParser
    {
        bool TryParseCinemaInfo(string input, out CinemaInfo cinemaInfo);
    }
}