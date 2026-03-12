using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HcControlDemo
{
    public class MenuItemModel
    {
        public string Title { get; set; }

        public string PageKey { get; set; }

        public ObservableCollection<MenuItemModel> Children { get; set; }
    }
}
