using System;
using System.Collections.Generic;

namespace NHMCore.Utils
{
    // auto properties don't trigger NotifyPropertyChanged so add this shitty boilerplate
    internal class NotifyPropertyChangedHelper<T> where T : IComparable
    {
        public delegate void NotifyPropertyChanged(string name);
        public NotifyPropertyChangedHelper(NotifyPropertyChanged notifyPropertyChanged)
        {
            _notifyPropertyChanged = notifyPropertyChanged;
        }

        NotifyPropertyChanged _notifyPropertyChanged;

        private Dictionary<string, T> _props = new Dictionary<string, T>();

        public T Get(string name)
        {
            return _props.ContainsKey(name) ? _props[name] : default(T);
        }

        public void Set(string name, T value)
        {
            var setIfDifferentOrNotExist = !_props.ContainsKey(name) || Get(name).CompareTo(value) != 0;
            if (setIfDifferentOrNotExist)
            {
                _props[name] = value;
                _notifyPropertyChanged?.Invoke(name);
            }
        }
    }
}
