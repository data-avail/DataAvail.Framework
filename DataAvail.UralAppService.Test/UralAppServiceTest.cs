using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataAvail.UralAppService.Models;
using DataAvail.UralAppService.Repositories;


namespace DataAvail.UralAppService.Test
{
    [TestClass]
    public class UralAppServiceTest
    {
        [TestMethod]
        public void Update_Product0_name_to_nill()
        {

            
            AutoMapper.Mapper.CreateMap<DataAvail.UralAppModel.Product, Product>()
                .ForMember(p => p.id, opt => opt.MapFrom(p => p.Id))
                .ForMember(p => p.name, opt => opt.MapFrom(p => p.Name));

            AutoMapper.Mapper.CreateMap<Product, DataAvail.UralAppModel.Product>()
                .ForMember(p => p.Id, opt => opt.MapFrom(p => p.id))
                .ForMember(p => p.Name, opt => opt.MapFrom(p => p.name));

            Product product = new Product { id = 0, name = "hip" };

            using (var model = new UralAppModel.Model())
            {
                ProductRepository repo = new ProductRepository();

                repo.SetContext(model);

                repo.GetAll(new Microsoft.Data.Services.Toolkit.QueryModel.ODataQueryOperation());

                repo.Save(product);
            }


            /*
            UralAppServ.DataServiceProvider prr = new UralAppServ.DataServiceProvider(new Uri(@"http://localhost:3360/Service.svc/"));
            var r = prr.Products.Where(p => p.id == 0).First();
            r.name = "nill";
            //prr.AttachTo("Products", r);
            prr.UpdateObject(r);
            prr.SaveChanges();
             */
        }

    }
}
