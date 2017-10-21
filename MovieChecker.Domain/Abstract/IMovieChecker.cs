using PlanetaKinoScheduleChecker.Domain.Models;

namespace PlanetaKinoScheduleChecker.Domain.Abstract
{
    public interface IMovieChecker
    {
        CinemaInfo GetCinemaInfo();
        bool CheckIfTicketsAvailiable(string movieTitle);
    }
}