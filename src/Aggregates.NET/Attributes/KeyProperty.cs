using System;

namespace Aggregates.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class KeyProperty : Attribute
    {
        public KeyProperty(bool always = false)
        {
            Always = always;
        }

        public bool Always { get; }
    }
}
