using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataAvail.LinqMapper;
using DataAvail.UralAppService.Models;
using System.Linq.Expressions;


namespace DataAvail.UralAppService.Test
{
    public static class Extensions
    {
        public static IQueryable<DataAvail.UralAppModel.Producer> GetRefProperty(this DataAvail.UralAppModel.Product Product)
        {
            return Product.ProducerReference.CreateSourceQuery();
        }
    }

    [TestClass]
    public class LinqMapperTest
    {
        [TestMethod]
        public void select_products()
        {
            Mapper.CreateMap<DataAvail.UralAppModel.Tag, Tag>()
                .ForMember(p => p.id, opt => opt.MapFrom(p => p.Id))
                .ForMember(p => p.name, opt => opt.MapFrom(p => p.Name));

            //Func<DataAvail.UralAppModel.Tag, Expression<Func<DataAvail.UralAppModel.Tag, Tag>>> func = (p) => Mapper.MapFunc<DataAvail.UralAppModel.Tag, Tag>();
            Expression<Func<DataAvail.UralAppModel.Tag, Tag>> func1 = x => new Tag { id = x.Id, name = x.Name };

            Mapper.CreateMap<DataAvail.UralAppModel.Product, Product>()
                .ForMember(p => p.id, opt => opt.MapFrom(p => p.Id))
                .ForMember(p => p.name, opt => opt.MapFrom(p => p.Name))
                .ForMember(p => p.name, opt => opt.ResolveUsing(p => p.Name + "..."))
                .ForMember(p => p.Tags, opt => opt.ResolveUsing(p => p.ProductTagMaps.AsQueryable().Select(s => s.Tag).Select(Mapper.MapExpression<UralAppModel.Tag, Tag>())));

            /* new Tag { id = x.Tag.Id, name = x.Tag.Name } */

            DataAvail.UralAppModel.Model model = new UralAppModel.Model();
            var q = LinqMapper.Mapper.Map<DataAvail.UralAppModel.Product, Product>(model.Products);
            var r = q.ToArray();
        }

        public class Nest
        {
            public int? id { get; set; } 
        }

        public class Item
        {
            public int id { get; set; }

            public Nest nest { get; set; }

        }


        
        [TestMethod]
        public void select_product_with_producer_ref()
        {
            DataAvail.UralAppModel.Model model = new UralAppModel.Model();
            var q = model.Products
                .Select(p => new Item { id = p.Id, nest = null });

            var r = q.ToArray();
        }
    }
}
