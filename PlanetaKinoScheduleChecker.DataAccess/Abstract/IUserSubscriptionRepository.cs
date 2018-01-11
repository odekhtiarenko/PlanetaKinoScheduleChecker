using System.Collections.Generic;
using System.ComponentModel.Composition;
using PlanetaKinoScheduleChecker.Data;

namespace PlanetaKinoScheduleChecker.DataAccess.Abstract
{
    [InheritedExport]
    public interface IUserSubscriptionRepository
    {
        int Add(UserSubscription userSubscription);
        void Update(UserSubscription userSubscription);
        IEnumerable<UserSubscription> GetAllByMovieId(int movieId, bool isNotified = false);
    }
}