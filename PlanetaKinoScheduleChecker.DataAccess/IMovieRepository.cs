using System.Collections.Generic;
using PlanetaKinoScheduleChecker.Data;

namespace PlanetaKinoScheduleChecker.DataAccess
{
    public interface IMovieRepository
    {
        int AddMovie(Movie movie);
        Movie GetMovieByExternalId(int cinemaMovieId);
        Movie GetMovieById(int movieId);
        IEnumerable<Movie> GetMovies();
        IEnumerable<Movie> GetAvailiableMovies();
    }
}
