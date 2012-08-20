using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAvail.DataService.Provider;
using DataAvail.UralAppService.Models;

namespace DataAvail.UralAppService.Repositories
{
    public class ProducerRepository : Repository<DataAvail.UralAppModel.Producer, Producer>
    {
        public void CreateRelation(object Target, object RelatedItem, string PropertyName)
        {
            Producer producer = (Producer)Target;

            if (PropertyName == "Products")
            {
                var product = ((UralAppModel.Model)Context).Products.Single(p => p.Id == ((Product)RelatedItem).id);

                product.ProducerId = producer.id;

                Context.SaveChanges();
            }
        }

        public void DeleteRelation(object Target, object RelatedItem, string PropertyName)
        {
            Producer producer = (Producer)Target;

            if (PropertyName == "Products")
            {
                var product = ((UralAppModel.Model)Context).Products.Single(p => p.Id == ((Product)RelatedItem).id);

                product.ProducerId = null;

                Context.SaveChanges();
            }            
        }

    }
}