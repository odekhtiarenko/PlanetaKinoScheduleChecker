using System.Collections.Generic;
using Dapper;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess.Abstract;

namespace PlanetaKinoScheduleChecker.DataAccess.Implementation
{
    public class UserSubscriptionRepository : IUserSubscriptionRepository
    {
        public int Add(UserSubscription userSubscription)
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                var entry = conn.QuerySingleOrDefault<UserSubscription>(SqlText.SubscriptionCheck,
                    new { userSubscription.MovieId, userSubscription.ChatId });

                if (entry != null)
                    throw new DuplicateUserSubscriptionError();

                var affectedRows = conn.Execute(SqlText.InsertSubscription,
                                new
                                {
                                    userSubscription.ChatId,
                                    userSubscription.MovieId,
                                    userSubscription.City,
                                    userSubscription.IsNotified
                                });
                return affectedRows;
            }
        }

        public void Update(UserSubscription userSubscription)
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Execute(SqlText.UpdateSubscription,
                    new
                    {
                        userSubscription.UserSubscriptionId,
                        userSubscription.ChatId,
                        userSubscription.MovieId,
                        userSubscription.City,
                        userSubscription.IsNotified
                    });
            }
        }

        public IEnumerable<UserSubscription> GetAllByMovieId(int movieId, bool isNotified = false)
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                var userSubscriptions = conn.Query<UserSubscription>(SqlText.GetSubscriptionByMovieId, new { MovieId = movieId, IsNotified = isNotified });
                return userSubscriptions;
            }
        }

        public IEnumerable<UserSubscription> GetAllByChatId(int chatId, bool isNotified = false)
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                var userSubscriptions = conn.Query<UserSubscription>(SqlText.GetSubscriptionByChatId, new { ChatId = chatId, IsNotified = isNotified });
                return userSubscriptions;
            }
        }
    }
}