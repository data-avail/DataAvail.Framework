using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services.Common;
using Microsoft.Data.Services.Toolkit.QueryModel;

namespace DataAvail.UralAppService.Models
{
    [DataServiceKey("id")]
    public class Producer
    {
        public int id { get; set; }

        public string name { get; set; }

        [ForeignProperty]
        public IEnumerable<Product> Products { get; set; }
    }
}