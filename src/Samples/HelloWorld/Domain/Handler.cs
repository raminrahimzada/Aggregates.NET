using Language;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Aggregates.Extensions;

namespace Domain
{
    public class Handler :
        IHandleMessages<SayHello>
    {
        public async Task Handle(SayHello command, IMessageHandlerContext ctx)
        {
            var world = await ctx.Entities().For<World>().TryGet("World");
            if (world == null)
                world = await ctx.Entities().For<World>().New("World");

            world.SayHello(command.Message);
        }
    }
}
