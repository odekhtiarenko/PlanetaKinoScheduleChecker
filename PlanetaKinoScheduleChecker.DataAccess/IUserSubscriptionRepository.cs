using System.Collections.Generic;
using PlanetaKinoScheduleChecker.Data;

namespace PlanetaKinoScheduleChecker.DataAccess
{
    public interface IUserSubscriptionRepository
    {
        int Add(UserSubscription userSubscription);
        void Update(UserSubscription userSubscription);
        IEnumerable<UserSubscription> GetAllByMovieId(int movieId, bool isNotified = false);
    }
}