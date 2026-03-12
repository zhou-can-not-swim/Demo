using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;


namespace HcControlDemo
{

    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<MenuItemModel> menus=new();

        [ObservableProperty]
        public UserControl currentView;

        public MainViewModel()
        {
            InitMenus();
        }
        private void InitMenus()
        {
            Menus = new ObservableCollection<MenuItemModel>
            {
                new MenuItemModel
                {
                    Title="系统管理",
                    Children=new ObservableCollection<MenuItemModel>
                    {
                        new MenuItemModel{Title="用户管理",PageKey="User"},
                        new MenuItemModel{Title="角色管理",PageKey="Role"}
                    }
                },

                new MenuItemModel
                {
                    Title="设备管理",
                    Children=new ObservableCollection<MenuItemModel>
                    {
                        new MenuItemModel{Title="设备列表",PageKey="Device"},
                        new MenuItemModel{Title="设备监控",PageKey="Monitor"}
                    }
                }
            };
        }

        [RelayCommand]
        private void Navigate(string page)
        {
            switch (page)
            {
                case "User":
                    CurrentView = new UserControl1();
                    break;

                case "Role":
                    CurrentView = new UserControl2();
                    break;

                
            }
        }


    }
}
