using System;
using System.Collections.Generic;
using System.Text;

namespace Aggregates.Contracts
{
    interface IFullMessage
    {
        object Message { get; }
        IDictionary<string,string> Headers { get; }
    }
}
