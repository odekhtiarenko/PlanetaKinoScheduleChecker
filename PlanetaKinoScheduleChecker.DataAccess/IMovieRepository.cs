using System.Collections.Generic;
using PlanetaKinoScheduleChecker.Data;

namespace PlanetaKinoScheduleChecker.DataAccess
{
    public interface IMovieRepository
    {
        int AddMovie(Movie movie);
        Movie GetMovie(int movieId);
        IEnumerable<Movie> GetMovies();
    }
}
