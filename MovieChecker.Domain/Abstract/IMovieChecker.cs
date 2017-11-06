using PlanetaKinoScheduleChecker.Domain.Models;

namespace PlanetaKinoScheduleChecker.Domain.Abstract
{
    public interface IMovieChecker
    {
        CinemaInfo GetCinemaInfo();
        int? GetMovieId(string movieTitle);
        bool CheckIfTicketsAvailiable(string movieTitle);
        bool CheckIfTicketsAvailiable(int movieId);
    }
}