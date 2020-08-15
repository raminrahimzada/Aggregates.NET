﻿using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Aggregates.Common
{
    public class Timer : Test
    {
        private class FakeState
        {
            public int counter;
        }

        [Fact]
        public async Task ShouldExecuteRepeatingTimerRepeatedly()
        {
            int count = 0;
            var task = Internal.Timer.Repeat(() =>
            {
                count++;
                return Task.CompletedTask;
            }, TimeSpan.FromMilliseconds(1), "test");

            await Task.Delay(200).ConfigureAwait(false);

            task.IsCompleted.Should().BeFalse();
            count.Should().BeGreaterOrEqualTo(2);
        }
        [Fact]
        public async Task ShouldCancelRepeatingTimer()
        {
            int count = 0;
            var cancellation = new CancellationTokenSource();

            var task = Internal.Timer.Repeat(() =>
            {
                count++;
                return Task.CompletedTask;
            }, TimeSpan.FromMilliseconds(1), cancellation.Token, "test");

            await Task.Delay(200).ConfigureAwait(false);

            cancellation.Cancel();

            await Task.Delay(5).ConfigureAwait(false);

            task.IsCompleted.Should().BeTrue();
            count.Should().BeGreaterOrEqualTo(1);
        }
        [Fact]
        public async Task ShouldRepeatTaskEvenAfterException()
        {
            int count = 0;
            var task = Internal.Timer.Repeat(() =>
            {
                count++;
                throw new InvalidOperationException();
            }, TimeSpan.FromMilliseconds(1), "test");

            await Task.Delay(200).ConfigureAwait(false);

            task.IsCompleted.Should().BeFalse();
            count.Should().BeGreaterOrEqualTo(2);
        }
        [Fact]
        public async Task ShouldProvideStateWhileRepeating()
        {
            // needs to be a reference type for this purpose
            var fake = new FakeState();
            var task = Internal.Timer.Repeat(state =>
            {
                var _count = state as FakeState;
                _count.counter++;
                return Task.CompletedTask;
            }, fake, TimeSpan.FromMilliseconds(1), "test");

            await Task.Delay(200).ConfigureAwait(false);

            task.IsCompleted.Should().BeFalse();
            fake.counter.Should().BeGreaterOrEqualTo(2);
        }
        [Fact]
        public async Task ShouldExpireTimer()
        {
            bool expired = false;
            var task = Internal.Timer.Expire(() =>
            {
                expired = true;
                return Task.CompletedTask;
            }, TimeSpan.FromMilliseconds(1), "test");

            await Task.Delay(200).ConfigureAwait(false);

            expired.Should().BeTrue();
        }
        [Fact]
        public async Task ShouldNotExpireCanceledTimer()
        {
            bool expired = false;
            var cancellation = new CancellationTokenSource();
            var task = Internal.Timer.Expire(() =>
            {
                expired = true;
                return Task.CompletedTask;
            }, TimeSpan.FromMilliseconds(100), cancellation.Token, "test");

            await Task.Delay(5).ConfigureAwait(false);

            cancellation.Cancel();

            await Task.Delay(200).ConfigureAwait(false);

            task.IsCompletedSuccessfully.Should().BeTrue();
            expired.Should().BeFalse();
        }
        [Fact]
        public async Task ShouldIgnoreThrownExceptionsWhileExpiring()
        {
            var task = Internal.Timer.Expire(() =>
            {
                throw new InvalidOperationException();
            }, TimeSpan.FromMilliseconds(10), "test");

            await Task.Delay(200).ConfigureAwait(false);

            task.IsCompletedSuccessfully.Should().BeTrue();
        }
        [Fact]
        public async Task ShouldExpireTimerWithState()
        {
            // needs to be a reference type for this purpose
            var fake = new FakeState();
            var task = Internal.Timer.Expire(state =>
            {
                var _count = state as FakeState;
                _count.counter++;
                return Task.CompletedTask;
            }, fake, TimeSpan.FromMilliseconds(1), "test");

            await Task.Delay(200).ConfigureAwait(false);

            task.IsCompleted.Should().BeTrue();
            fake.counter.Should().Be(1);
        }
    }
}
