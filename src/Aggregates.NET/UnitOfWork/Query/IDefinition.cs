﻿namespace Aggregates.UnitOfWork.Query
{
    public interface IDefinition
    {
        IGrouped[] Operations { get; set; }

        long? Skip { get; set; }
        long? Take { get; set; }
        ISort[] Sort { get; set; }
    }
}
