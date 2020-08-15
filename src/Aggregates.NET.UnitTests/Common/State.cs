﻿using Aggregates.Contracts;
using FluentAssertions;
using System;
using Xunit;

namespace Aggregates.Common
{
    public class State : Test
    {
        private IEntityFactory<FakeEntity> Factory = Internal.EntityFactory.For<FakeEntity>();
    
        [Fact]
        public void ShouldRestoreFromSnapshot()
        {
            var snapshot = Fake<FakeState>();
            var entity = Factory.Create("test", "test", snapshot: snapshot);
            entity.State.SnapshotWasRestored.Should().BeTrue();
        }
        [Fact]
        public void ShouldRestoreFromEvents()
        {
            var events = Many<FakeDomainEvent.FakeEvent>();
            var entity = Factory.Create("test", "test", events: events);
            // 3 events = version 2
            entity.State.Version.Should().Be(2L);
        }
        [Fact]
        public void ShouldRejectInvalidSnapshot()
        {
            var snapshot = Fake<int>();
            var e = Record.Exception(() => Factory.Create("test", "test", snapshot: snapshot));
            e.Should().BeOfType<ArgumentException>();
        }
    }
}
