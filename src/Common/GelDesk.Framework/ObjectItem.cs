using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public class ObjectItem : PropertyChangedBase, INotifyPropertyChanged
    {
        readonly ObjectSet _owner;
        JToken _data;
    }
}
