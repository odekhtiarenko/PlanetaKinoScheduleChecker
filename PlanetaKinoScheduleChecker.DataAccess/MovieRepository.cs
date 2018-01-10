using System.Collections.Generic;
using System.Linq;
using Dapper;
using PlanetaKinoScheduleChecker.Data;

namespace PlanetaKinoScheduleChecker.DataAccess
{
    public class MovieRepository : IMovieRepository
    {
        public int AddMovie(Movie movie)
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Open();

                var affectedRows = conn.Execute(SqlText.Insert_Movie, new { MovieId = movie.MovieId, Title = movie.Title, StartDate = movie.StartDate, EndDate = movie.EndDate });
                return affectedRows;
            }
        }

        public Movie GetMovie(int movieId)
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Open();

                var movie = conn.QueryFirstOrDefault<Movie>(SqlText.Get_Movie, new { MovieId = movieId });
                return movie;
            }
        }

        public IEnumerable<Movie> GetMovies()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Open();

                var movies = conn.Query<Movie>(SqlText.GetAll_Movie);
                return movies;
            }
        }
    }
}
