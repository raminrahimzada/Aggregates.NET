using FluentAssertions;
using System;
using Xunit;

namespace Aggregates.Common
{
    public class EntityFactory : TestSubject<Internal.EntityFactory<FakeEntity, FakeState>>
    {
        [Fact]
        public void ShouldCreateNewEntity()
        {
            var entity = Sut.Create("test", "test");
            entity.Version.Should().Be(Internal.EntityFactory.NewEntityVersion);
        }
        [Fact]
        public void ShouldUseSnapshot()
        {
            var snapshot = Fake<FakeState>();
            var entity = Sut.Create("test", "test", snapshot: snapshot);
            entity.State.SnapshotWasRestored.Should().BeTrue();
        }
        [Fact]
        public void ShouldRejectIncorrectSnapshot()
        {
            var snapshot = Fake<int>();
            var e = Record.Exception(() => Sut.Create("test", "test", snapshot: snapshot));
            e.Should().BeOfType<ArgumentException>();
        }
    }
}
