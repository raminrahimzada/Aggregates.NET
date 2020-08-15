using System;
using System.Threading.Tasks;
using Aggregates.Contracts;
using Xunit;
using FakeItEasy;

namespace Aggregates.Common
{
    public class DelayedSubscriber : TestSubject<Internal.DelayedSubscriber>
    {
        [Fact]
        public async Task ShouldEnableByCategoryProjection()
        {
            var consumer = Fake<IEventStoreConsumer>();
            Inject(consumer);

            await Sut.Setup("test", Version.Parse("0.0.0")).ConfigureAwait(false);

            A.CallTo(() => consumer.EnableProjection("$by_category")).MustHaveHappened();
        }

        [Fact]
        public async Task ShouldConnectToEventStore()
        {
            var consumer = Fake<IEventStoreConsumer>();
            Inject(consumer);

            await Sut.Setup("test", Version.Parse("0.0.0")).ConfigureAwait(false);
            await Sut.Connect().ConfigureAwait(false);

            A.CallTo(consumer).Where(call => call.Method.Name == "ConnectRoundRobinPersistentSubscription").MustHaveHappened();
        }


    }
}
