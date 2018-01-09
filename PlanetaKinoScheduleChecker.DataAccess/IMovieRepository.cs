using System.Collections.Generic;
using PlanetaKinoScheduleChecker.Data;

namespace PlanetaKinoScheduleChecker.DataAccess
{
    public interface IMovieRepository
    {
        void AddMovie(Movie movie);
        void Update(Movie movie);
        Movie GetMovie(int movieId);
        IEnumerable<Movie> GetMovies();
    }
}
