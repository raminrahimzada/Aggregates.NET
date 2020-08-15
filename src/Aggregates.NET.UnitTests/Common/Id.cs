using FluentAssertions;
using System;
using Xunit;

namespace Aggregates.Common
{
    public class IdTests : Test
    {
        [Fact]
        public void ShouldCreateIdFromString()
        {
            Id id = "test";
            string test = id;
        }
        [Fact]
        public void ShouldCreateIdFromGuid()
        {
            Id id = Guid.NewGuid();
            Guid test = id;
            Guid? test2 = id;
        }
        [Fact]
        public void ShouldCreateIdFromLong()
        {
            Id id = 1L;
            long test = id;
            long? test2 = id;
        }
        [Fact]
        public void TwoSameIdShouldBeEqual()
        {
            Id id = "test";
            id.Equals(id).Should().BeTrue();
        }
        [Fact]
        public void IdShouldNotEqualNull()
        {
            Id id = "test";
            id.Equals(null).Should().BeFalse();
        }
        [Fact]
        public void NullIdShouldEqualNull()
        {
            Id id = (string)null;
            id.Equals(null).Should().BeTrue();
        }
        [Fact]
        public void NullIdValueShouldEqualNull()
        {
            Id id = new Id(null);
            id.Equals(null).Should().BeTrue();
            (id == new Id(null)).Should().BeTrue();
            (id == (Id)null).Should().BeTrue();
            (id != "tt").Should().BeTrue();
        }
        [Fact]
        public void TwoIdenticalIdsShouldEqual()
        {
            Id id1 = "test";
            Id id2 = "test";

            id1.Equals(id2).Should().BeTrue();
        }
        [Fact]
        public void StringIdShouldEqualString()
        {
            Id id = "test";
            id.Equals("test").Should().BeTrue();
            (id == (Id)"test").Should().BeTrue();
            (id != (Id)"tt").Should().BeTrue();
        }
        [Fact]
        public void LongIdShouldEqualLong()
        {
            Id id = 1L;
            id.Equals(1L).Should().BeTrue();
            (id == (Id)1L).Should().BeTrue();
            (id != (Id)2L).Should().BeTrue();
        }
        [Fact]
        public void GuidIdShouldEqualGuid()
        {
            var guid = Guid.NewGuid();
            Id id = guid;
            id.Equals(guid).Should().BeTrue();
            (id == (Id)guid).Should().BeTrue();
            (id != (Id)Guid.NewGuid()).Should().BeTrue();
        }
        [Fact]
        public void ShouldGetStringWithNullValue()
        {
            var id = new Id(null);

            id.ToString().Should().Be("null");
        }
    }
}
