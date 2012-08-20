using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAvail.UralAppService.Models;
using DataAvail.DataService.Provider;
using System.Data.Objects;
using System.Collections;
using DataAvail.LinqMapper;

namespace DataAvail.UralAppService.Repositories
{

    public class ProductRepository : Repository<DataAvail.UralAppModel.Product, Product>
    {
        public void CreateRelation(object Target, object RelatedItem, string PropertyName)
        {
            Product product = (Product)Target;

            if (PropertyName == "Tags")
            {
                var map = ((UralAppModel.Model)Context).ProductTagMaps.SingleOrDefault(p => p.ProductId == product.id && p.TagId == ((Tag)RelatedItem).id);

                if (map == null)
                {
                    map = new UralAppModel.ProductTagMap { ProductId = ((Product)Target).id, TagId = ((Tag)RelatedItem).id };

                    ((UralAppModel.Model)Context).AddToProductTagMaps(map);

                    Context.SaveChanges();
                }
            }
        }

        public void DeleteRelation(object Target, object RelatedItem, string PropertyName)
        {
            Product product = (Product)Target;

            if (PropertyName == "Tags")
            {
                var map = ((UralAppModel.Model)Context).ProductTagMaps.Single(p => p.ProductId == product.id && p.TagId == ((Tag)RelatedItem).id);

                ((UralAppModel.Model)Context).DeleteObject(map);

                
            }

            Context.SaveChanges();
        }

        public void SetRelation(object Target, object RelatedItem, string PropertyName)
        {
            Product product = (Product)Target;

            if (PropertyName == "Producer")
            {
                var modelProduct = ((UralAppModel.Model)Context).Products.SingleOrDefault(p => p.Id == product.id);

                modelProduct.ProducerId = RelatedItem != null ? (int?)((Producer)RelatedItem).id : null;

                Context.ObjectStateManager.GetObjectStateEntry(modelProduct).SetModifiedProperty("ProducerId");

                Context.SaveChanges();
            }
        }
    }
}