using System.Collections.Generic;
using System.Linq;
using PlanetaKinoScheduleChecker.Data;

namespace PlanetaKinoScheduleChecker.DataAccess
{
    public class UserSubscriptionRepository : IUserSubscriptionRepository
    {
        private static readonly IList<UserSubscription> UserSubscriptions = new List<UserSubscription>();

        public void Add(UserSubscription userSubscription)
        {
            userSubscription.Id = UserSubscriptions.Count + 1;
            UserSubscriptions.Add(userSubscription);
        }

        public void Update(UserSubscription userSubscription)
        {
            var entity = UserSubscriptions.SingleOrDefault(x => x.Id == userSubscription.Id);

            if (entity != null)
            {
                var i = UserSubscriptions.IndexOf(entity);
                UserSubscriptions[i] = userSubscription;
            }
        }

        public UserSubscription Get(int id)
        {
            return UserSubscriptions.SingleOrDefault(x => x.Id == id);
        }

        public IEnumerable<UserSubscription> GetAll()
        {
            return UserSubscriptions;
        }

        public IEnumerable<UserSubscription> GetAllByChatId(long chatId)
        {
            return UserSubscriptions.Where(x => x.ChatId == chatId).ToList();
        }

        public IEnumerable<UserSubscription> GetAllByMovieId(int movieId)
        {
            return UserSubscriptions.Where(x => x.MovieId == movieId && !x.IsNotified).ToList();
        }
    }
}