﻿using System;

namespace Aggregates.Extensions
{
    public class SagaAbortionFailureException : Exception
    {
        public Messages.IMessage Originating { get; }

        public SagaAbortionFailureException(Messages.IMessage originating) : 
            base("Failed to run abort commands for saga")
        {
            Originating = originating;
        }
    }
}
