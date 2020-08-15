using System;

namespace Aggregates.Extensions
{
    public class SagaWasAborted : Exception
    {
        public Messages.IMessage Originating { get; }
        public SagaWasAborted(Messages.IMessage original)
        {
            Originating = original;
        }
    }
}
