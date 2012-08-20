using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAvail.DataService.Provider
{
    public interface IContextNotifier
    {
        void NotfyItemChanged(string TypeName, NotifyItemChangedData Data);
    }

    public struct NotifyItemChangedData
    {
        public object key;

        public ItemChangedState state;
    }

    public enum ItemChangedState
    {
        Added,
        Deleted,
        Modified,
        Unchanged
    }

}
