using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner
{
    // https://stackoverflow.com/questions/10156991/inotifypropertychanged-causes-cross-thread-error
    // http://www.claassen.net/geek/blog/2007/07/generic-asynchronous.html
    public static class ControlBindingsCollectionExtensions
    {
        private class ThreadSafeBinding : INotifyPropertyChanged
        {
            private readonly Control bindingControl;
            INotifyPropertyChanged bindingSource;
            string dataMember;

            public static Binding GetBinding(Control bindingControl,
                                      string propertyName,
                                      INotifyPropertyChanged bindingSource,
                                      string dataMember,
                                      bool formattingEnabled,
                                      DataSourceUpdateMode dataSourceUpdateMode)
            {
                var helper = new ThreadSafeBinding(bindingControl, bindingSource, dataMember);
                return new Binding(propertyName, helper, "Value", formattingEnabled, dataSourceUpdateMode);
            }

            private ThreadSafeBinding(Control bindingControl,
                                INotifyPropertyChanged bindingSource,
                                string dataMember)
            {
                this.bindingControl = bindingControl;
                this.bindingSource = bindingSource;
                this.dataMember = dataMember;
                bindingSource.PropertyChanged
                  += new PropertyChangedEventHandler(bindingSource_PropertyChanged);
            }

            void bindingSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (PropertyChanged != null && e.PropertyName == dataMember)
                {
                    if (bindingControl.InvokeRequired)
                    {
                        bindingControl.Invoke(
                          new PropertyChangedEventHandler(bindingSource_PropertyChanged),
                          sender,
                          e);
                        return;
                    }
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }

            /// <summary>
            /// The current value of the data sources' datamember
            /// </summary>
            public object Value
            {
                get
                {
                    return bindingSource.GetType().GetProperty(dataMember)
                      .GetValue(bindingSource, null);
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }



        public static Binding AddSafeBinding(this ControlBindingsCollection cbc, string propertyName, INotifyPropertyChanged dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode)
        {
            var threadSafeBinding = ThreadSafeBinding.GetBinding(cbc.Control, propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode);
            cbc.Add(threadSafeBinding);
            return threadSafeBinding;
        }

    }
}
