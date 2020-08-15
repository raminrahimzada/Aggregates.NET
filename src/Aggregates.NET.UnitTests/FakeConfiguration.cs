using Aggregates.Contracts;
using FakeItEasy;

namespace Aggregates
{
    internal class FakeConfiguration : Configure
    {
        public FakeConfiguration() : base()
        {
            Container = A.Fake<IContainer>();
            A.CallTo(() => Container.GetChildContainer()).Returns(Container);
        }
    }
}
