//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Application
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Collections.ObjectModel;
    
    
    public partial class Utilizator : INotifyPropertyChanged
    
    {
        public event PropertyChangedEventHandler PropertyChanged;
    
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    
    	private int _id;
    
    
        public int Id { get { return _id; } set { _id = value;  OnPropertyChanged(); } }
    	private string _nume;
    
    
        public string Nume { get { return _nume; } set { _nume = value;  OnPropertyChanged(); } }
    	private byte[] _parola;
    
    
        public byte[] Parola { get { return _parola; } set { _parola = value;  OnPropertyChanged(); } }
    	private bool _isadmin;
    
    
        public bool IsAdmin { get { return _isadmin; } set { _isadmin = value;  OnPropertyChanged(); } }
    	private bool _isdefault;
    
    
        public bool IsDefault { get { return _isdefault; } set { _isdefault = value;  OnPropertyChanged(); } }
    }
}
