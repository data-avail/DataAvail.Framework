using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAvail.DataService.Provider
{
    public interface IRepositoryValidate
    {
        void ValidateUpdate(object Item);
    }
}
