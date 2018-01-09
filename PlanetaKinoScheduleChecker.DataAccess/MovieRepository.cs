using System.Collections.Generic;
using System.Linq;
using PlanetaKinoScheduleChecker.Data;

namespace PlanetaKinoScheduleChecker.DataAccess
{
    public class MovieRepository : IMovieRepository
    {
        private static readonly IList<Movie> Movies = new List<Movie>();

        public void AddMovie(Movie movie)
        {
            if (Movies.All(x => x.Id != movie.Id))
                Movies.Add(movie);
        }

        public void Update(Movie movie)
        {
            var entity = Movies.SingleOrDefault(x => x.Id == movie.Id);

            if (entity != null)
            {
                var i = Movies.IndexOf(entity);
                Movies[i] = movie;
            }
        }

        public Movie GetMovie(int movieId)
        {
            return Movies.SingleOrDefault(x => x.Id == movieId);
        }

        public IEnumerable<Movie> GetMovies()
        {
            return Movies;
        }
    }
}