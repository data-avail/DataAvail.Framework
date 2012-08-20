using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAvail.DataService.Provider;
using DataAvail.UralAppService.Models;

namespace DataAvail.UralAppService.Repositories
{
    public class TagRepository : Repository<DataAvail.UralAppModel.Tag, Tag>
    {
        public IQueryable<Tag> GetTagsByProduct(string id)
        {
            int productId = int.Parse(id);

            return LinqMapper.Mapper.Map<DataAvail.UralAppModel.Tag, Tag>(((DataAvail.UralAppModel.Model)Context).ProductTagMaps.Where(p => p.ProductId == productId).Select(p => p.Tag));
        }

    }
}