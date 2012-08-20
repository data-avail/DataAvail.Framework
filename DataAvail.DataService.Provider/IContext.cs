using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAvail.DataService.Provider
{
    public interface IContext
    {
        object DataSource { get; }

        void BeginTransaction();

        void CommitTransaction();

        void RollbackTransaction();
    }
}
