using System.Collections.Generic;
using System.Linq;
using Dapper;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess.Abstract;

namespace PlanetaKinoScheduleChecker.DataAccess.Implementation
{
    public class MovieRepository : IMovieRepository
    {
        public int AddMovie(Movie movie)
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Open();

                var affectedRows = conn.QuerySingle<int>(SqlText.InsertMovie, new { CinemaMovieId = movie.CinemaMovieId, Title = movie.Title, StartDate = movie.StartDate, EndDate = movie.EndDate });
                return affectedRows;
            }
        }

        public Movie GetMovieByExternalId(int movieId)
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Open();

                var movie = conn.QueryFirstOrDefault<Movie>(SqlText.GetMovieByCinemaMovieId, new { CinemaMovieId = movieId });
                return movie;
            }
        }

        public Movie GetMovieById(int movieId)
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Open();

                var movie = conn.QueryFirstOrDefault<Movie>(SqlText.GetMovieByMovieId, new { MovieId = movieId });
                return movie;
            }
        }

        public IEnumerable<Movie> GetMovies()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Open();

                var movies = conn.Query<Movie>(SqlText.GetAllMovie);
                return movies;
            }
        }

        public IEnumerable<Movie> GetAvailiableMovies()
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                connection.Open();

                var movieDictionary = new Dictionary<int, Movie>();

                var movies = connection.Query<Movie, UserSubscription, Movie>(
                        SqlText.GetAvailiableSubscription,
                        (invoice, invoiceItem) =>
                        {
                            Movie movieEntry;

                            if (!movieDictionary.TryGetValue(invoice.MovieId, out movieEntry))
                            {
                                movieEntry = invoice;
                                movieEntry.Subscriptions = new List<UserSubscription>();
                                movieDictionary.Add(movieEntry.MovieId, movieEntry);
                            }

                            movieEntry.Subscriptions.Add(invoiceItem);
                            return movieEntry;
                        },
                        splitOn: "UserSubscriptionId")
                    .Distinct()
                    .ToList();
                return movies;
            }
        }
    }
}
