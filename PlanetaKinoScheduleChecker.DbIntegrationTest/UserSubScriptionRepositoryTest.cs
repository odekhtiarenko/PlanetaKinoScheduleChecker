using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlanetaKinoScheduleChecker.Data;
using PlanetaKinoScheduleChecker.DataAccess;
using PlanetaKinoScheduleChecker.DataAccess.Abstract;
using PlanetaKinoScheduleChecker.DataAccess.Implementation;
using Ploeh.AutoFixture;

namespace PlanetaKinoScheduleChecker.DbIntegrationTest
{
    [TestFixture]
    public class UserSubScriptionRepositoryTest
    {
        private readonly IUserSubscriptionRepository _userSubscriptionRepository = new UserSubscriptionRepository();
        private readonly Fixture _fixture = new Fixture();

        [Test]
        public void Should_insert_Subscription()
        {

            var affectedRows = _userSubscriptionRepository.Add(
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

            var usersubsLst = new List<UserSubscription>
            {
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                },
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                },
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                },
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                },
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                },
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                },
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                }
            };




            foreach (var userSubscription in usersubsLst)
            {
                _userSubscriptionRepository.Add(userSubscription);
            }

            foreach (var userSubscription in _userSubscriptionRepository.GetAllByMovieId(movieId))
            {
                userSubscription.IsNotified = true;
                userSubscription.City = "KIEV";
                _userSubscriptionRepository.Update(userSubscription);
            }

            var subscriptions = _userSubscriptionRepository.GetAllByMovieId(movieId);

            Assert.That(subscriptions.Count(), Is.EqualTo(0));

        }

        [Test]
        public void Should_GetByMovieId_Subscription()
        {
            var movieId = _fixture.Create<int>();

            _userSubscriptionRepository.Add(
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                });

            _userSubscriptionRepository.Add(
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                });

            _userSubscriptionRepository.Add(
                new UserSubscription()
                {
                    ChatId = _fixture.Create<long>(),
                    IsNotified = false,
                    MovieId = movieId
                });

            var userSub = _userSubscriptionRepository.GetAllByMovieId(movieId);

            Assert.That(userSub.Count(), Is.EqualTo(3));
        }
    }
}
