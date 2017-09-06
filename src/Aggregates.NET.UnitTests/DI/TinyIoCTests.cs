using System;
using System.Collections.Generic;
using System.Text;
using Aggregates.DI;
using NUnit.Framework;

namespace Aggregates.UnitTests.DI
{
    [TestFixture]
    public class TinyIoCTests
    {
        interface ITest
        {
            bool Same { get; set; }
        }

        class Test : ITest
        {
            public bool Same { get; set; }
        }

        [SetUp]
        public void Setup()
        {
            TinyIoCContainer.Current.Unregister<ITest>();
        }


        [Test]
        public void di_resolve()
        {
            TinyIoCContainer.Current.Register<ITest>((c, _) => new Test());

            ITest temp;
            Assert.IsTrue(TinyIoCContainer.Current.TryResolve<ITest>(out temp));
        }

        [Test]
        public void di_no_resolve()
        {
            ITest temp;
            Assert.IsFalse(TinyIoCContainer.Current.TryResolve<ITest>(out temp));
        }
        

        [Test]
        public void multi_instance()
        {
            TinyIoCContainer.Current.Register<ITest, Test>().AsMultiInstance();

            var test = TinyIoCContainer.Current.Resolve<ITest>();
            Assert.IsFalse(test.Same);

            test.Same = true;

            var test2 = TinyIoCContainer.Current.Resolve<ITest>();
            Assert.IsFalse(test2.Same);
        }

        [Test]
        public void child_multi_instance()
        {
            TinyIoCContainer.Current.Register<ITest, Test>().AsMultiInstance();

            var test = TinyIoCContainer.Current.Resolve<ITest>();
            Assert.IsFalse(test.Same);

            test.Same = true;

            var child = TinyIoCContainer.Current.GetChildContainer();
            var test2 = child.Resolve<ITest>();
            Assert.IsFalse(test2.Same);

            child.Register<ITest>(test);

            test2 = child.Resolve<ITest>();
            Assert.IsTrue(test2.Same);
        }
    }
}
