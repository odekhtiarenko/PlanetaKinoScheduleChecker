using PlanetaKinoScheduleChecker.Domain.Models;

namespace PlanetaKinoScheduleChecker.Domain.Abstract
{
    public interface IMovieCheckerClient
    {
        CinemaInfo GetCinemaInfo();
    }
}