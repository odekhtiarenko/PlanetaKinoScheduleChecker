using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess.Abstract;

namespace PlanetaKinoScheduleChecker.Api.Host.Controllers
{
    public class UserSubscriptionController : ApiController
    {
        private readonly IUserSubscriptionRepository _subscriptionRepository;

        public UserSubscriptionController(IUserSubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public IEnumerable<UserSubscription> Get()
        {
             return _subscriptionRepository.GetAllByMovieId(2);
        }
    }
}
