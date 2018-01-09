using System.Collections.Generic;
using PlanetaKinoScheduleChecker.Data;

namespace PlanetaKinoScheduleChecker.DataAccess
{
    public interface IUserSubscriptionRepository
    {
        void Add(UserSubscription userSubscription);
        void Update(UserSubscription userSubscription);
        UserSubscription Get(int id);
        IEnumerable<UserSubscription> GetAll();
        IEnumerable<UserSubscription> GetAllByChatId(long chatId);
        IEnumerable<UserSubscription> GetAllByMovieId(int movieId);
    }
}