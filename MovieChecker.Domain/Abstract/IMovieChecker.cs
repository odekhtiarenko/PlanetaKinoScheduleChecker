using PlanetaKinoScheduleChecker.Domain.Implementation;
using PlanetaKinoScheduleChecker.Domain.Models;

namespace PlanetaKinoScheduleChecker.Domain.Abstract
{
    public interface IMovieChecker
    {
        event MovieChecker.MovieReales OnRelease;
        void StartCheck();
        CinemaInfo GetCinemaInfo();
        int? GetMovieId(string movieTitle);
        bool CheckIfTicketsAvailiable(string movieTitle);
    }
}