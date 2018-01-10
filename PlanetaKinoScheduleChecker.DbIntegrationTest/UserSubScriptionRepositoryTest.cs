using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess;
using Ploeh.AutoFixture;

namespace PlanetaKinoScheduleChecker.DbIntegrationTest
{
    [TestFixture]
    public class UserSubScriptionRepositoryTest
    {
        private readonly IUserSubscriptionRepository userSubscriptionRepository = new UserSubscriptionRepository();
        private Fixture _fixture = new Fixture();

        [Test]
        public void Should_insert_Subscription()
        {

            var affectedRows = userSubscriptionRepository.Add(
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = _fixture.Create<int>()
                });

            Assert.That(affectedRows, Is.GreaterThan(0));

        }

        [Test]
        public void Should_Update_Subscription()
        {
            var movieId = _fixture.Create<int>();

            var usersubsLst = new List<UserSubscription>();

            usersubsLst.Add(new UserSubscription()
            {
                ChatId = _fixture.Create<long>(),
                IsNotified = false,
                MovieId = movieId
            });
            usersubsLst.Add(new UserSubscription()
            {
                ChatId = _fixture.Create<long>(),
                IsNotified = false,
                MovieId = movieId
            });
            usersubsLst.Add(new UserSubscription()
            {
                ChatId = _fixture.Create<long>(),
                IsNotified = false,
                MovieId = movieId
            });

            usersubsLst.Add(new UserSubscription()
            {
                ChatId = _fixture.Create<long>(),
                IsNotified = false,
                MovieId = movieId
            });
            usersubsLst.Add(new UserSubscription()
            {
                ChatId = _fixture.Create<long>(),
                IsNotified = false,
                MovieId = movieId
            });
            usersubsLst.Add(new UserSubscription()
            {
                ChatId = _fixture.Create<long>(),
                IsNotified = false,
                MovieId = movieId
            });
            usersubsLst.Add(new UserSubscription()
            {
                ChatId = _fixture.Create<long>(),
                IsNotified = false,
                MovieId = movieId
            });


            foreach (var userSubscription in usersubsLst)
            {
                userSubscriptionRepository.Add(userSubscription);
            }

            foreach (var userSubscription in userSubscriptionRepository.GetAllByMovieId(movieId))
            {
                userSubscription.IsNotified = true;
                userSubscription.City = "KIEV";
                userSubscriptionRepository.Update(userSubscription);
            }

            var subscriptions = userSubscriptionRepository.GetAllByMovieId(movieId);

            Assert.That(subscriptions.Count(), Is.EqualTo(0));

        }

        [Test]
        public void Should_GetByMovieId_Subscription()
        {
            var movieId = _fixture.Create<int>();

            userSubscriptionRepository.Add(
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                });

            userSubscriptionRepository.Add(
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                });

            userSubscriptionRepository.Add(
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                });

            var userSub = userSubscriptionRepository.GetAllByMovieId(movieId);

            Assert.That(userSub.Count(), Is.EqualTo(3));
        }
    }
}
