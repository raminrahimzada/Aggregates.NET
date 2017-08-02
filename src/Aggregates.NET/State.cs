using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aggregates.Contracts;

namespace Aggregates
{
    public class State : IState
    {
        public Id Id { get; internal set; }
        public long Version { get; internal set; }

    }
}
