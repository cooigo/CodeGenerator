﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cooigo.CodeGenerator
{
    public abstract class BindableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged !=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void SetProperty<T>(ref T item, T value,[CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(item,value))
            {
                item = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}
