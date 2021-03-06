﻿namespace ionix.Data
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public enum BatchCommandMode : int
    {
        Batch = 0,
        Single = 1
    }

    public interface IBatchCommandExecute
    {
        int Execute<TEntity>(IEnumerable<TEntity> entityList, BatchCommandMode mode, IEntityMetaDataProvider provider);

    }
    public abstract class BatchCommandExecute : IBatchCommandExecute
    {
        protected BatchCommandExecute(IDbAccess dataAccess)
        {
            if (null == dataAccess)
                throw new ArgumentNullException(nameof(dataAccess));

            this.DataAccess = dataAccess;
        }
        public IDbAccess DataAccess { get; }

        public abstract int Execute<TEntity>(IEnumerable<TEntity> entityList, BatchCommandMode mode, IEntityMetaDataProvider provider);
    }

    public interface IBatchCommandUpdate
    {
        HashSet<string> UpdatedFields { get; set; }
        int Update<TEntity>(IEnumerable<TEntity> entityList, BatchCommandMode mode, IEntityMetaDataProvider provider);
    }

    public interface IBatchCommandInsert
    {
        HashSet<string> InsertFields { get; set; }
        int Insert<TEntity>(IEnumerable<TEntity> entityList, BatchCommandMode mode, IEntityMetaDataProvider provider);
    }

    public interface IBatchCommandUpsert
    {
        HashSet<string> UpdatedFields { get; set; }
        HashSet<string> InsertFields { get; set; }
        int Upsert<TEntity>(IEnumerable<TEntity> entityList, BatchCommandMode mode, IEntityMetaDataProvider provider);
    }

    public interface IBatchCommandDelsert
    {
        SqlQuery DeleteQuery { get; set; }
        HashSet<string> InsertFields { get; set; }
        int Delsert<TEntity>(IEnumerable<TEntity> entityList, BatchCommandMode mode, IEntityMetaDataProvider provider);
    }
}
