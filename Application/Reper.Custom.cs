using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Application
{
    public partial class Reper
    {
        public Reper()
        {
            Operatii = new ObservableCollection<Operatie>();
            Calibre = new ObservableCollection<Calibru>();

            OperatiiSterse = new List<Operatie>();
            CalibreSterse = new List<Calibru>();
        }

        public List<Calibru> CalibreSterse { get; set; }

        public List<Operatie> OperatiiSterse { get; set; }
    }
}