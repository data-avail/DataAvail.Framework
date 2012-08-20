using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.Common;

namespace DataAvail.DataService.Provider
{
    public class EntityContext : IContext
    {
        public EntityContext(ObjectContext ObjectContext)
        {
            _objectContext = ObjectContext;

        }

        private readonly ObjectContext _objectContext;

        private DbTransaction _transaction;

        public void BeginTransaction()
        {
            System.Diagnostics.Debug.WriteLine("BeginTransaction");

            if (_objectContext.Connection.State == System.Data.ConnectionState.Closed)
            {
                _objectContext.Connection.Open();
            }

            _transaction = _objectContext.Connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            System.Diagnostics.Debug.WriteLine("CommitTransaction");

            _transaction.Commit();

            _transaction.Dispose();

            _objectContext.Connection.Dispose();

        }

        public void RollbackTransaction()
        {
            System.Diagnostics.Debug.WriteLine("RollbackTransaction");

            _transaction.Rollback();

            _transaction.Dispose();

            _objectContext.Connection.Dispose();

        }

        public object DataSource
        {
            get
            {
                return _objectContext;
            }
        }
    }
}
