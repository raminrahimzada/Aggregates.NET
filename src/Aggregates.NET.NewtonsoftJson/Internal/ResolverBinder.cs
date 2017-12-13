using Aggregates.Contracts;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aggregates.Internal
{
    class EventContractResolver : DefaultContractResolver
    {
        private readonly IEventMapper _mapper;
        private readonly IEventFactory _factory;

        public EventContractResolver(IEventMapper mapper, IEventFactory factory)
        {
            _mapper = mapper;
            _factory = factory;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var mappedTypeFor = objectType;

            if(!mappedTypeFor.IsInterface)
                return base.CreateObjectContract(objectType);
            
            mappedTypeFor = _mapper.GetMappedTypeFor(objectType);

            if (mappedTypeFor == null)
                return base.CreateObjectContract(objectType);

            var objectContract = base.CreateObjectContract(mappedTypeFor);

            objectContract.DefaultCreator = () => _factory.Create(mappedTypeFor);

            return objectContract;
        }
    }

    class EventSerializationBinder : DefaultSerializationBinder
    {
        private readonly IEventMapper _mapper;

        public EventSerializationBinder(IEventMapper mapper)
        {
            _mapper = mapper;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            var mappedType = serializedType;
            if (!serializedType.IsInterface && typeof(Messages.IEvent).IsAssignableFrom(serializedType))
                mappedType = _mapper.GetMappedTypeFor(serializedType) ?? serializedType;

            assemblyName = null;
            typeName = mappedType.AssemblyQualifiedName;
        }
    }
}
