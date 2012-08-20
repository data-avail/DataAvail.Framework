using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAvail.UralAppService.Models;

namespace DataAvail.UralAppService
{
    public class DataServiceProvider : DataAvail.DataService.Provider.DataServiceProvider
    {
        public DataServiceProvider()
            : base(new UralAppModel.Model(), "DataAvail.UralAppService.Repositories.{0}Repository")
        {
        }

        public IQueryable<Product> Products
        {
            get
            {
                return base.CreateQuery<Product>();
            }
        }

        public IQueryable<Producer> Producers
        {
            get
            {
                return base.CreateQuery<Producer>();
            }
        }

        public IQueryable<Tag> Tags
        {
            get
            {
                return base.CreateQuery<Tag>();
            }
        }


    }
}