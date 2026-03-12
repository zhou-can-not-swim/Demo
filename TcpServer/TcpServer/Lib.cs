using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    public abstract class ViewModelBase : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        protected virtual bool RaiseIfValueChanged<T>(ref T origin, T value, [CallerMemberName] string memberName = null)
        {
            if (!origin.Equals(value))
            {
                origin = value;
                OnPropertyChanged(memberName);
                return true;
            }
            return false;
        }
    }
}
