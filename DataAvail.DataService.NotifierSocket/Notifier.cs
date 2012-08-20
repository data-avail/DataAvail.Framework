using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalR;
using SignalR.Hubs;

namespace DataAvail.DataService.NotifierSocket
{
    public static class Notifier
    {
        public static void ItemChanged(ItemChangedData Data)
        {
            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<NotifierHub>();

            
            var userData = new ItemChangedUserData { 
                key = Data.key,
                type = Data.type,
                state = Data.state,
                _userId = GetUserId()
            };
           
            //invoke subscribe event on client
            hubContext.Clients.onItemChanged(userData);
        }

        private static string GetUserId()        
        {
            var cookie = System.Web.HttpContext.Current.Request.Cookies["SIGNALR_ID"];

            return cookie != null ? cookie.Value : null;
        }
    }    

    public class ItemChangedData
    {
        public object key { get; set; }

        public string type { get; set; }

        public string state { get; set; }

    }

    public class ItemChangedUserData : ItemChangedData
    {
        public string _userId { get; set; }

        public string _user { get; set; }
    }

}
