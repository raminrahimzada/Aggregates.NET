using Aggregates.Attributes;
using FluentAssertions;
using System;
using Xunit;

namespace Aggregates.Common
{
    public class DelayedAttributes : Test
    {
        private class FakeDto
        {
            public string Property { get; set; }
        }

        private class FakeDtoWithKeyProperties
        {
            [KeyProperty]
            public string Property { get; set; }
        }

        [Fact]
        public void ShouldCreateDelayedAttribute()
        {
            var attr = new DelayedAttribute(typeof(FakeDto), count: 1);

            attr.Type.Should().Be(typeof(FakeDto));
            attr.KeyPropertyFunc(new FakeDto()).Should().BeEmpty();
        }
        [Fact]
        public void ShouldCreateKeyProperties()
        {
            var attr = new DelayedAttribute(typeof(FakeDtoWithKeyProperties), count: 1);

            attr.Type.Should().Be(typeof(FakeDtoWithKeyProperties));
            attr.KeyPropertyFunc(new FakeDtoWithKeyProperties { Property = "test" }).Should().Contain("test");
        }
        [Fact]
        public void ShouldRequireCountOrDelayed()
        {
            var e1 = Record.Exception(() => new DelayedAttribute(typeof(FakeDto)));
            var e2 = Record.Exception(() => new DelayedAttribute(typeof(FakeDto), count: 1));
            var e3 = Record.Exception(() => new DelayedAttribute(typeof(FakeDto), delayMs: 1));

            e1.Should().BeOfType<ArgumentException>();
            e2.Should().BeNull();
            e3.Should().BeNull();
        }
        [Fact]
        public void ShouldNotAllowTooBigCount()
        {
            var e1 = Record.Exception(() => new DelayedAttribute(typeof(FakeDto), count: int.MaxValue));

            e1.Should().BeOfType<ArgumentException>();
        }
        [Fact]
        public void ShouldRejectInvalidDtoType()
        {
            var attr = new DelayedAttribute(typeof(FakeDtoWithKeyProperties), count: 1);

            var e = Record.Exception(() => attr.KeyPropertyFunc(new FakeDto()));
            e.Should().BeOfType<ArgumentException>();
        }
    }
}
