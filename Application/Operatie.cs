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
    
    
    public partial class Operatie : INotifyPropertyChanged
    
    {
        public event PropertyChangedEventHandler PropertyChanged;
    
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    
    	private int _id;
    
    
        public int Id { get { return _id; } set { _id = value;  OnPropertyChanged(); } }
    	private int _reperid;
    
    
        public int ReperId { get { return _reperid; } set { _reperid = value;  OnPropertyChanged(); } }
    	private string _nume;
    
    
        public string Nume { get { return _nume; } set { _nume = value;  OnPropertyChanged(); } }
    	private string _masina;
    
    
        public string Masina { get { return _masina; } set { _masina = value;  OnPropertyChanged(); } }
    	private int _durata;
    
    
        public int Durata { get { return _durata; } set { _durata = value;  OnPropertyChanged(); } }
    
        public virtual Reper Repere { get; set; }
    }
}
