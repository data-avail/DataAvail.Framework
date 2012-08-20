using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;

namespace DataAvail.DataService.Provider
{
    public interface IRepository
    {
        void SetContext(object Context);
    }

    public interface IRepositoryEntity
    {
        object GetEntityKey(object ServiceModel);

        System.Data.EntityState GetEntityState(object ServiceModel);        
    }
}
