﻿using System;

namespace Aggregates
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
    public class Versioned : Attribute
    {
        public string Name { get; }
        public string Namespace { get; }
        public int Version { get; }

        public Versioned(string name, string @namespace, int version = 1)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(@namespace))
                throw new ArgumentNullException(nameof(@namespace));
            if (version < 1)
                throw new ArgumentOutOfRangeException(nameof(version), "Version must be > 1");

            Name = name;
            Namespace = @namespace;
            Version = version;
        }
    }
}
