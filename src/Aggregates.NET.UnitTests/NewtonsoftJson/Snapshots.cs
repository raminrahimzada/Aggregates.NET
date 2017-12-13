using Aggregates.Contracts;
using Aggregates.Internal;
using Aggregates.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates.NewtonsoftJson
{
    [TestFixture]
    public class Snapshots
    {
        class Simple
        {
            public string Test { get; set; }
        }
        class Private
        {
            public string Test { get; private set; }

            // JsonNet will detect a constructor and use that so don't use constructor
            public void setTest(string test) { Test = test; }
        }

        private Moq.Mock<IEventMapper> _mapper;
        private Moq.Mock<IEventFactory> _factory;

        private JsonMessageSerializer _serializer;

        [SetUp]
        public void Setup()
        {
            _mapper = new Moq.Mock<IEventMapper>();
            _factory = new Moq.Mock<IEventFactory>();

            _serializer = new JsonMessageSerializer(_mapper.Object, _factory.Object);
        }

        [Test]
        public void simple_object()
        {
            var obj = new Simple { Test = "test" };

            var serialized = _serializer.Serialize(obj);

            var deserialized = _serializer.Deserialize<Simple>(serialized);

            Assert.AreEqual(obj.Test, deserialized.Test);
        }

        [Test]
        public void private_object()
        {
            var obj = new Private();
            obj.setTest("test");

            var serialized = _serializer.Serialize(obj);

            var deserialized = _serializer.Deserialize<Private>(serialized);

            Assert.AreEqual(obj.Test, deserialized.Test);
        }
    }
}
