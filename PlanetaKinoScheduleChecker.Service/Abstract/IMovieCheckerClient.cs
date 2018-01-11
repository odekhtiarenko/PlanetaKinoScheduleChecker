using PlanetaKinoScheduleChecker.Service.Models;

namespace PlanetaKinoScheduleChecker.Service.Abstract
{
    public interface IMovieCheckerClient
    {
        CinemaInfo GetCinemaInfo();
    }
}