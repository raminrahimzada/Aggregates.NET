using System;
using System.Collections.Generic;
using System.Text;
using Aggregates.DI;

namespace Aggregates.Contracts
{
    interface INeedContainer
    {
        TinyIoCContainer Container { get; set; }
    }
}
