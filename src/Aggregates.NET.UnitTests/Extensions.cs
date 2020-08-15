﻿using FakeItEasy;
using FakeItEasy.Configuration;
using FluentAssertions.Primitives;

namespace Aggregates
{
    public static class FakeItEasyFluentAssertionsExtensions
    {
        public static FakeItEasyCallAssertions Should(this IAssertConfiguration configuration)
        {
            return new FakeItEasyCallAssertions(configuration);
        }
    }

    public class FakeItEasyCallAssertions : ReferenceTypeAssertions<IAssertConfiguration, FakeItEasyCallAssertions>
    {
        private IAssertConfiguration _configuration;

        public FakeItEasyCallAssertions(IAssertConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override string Identifier => "call";
        
        public void HaveHappenedOnce()
        {
            _configuration.MustHaveHappenedOnceExactly();
        }

        public void NotHaveHappened()
        {
            _configuration.MustNotHaveHappened();
        }
    }
    
}
