using System;
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

                var affectedRows = conn.QuerySingle<int>(SqlText.Insert_Movie, new { CinemaMovieId = movie.CinemaMovieId, Title = movie.Title, StartDate = movie.StartDate, EndDate = movie.EndDate });
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
