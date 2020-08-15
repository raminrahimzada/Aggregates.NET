﻿using Aggregates.Contracts;
using Aggregates.Internal;
using Aggregates.Messages;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using System;
using System.Linq;

namespace Aggregates
{

    public abstract class Test
    {
        protected IFixture Fixture { get; }
        
        public Test()
        {
            Configuration.Settings = new FakeConfiguration();

            Fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });

            Fixture.Customize<Id>(x => x.FromFactory(() => Guid.NewGuid()));
            Fixture.Customize<IEvent>(x => x.FromFactory(() => new FakeDomainEvent.FakeEvent()));
            Fixture.Customize<FakeEntity>(x => x.FromFactory(() =>
            {
                var factory = EntityFactory.For<FakeEntity>();

                var entity = factory.Create(Defaults.Bucket, Fake<Id>(), null, Many<FakeDomainEvent.FakeEvent>());

                (entity as INeedDomainUow).Uow = Fake<UnitOfWork.IDomain>();
                (entity as INeedEventFactory).EventFactory = Fake<IEventFactory>();
                (entity as INeedStore).Store = Fake<IStoreEvents>();
                (entity as INeedStore).OobWriter = Fake<IOobWriter>();

                return entity;
            }));
            Fixture.Customize<FakeChildEntity>(x => x.FromFactory(() =>
            {
                var factory = EntityFactory.For<FakeChildEntity>();

                var entity = factory.Create(Defaults.Bucket, Fake<Id>(), null, Many<FakeDomainEvent.FakeEvent>());

                (entity as INeedDomainUow).Uow = Fake<UnitOfWork.IDomain>();
                (entity as INeedEventFactory).EventFactory = Fake<IEventFactory>();
                (entity as INeedStore).Store = Fake<IStoreEvents>();
                (entity as INeedStore).OobWriter = Fake<IOobWriter>();

                entity.Parent = Fake<FakeEntity>();
                return entity;
            }));
        }

        protected T Fake<T>() => Fixture.Create<T>();
        protected T[] Many<T>(int count = 3) => Fixture.CreateMany<T>(count).ToArray();
        protected void Inject<T>(T instance) => Fixture.Inject(instance);
    }
}
