using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application.Annotations;
using Microsoft.Win32;

namespace Application
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        //aici sunt datele principale folosite de aplicatie local
        private readonly BazaDateAplicatie _bazaDateAplicatie;
        private bool _isLoggedIn;
        private bool _isNotLoggedIn;

        //aici e o variabila care stocheaza calibrul selectat din lista din interfata grafica
        private Calibru _selectedCalibru;

        //aici e o variabila care stocheaza operatia selectata din lista din interfata grafica
        private Operatie _selectedOperatie;

        //aici e o variabila care stocheaza reperul din baza de date
        private Reper _selectedReper;

        //aici salvam utilizatoru logat
        private Utilizator _utilizatorLogat;

        /// <summary>
        ///     Functie care porneste intreaga fereastra
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //aici ii zicem ca dupa ce s-a terminat de pornit fereastra principala
            //sa se execute functia de la dreapta la "=+", MainWindow_Loaded e o functie (de fapt metoda, tot aia)
            Loaded += MainWindow_Loaded;


            //_bazaDateAplicatie e comunicarea noastra cu baza de date, si aici o stabilim
            _bazaDateAplicatie = new BazaDateAplicatie();

            //Aici cream lista cu toate reperele care vor fi aduse din baza de date
            Repere = new ObservableCollection<Reper>();

            //aici sunt niste liste cu operatiile si calibrele, cand selectez un reper din lista de repere
            Operatii = new ObservableCollection<Operatie>();

            //aici ii zic ca atunci cand adaug sau scot o operatii din lista de operatii de la reperul selectat
            // sa ii modific si timpul de executie la reper
            Operatii.CollectionChanged += OperatiiOnCollectionChanged;

            //aici sunt niste liste cu operatiile si calibrele, cand selectez un reper din lista de repere
            Calibre = new ObservableCollection<Calibru>();

            Utilizatori = new ObservableCollection<Utilizator>();

            //aici e ca mai sus, am lista de utilizatori si vreau sa execut o functie cand s-a schimbat
            Utilizatori.CollectionChanged += UtilizatoriOnCollectionChanged;


            IsLoggedIn = false;
        }

        /// <summary>
        ///     Asta este o lista cu calibre sterse, ele trebuie sterse de mana din baza de date
        /// </summary>
        public List<Calibru> CalibreSterse { get; set; }

        public ObservableCollection<Reper> Repere { get; set; }

        /// <summary>
        ///     Aici se retine reperul selectat din lista de repere cand l-am selectat
        ///     sau deselectat
        /// </summary>
        public Reper SelectedReper
        {
            get { return _selectedReper; }
            set
            {
                _selectedReper = value;

                Calibre.Clear();
                Operatii.Clear();

                //daca am selectat un reper, inseamna ca _selectedReper nu e null (null inseamna 0, neasignat, gol)
                if (_selectedReper != null)
                {
                    foreach (var calibru in _selectedReper.Calibre)
                    {
                        //punem in lista de calibre afisate, calibrele din reperul nou selectat
                        Calibre.Add(calibru);
                    }

                    foreach (var operatie in _selectedReper.Operatii)
                    {
                        //punem in lista de operatii afisate, operatiile din reperul nou selectat
                        Operatii.Add(operatie);
                    }
                }

                //aici ii zicem la interfata grafica ca a fost modificat reperul selectat si ca ar trebui sa il afiseze
                OnPropertyChanged();
            }
        }


        /// <summary>
        ///     Asta e o proprietate, sunt 2 functii de fapt, una care modifica daca utilizatorul e logat si alta care imi zice
        /// </summary>
        public bool IsLoggedIn
        {
            //functia asta imi zice daca utilizatorul e logat
            get { return _isLoggedIn; }
            set
            {
                //functia asta seteaza daca utilizatorul e logat
                _isLoggedIn = value;
                IsNotLoggedIn = !value;
                OnPropertyChanged();
            }
        }

        public bool IsNotLoggedIn
        {
            get { return _isNotLoggedIn; }
            set
            {
                _isNotLoggedIn = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Aici e o proprietate care reprezintul utilizatorul adus din baza de date daca cineva e logat in aplicatie
        /// </summary>
        public Utilizator UtilizatorLogat
        {
            get { return _utilizatorLogat; }
            set
            {
                _utilizatorLogat = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Aici e proprietate care modifica calibrul selectat, de aici interfata grafica ia calibrul selectat din lista de
        ///     calibre pentru reper
        ///     si il afiseaza
        /// </summary>
        public Calibru SelectedCalibru
        {
            get { return _selectedCalibru; }
            set
            {
                _selectedCalibru = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Lista de calibre, cand selectez un reper, ia calibrele lui si le pune in lista asta
        ///     Mai departe componenta grafica de listat din interfata grafica care se cheama "lstCalibre" , ia lista asta
        ///     si afiseaza ce e in ea
        ///     E mai speciala un pic lista asta ca interfata grafica stie cand s-a modificat.
        /// </summary>
        public ObservableCollection<Calibru> Calibre { get; set; }

        /// <summary>
        ///     Tot o lista ca una de mai sus dar cu operatiile din reperul selectat, componenta grafica
        ///     "lstOperatii" pentru listat e legata de lista asta si cand punem
        ///     ceva nou in lista asta, o sa puna si componenta
        ///     grafica un rand nou, la fel e si mai sus
        /// </summary>
        public ObservableCollection<Operatie> Operatii { get; set; }

        /// <summary>
        ///     Aici e lista de utilizatori inregistrati in baza de date,
        ///     este umpluta cu utilizatori dupa ce se logheaza cineva
        /// </summary>
        public ObservableCollection<Utilizator> Utilizatori { get; set; }

        /// <summary>
        ///     Aici e o proprietate care imi zice operatia selectata din interfata grafica, de aici
        ///     se ia operatia si se afiseaza in interfata grafica
        /// </summary>
        public Operatie SelectedOperatie
        {
            get { return _selectedOperatie; }
            set
            {
                if (_selectedOperatie != null)
                {
                    //aici ii zic la operatia precedent selectata sa nu mai modifice si timpul de executie din reper ca nu se mai poate modifica
                    _selectedOperatie.PropertyChanged -= SelectedOperatieOnPropertyChanged;
                }

                _selectedOperatie = value;

                if (_selectedOperatie != null)
                {
                    //aici ii zic la noua operatie selectata, daca a fost modificat durata ei, sa modifice si timpul de executie al reperului
                    //SelectedOperatieOnPropertyChanged e chiar o functie
                    _selectedOperatie.PropertyChanged += SelectedOperatieOnPropertyChanged;
                }

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Functie care se executa cand adaug sau sterg o operatie la un reper sa
        ///     ii modifice timpul de executie la reper
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="notifyCollectionChangedEventArgs"></param>
        private void OperatiiOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (SelectedReper != null)
            {
                SelectedReper.TimpExecutie = 0;

                //aici parcurg lista cu toate operatiile din reperul curent si adaug durata la timpul de executie de la reperul selectat
                foreach (var operatie in Operatii)
                {
                    SelectedReper.TimpExecutie += operatie.Durata;
                }
            }
        }

        /// <summary>
        ///     Asta e o functie care se executa cand am pus sau sters un utizator din lista de utilizatori
        ///     Scopul ei e de a atasa de fiecare utilizator adus din baza de date o functie care sa ii schimbe
        ///     si in baza de date proprietatile daca schimbam ceva in interfata grafica, gen IsAdmin(Administrator) sau
        ///     IsDefault(Predefinit)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="notifyCollectionChangedEventArgs"></param>
        private void UtilizatoriOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                var utilizatori = notifyCollectionChangedEventArgs.NewItems.Cast<Utilizator>();

                foreach (var utilizator in utilizatori)
                {
                    //pentru utilizatorii noi, ii legam de functia/metoda care ii schimba proprietatile in baza de date
                    utilizator.PropertyChanged += UtilizatorOnPropertyChanged;
                }
            }

            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                var utilizatori = notifyCollectionChangedEventArgs.OldItems.Cast<Utilizator>();

                foreach (var utilizator in utilizatori)
                {
                    //pentru utilizatorii noi, ii deconectam de functia/metoda care ii schimba proprietatile in baza de date
                    utilizator.PropertyChanged -= UtilizatorOnPropertyChanged;
                }
            }
        }

        /// <summary>
        ///     Functie care daca un utilizator a fost modificat din interfata grafica, il modifica si in baza de date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void UtilizatorOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var utilizator = (Utilizator) sender;

            _bazaDateAplicatie.Utilizatori.Attach(utilizator);

            //aici ii zicem fortat la conexiunea/comunicarea cu baza de date ca utilizatorul este modificat
            _bazaDateAplicatie.Entry(utilizator).State = EntityState.Modified;

            //aici ii zicem sa aplice schimbarile la utilizator si in baza de date
            _bazaDateAplicatie.SaveChanges();
        }

        /// <summary>
        ///     Functia asta se executa cand modific o operatie, ea reface timpul total
        ///     de durata al unui reper ca suma a duratelor operatiilor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void SelectedOperatieOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Operatie.Durata) && SelectedReper != null)
            {
                SelectedReper.TimpExecutie = 0;

                //aici parcurg lista cu toate operatiile din reperul curent si adaug durata la timpul de executie de la reperul selectat
                foreach (var operatie in Operatii)
                {
                    SelectedReper.TimpExecutie += operatie.Durata;
                }
            }
        }

        /// <summary>
        ///     Functie care dupa ce s-a deschis fereastra blocheaza casutele de text si listele de repere pana cand
        ///     utilizatorul se logheaza
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFields(true);
        }

        private void ResetSelectedReper()
        {
            SelectedReper = new Reper();
            SelectedOperatie = new Operatie();
            SelectedCalibru = new Calibru();
        }

        /// <summary>
        ///     Functia asta e folosita dupa logare sa aduca din baza de date reperele cu calibre si operatii
        ///     sau dupa delogare sa blocheze interfata grafica pana cand cineva se logheaza din nou
        /// </summary>
        /// <param name="block"></param>
        private void UpdateFields(bool block)
        {
            if (block)
            {
                //se curata listele cu repere, calibre si operatii daca utilizatorul s-a delogat
                Repere.Clear();
                Operatii.Clear();
                Calibre.Clear();
                Utilizatori.Clear();
                ResetSelectedReper();
            }
            else
            {
                //aici aduc repere din baza de date si am grija cu Include sa aduc si calibrele si operatiile la fiecare reper
                var repereInBazaDate = _bazaDateAplicatie.Repere.Include(r => r.Calibre).Include(r => r.Operatii);

                foreach (var reper in repereInBazaDate)
                {
                    Repere.Add(reper);
                }

                ResetSelectedReper();

                //aici aduc utilizatorii din baza de date
                var utilizatoriInBazaDate = _bazaDateAplicatie.Utilizatori;

                foreach (var utilizator in utilizatoriInBazaDate)
                {
                    Utilizatori.Add(utilizator);
                }
            }

            //fiecare componenta din interfata cu utilizatorul are in interiorul ei alte componente
            //GetChildrenCount iti aduce numarul de componente care le contine ea
            //"tbctrlAplicatie" e componenta mare aia care are taburile, butoanele alea de sus
            // care schimba ce afiseaza, lista de repere sau lista de utilizatori
            //ea contine toate celelalte componente
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(tbctrlAplicatie); i++)
            {
                //iar GetChild iti aduce o componente copil din interiorul ei
                var dependencyObject = VisualTreeHelper.GetChild(tbctrlAplicatie, i);

                BlockFieldsRecursive(dependencyObject, block);
            }
        }

        /// <summary>
        ///     Functia asta e apelata cand vreau sa blochez aplicatia ca un utilizator
        ///     sa nu poate sa faca nimic decat sa se logheze sau inregistreze
        ///     Dupa ce e deblocat, tot functia asta deblocheaza interfata grafica
        ///     ca un utilizator logat sa poate sa vada reperele existente
        ///     "block" zice daca sa blocheze sau sa deblocheze interfata
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="block"></param>
        private void BlockFieldsRecursive(DependencyObject dependencyObject, bool block)
        {
            if (dependencyObject is Button)
            {
                var button = (Button) dependencyObject;

                //butonul din interfata grafica are o proprietate speciala "IsEnabled" care blocheaza butonul
                // sa nu poata fi apasat
                button.IsEnabled = !block;
            }
            else if (dependencyObject is TextBox)
            {
                var textBox = (TextBox) dependencyObject;

                //un chenar in care bag text din interfata grafica are o proprietate speciala "IsReadOnly" care
                //ii zice daca il lasa pe utilizator sa scrie in ea sau nu, daca IsReadOnly este false, atunci il lasa sa scrie, altfel nu
                textBox.IsReadOnly = block;
            }
            else if (dependencyObject is ListView)
            {
                var listView = (ListView) dependencyObject;
                listView.IsEnabled = !block;
            }
            else
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);

                    //e recursiva metoda,
                    BlockFieldsRecursive(child, block);
                }
            }
        }

        /// <summary>
        ///     Functia asta e folosita ca sa anunte componente grafice despre care tot am vorbit
        ///     si interfata grafica ca au aparut niste modificari si ca trebuie sa reafiseze
        ///     continutul lor modificat
        /// </summary>
        /// <param name="propertyName"></param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        ///     Fucntia asta se executa cand se apasa butonul de logare
        ///     Ea se duce in baza de date, cauta un utilizator cu acelasi nume
        ///     ca cel introdus de utilzator,
        ///     il gaseste si verifica parola
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogare_Click(object sender, RoutedEventArgs e)
        {
            var utilizator = txtUtilizator.Text;
            var parola = psbParola.Password;

            //MD5 e un fel de mod de a cripta parola, de a ascunde parola sa nu apara direct in baza de date
            var md5 = MD5.Create();
            var inputBytes = Encoding.Unicode.GetBytes(parola);

            // aici cifram parola efectiv folosind MD5
            var parolaMD5 = md5.ComputeHash(inputBytes);

            var foundUtilizator = false;
            Utilizator utilizatorFound = null;

            //aici aducem toti utilizatorii stocati,retinuti, existenti din baza de date
            //folosind comunicarea cu baza de date "_bazaDateAplicatie"
            var utilizatoriExistenti = from u in _bazaDateAplicatie.Utilizatori
                select u;

            //luam pe rand fiecare utilizator adus din baza de date, asta face foreach tradus inseamna "pentru fiecare"
            foreach (var utilizatorExistent in utilizatoriExistenti)
            {
                //daca am gasit un utilizator in baza de date cu acelasi nume, ne oprim din cautat prin lista de utilizator
                // si incepem sa ii verificam parola
                //
                if (utilizatorExistent.Nume == utilizator && parolaMD5.Length == utilizatorExistent.Parola.Length)
                {
                    var sameParola = true;
                    utilizatorFound = utilizatorExistent;

                    for (var i = 0; i < parolaMD5.Length; i++)
                    {
                        if (parolaMD5[i] != utilizatorExistent.Parola[i])
                        {
                            sameParola = false;
                            break;
                        }
                    }

                    foundUtilizator = sameParola;

                    //aici ne oprim din parcus toti utilizatorii din baza de date
                    break;
                }
            }

            //aici verificam, daca am gasit utilizator stocat in baza de date cu nume si parola exact ca cele introduse de utilizatorul
            // care foloseste aplicatia
            if (foundUtilizator)
            {
                //daca totul a mers bine si am gasit, il logam
                UtilizatorLogat = utilizatorFound;
                UpdateFields(false);
                IsLoggedIn = true;
            }
            else
            {
                // daca nu ii afisam o eroare
                UtilizatorLogat = null;
                UpdateFields(true);
                MessageBox.Show("Utilizator sau parola gresita!");
            }
        }

        /// <summary>
        ///     Functia asta se executa cand utilizatorul da click pe butonul de inregistrare
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInregistrare_OnClick(object sender, RoutedEventArgs e)
        {
            //aici luam parola si numele introduse de utilizator
            var utilizator = txtUtilizator.Text;
            var parola = psbParola.Password;

            //MD5 e un algoritm care genereaza un cod unic pentru un sir binar de orice
            var md5 = MD5.Create();
            var inputBytes = Encoding.Unicode.GetBytes(parola);

            //iar criptam parola cu MD5
            var parolaMD5 = md5.ComputeHash(inputBytes);

            //verificam daca nu cumva exista deja un utiliator retinut in baza de date cu acelasi nume
            // daca da afisam eroare si ne oprim
            foreach (var utilizatorExistent in _bazaDateAplicatie.Utilizatori)
            {
                if (utilizatorExistent.Nume == utilizator)
                {
                    MessageBox.Show("Utilizator exista deja!");
                    return;
                }
            }

            //aici cream un nou utilizator de retinut in baza de date, cu numele si parola introduse
            var newUtilizator = new Utilizator();
            newUtilizator.Nume = utilizator;
            newUtilizator.Parola = parolaMD5;

            //aici salvam in baza de date, folosind comunicare cu baza de date, functia "Add" ii zice ca trebuie sa il puna in baza de date
            // iar SaveChanges il insereaza efectiv in baza de date, il retine/salveaza
            _bazaDateAplicatie.Utilizatori.Add(newUtilizator);
            _bazaDateAplicatie.SaveChanges();
        }

        //Functia asta este apelata cand utilizatorul se delogheaza
        private void btnDelogare_Click(object sender, RoutedEventArgs e)
        {
            UpdateFields(true);
            UtilizatorLogat = null;
            IsLoggedIn = false;
        }

        /// <summary>
        ///     Functia asta se executa cand adaugam un reper nou
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdaugaReperNou_Click(object sender, RoutedEventArgs e)
        {
            ResetSelectedReper();
        }

        /// <summary>
        ///     Functie care valideaza un reper
        /// </summary>
        /// <returns></returns>
        private bool ValideazaReper()
        {
            var errors = new List<string>();

            //aici mai jos facem verificari daca reperul e valid, daca numele nu e gol, string.IsNullOrEmpty verifica daca un text este gol, cum ar fi Nume
            if (string.IsNullOrEmpty(SelectedReper.Nume))
            {
                errors.Add("Reperul trebuie sa aiba un nume.");
            }

            if (SelectedReper.TimpExecutie == 0)
            {
                errors.Add("Reperul trebuie sa aiba un timp de executie.");
            }

            //aici mai jos facem verificari daca reperul e valid, daca materialul nu e gol
            if (string.IsNullOrEmpty(SelectedReper.TipMaterial))
            {
                errors.Add("Reperul trebuie sa aiba un tip de material.");
            }

            if (SelectedReper.PretVanzare == 0)
            {
                errors.Add("Reperul trebuie sa aiba un pret de vanzare.");
            }

            if (SelectedReper.CostProductie == 0)
            {
                errors.Add("Reperul trebuie sa aiba un cost de productie.");
            }

            if (SelectedReper.Calibre.Count == 0)
            {
                errors.Add("Reperul trebuie sa aiba macar un calibru.");
            }

            if (SelectedReper.Operatii.Count == 0)
            {
                errors.Add("Reperul trebuie sa aiba macar o operatie.");
            }

            //daca am gasit macar o eroare, reperul e invalid si afisam mesaj de eroare
            if (errors.Count > 0)
            {
                var bld = new StringBuilder();

                //aici luam pe rand fiecare mesaj de eroare si le combinam impreuna
                bld.Append("Aveti urmatoarele erori in reperul curent:" + Environment.NewLine);

                //trecem prin fiecare mesaj de eroare si il adaugam la un mesaj mai mare de eroare 
                foreach (var error in errors)
                {
                    bld.Append(error + Environment.NewLine);
                }

                //aici se apeleaza functia care da mesajul de eroare, bld.ToString() e mesajul mare de eroare ce contine toate erorile
                MessageBox.Show(bld.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Metoda are adauga o noua operatie cand se apasa butonul de adaugat operatii
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdaugaOperatie_Click(object sender, RoutedEventArgs e)
        {
            //daca nu este inca pusa operatia selectata curent in reper, o punem
            if (!SelectedReper.Operatii.Contains(SelectedOperatie))
            {
                SelectedReper.Operatii.Add(SelectedOperatie);
                Operatii.Add(SelectedOperatie);
            }

            //cream alta operatie noua si o punem ca fiind selectata da nu si in reper, cand da
            SelectedOperatie = new Operatie();
        }


        /// <summary>
        ///     Metoda sau functie care se executa cand apas pe butonul de adaugat un nou calibru
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdaugaCalibru_Click(object sender, RoutedEventArgs e)
        {
            //daca calibrul precedent nu exista in reper, il punem
            if (!SelectedReper.Calibre.Contains(SelectedCalibru))
            {
                SelectedReper.Calibre.Add(SelectedCalibru);
                Calibre.Add(SelectedCalibru);
            }

            //facem apoi alt calibru nou, da nu il punem in reper inca, se pune cand apasa din nou pe buton
            SelectedCalibru = new Calibru();
        }

        private void LstRepere_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var reperSelectat = e.AddedItems.Cast<Reper>().First();
                ResetSelectedReper();
                SelectedReper = reperSelectat;
            }
            else
            {
                SelectedReper = new Reper();
            }
        }

        /// <summary>
        ///     Functie care sterge un reper din baza de date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStergeReper_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedReper.Id != 0)
            {
                //aici ii zicem la conexiunea la baza de date ca trebuie sters
                _bazaDateAplicatie.Repere.Remove(SelectedReper);
                _bazaDateAplicatie.SaveChanges();
                Repere.Remove(SelectedReper);
                ResetSelectedReper();
            }
        }

        private void LstOperatii_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedOperatie = e.AddedItems.Cast<Operatie>().First();

                //modific operatia selectata si o dau pe asta noua
                //interfata grafica o sa vada ca a fost selectat alta si o sa fie afisat asta noua
                SelectedOperatie = selectedOperatie;
            }
        }

        /// <summary>
        ///     Functia asta se executa cand din lista de calibre am apasat pe unul si l-am selectat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LstCalibre_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedCalibru = e.AddedItems.Cast<Calibru>().First();

                //modific calibrul selectat si il dau pe asta nou
                //interfata grafica o sa vada ca a fost selectat altul si o sa fie afisat ala nou
                SelectedCalibru = selectedCalibru;
            }
        }

        /// <summary>
        ///     Functia asta se apeleaza cand sterg un calibru, il sterge din reper si din lista de calibre
        ///     afisate in interfata grafica
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStergeCalibru_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedCalibru != null)
            {
                //stergem calibrul selectat in reper
                SelectedReper.Calibre.Remove(SelectedCalibru);

                //stergem si din lista de calibre afisate utilizatorului
                Calibre.Remove(SelectedCalibru);

                //Marca, calibrul ca fiind sters, sa il sterge, din baza de date
                SelectedReper.CalibreSterse.Add(SelectedCalibru);

                //cream un calibru nou gol si il inlocuim pe cel selectat cu asta gol
                SelectedCalibru = new Calibru();
            }
        }

        /// <summary>
        ///     Functie care se apeleaza pentru a deschide o imagine desen tehnic si asigna-o la un reper
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdaugaImagineDesenTehnic_Click(object sender, RoutedEventArgs e)
        {
            //aici in 4 linii de jos pregatim fereastra de deschis fisiere, ea este facuta deja de la sistemul de operare
            //noi doar o afisam, ii dam tipurile de fisiere si anume imagini de tip jpeg sau png sau bitmap
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Selecteaza o Imagine";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Toate Formatele Suportate|*.jpg;*.jpeg;*.png;*.bmp|" +
                                    "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                                    "Bitmap (*.bmp)|*.bmp|" +
                                    "Portable Network Graphic (*.png)|*.png";

            //aici in if o afisam cu ShowDialog()
            if (openFileDialog.ShowDialog() == true)
            {
                //daca utilizatorul a selectat o imagine, o deschidem
                var image = new BitmapImage(new Uri("file://" + openFileDialog.FileName));

                //asta de mai jos ia imaginea si o transforma in biti care sunt salvati in baza de date cand se salveaza si reperul
                var bmpBitmapEncoder = new BmpBitmapEncoder();
                bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(image));

                using (var ms = new MemoryStream())
                {
                    //aici luam bitii de la imagine si ii asignam la reper
                    bmpBitmapEncoder.Save(ms);
                    SelectedReper.DesenTehnic = ms.ToArray();
                }
            }
        }

        /// <summary>
        ///     Functie care sterge desenul tehnic de la o imagine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStergeImagineDesenTehnic_Click(object sender, RoutedEventArgs e)
        {
            SelectedReper.DesenTehnic = null;
        }


        /// <summary>
        ///     Functie care se apeleaza pentru a deschide o imagine desen 3D si asigna-o la un reper
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdaugaImagineDesen3d_OnClick(object sender, RoutedEventArgs e)
        {
            //aici in 4 linii de jos pregatim fereastra de deschis fisiere, ea este facuta deja de la sistemul de operare
            //noi doar o afisam, ii dam tipurile de fisiere si anume imagini de tip jpeg sau png sau bitmap
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Selecteaza o Imagine";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Toate Formatele Suportate|*.jpg;*.jpeg;*.png;*.bmp|" +
                                    "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                                    "Bitmap (*.bmp)|*.bmp|" +
                                    "Portable Network Graphic (*.png)|*.png";

            //aici in if o afisam cu ShowDialog()
            if (openFileDialog.ShowDialog() == true)
            {
                //daca utilizatorul a selectat o imagine, o deschidem
                var image = new BitmapImage(new Uri("file://" + openFileDialog.FileName));

                //asta de mai jos ia imaginea si o transforma in biti care sunt salvati in baza de date cand se salveaza si reperul
                var bmpBitmapEncoder = new BmpBitmapEncoder();
                bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(image));

                //asta e un fel de fisier in memoria ram, noi scriem imaginea in fisierul asta din ram si apoi luam bitii din el cu ms.ToArray();
                using (var ms = new MemoryStream())
                {
                    //aici luam bitii de la imagine si ii asignam la reper
                    bmpBitmapEncoder.Save(ms);
                    SelectedReper.Desen3d = ms.ToArray();
                }
            }
        }

        private void BtnStergeImagineDesen3d_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedReper.Desen3d = null;
        }

        /// <summary>
        ///     Functia asta salveaza modificarile la reperul selectat in baza de date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSalveazaReper_OnClick(object sender, RoutedEventArgs e)
        {
            //daca nu este valid reperul modificat, nu il salvam, daca nu are calibre sau operatii
            if (!ValideazaReper())
            {
                return;
            }

            //reperul poate sa fie unul nou, sau unul vechi, daca id-ul lui e 0 inseamna ca e nou si trebuie inserat, daca e diferit de 0 inseamna ca e vechi si trebuie modificat si in baza de date
            //Id reprezinta identificatorul unic al repererului din baza de date
            if (SelectedReper.Id != 0)
            {
                _bazaDateAplicatie.Repere.Attach(SelectedReper);
                _bazaDateAplicatie.Entry(SelectedReper).State = EntityState.Modified;

                //marcam operatiile sterse din interfata grafica ca fiind sterse in baza de date daca exista in baza de date, cand au Id-ul diferit de 0
                foreach (var operatie in SelectedReper.OperatiiSterse)
                {
                    if (operatie.Id > 0)
                        _bazaDateAplicatie.Operatii.Remove(operatie);
                }

                //marcam calibrele sterse din interfata grafica ca fiind sterse in baza de date, cand au Id-ul diferit de 0
                foreach (var calibru in SelectedReper.CalibreSterse)
                {
                    if (calibru.Id > 0)
                        _bazaDateAplicatie.Calibre.Remove(calibru);
                }

                _bazaDateAplicatie.SaveChanges();

                SelectedReper.CalibreSterse.Clear();
                SelectedReper.OperatiiSterse.Clear();
            }
            else
            {
                //reperul este nou, il inseram in baza de date
                Repere.Add(SelectedReper);
                _bazaDateAplicatie.Repere.Add(SelectedReper);
                _bazaDateAplicatie.SaveChanges();
            }
        }

        /// <summary>
        ///     Functie care sterge operatia selectata din reper
        ///     Nu il sterge si din baza de date, este sters mai sus in functia de sus BtnSalveazaReper_OnClick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStergeOperatie_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedOperatie != null)
            {
                //Sterge operatia din lista de operatii a reperului selectat
                SelectedReper.Operatii.Remove(SelectedOperatie);

                //Sterge operatia si din lista de operatii afisate in interfata grafica
                Operatii.Remove(SelectedOperatie);

                //Adauga operatia in lista de operatii a reperului care trebuie sterse din baza de date
                SelectedReper.OperatiiSterse.Add(SelectedOperatie);

                //Inlocuieste operatia veche cu una noua, cea veche a fost stearsa
                SelectedOperatie = new Operatie();
            }
        }

        /// <summary>
        ///     Functie care insereaza un nou utilizator inregistrat in baza de date cu parola si numele introduse in casutele de
        ///     text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdaugaUtilizator_OnClick(object sender, RoutedEventArgs e)
        {
            if (txtAdaugaUtilizatorNume.Text.Length > 0 && txtAdaugaUtilizatorParola.Password.Length > 0)
            {
                var utilizator = txtAdaugaUtilizatorNume.Text;
                var parola = txtAdaugaUtilizatorParola.Password;

                var md5 = MD5.Create();
                var inputBytes = Encoding.Unicode.GetBytes(parola);
                var parolaMD5 = md5.ComputeHash(inputBytes);


                foreach (var utilizatorExistent in _bazaDateAplicatie.Utilizatori.Local)
                {
                    if (utilizatorExistent.Nume == utilizator)
                    {
                        MessageBox.Show("Utilizator exista deja!");
                        return;
                    }
                }

                //cream utilizator nou daca nu exista in baza de date
                var newUtilizator = new Utilizator();
                newUtilizator.Nume = utilizator;
                newUtilizator.Parola = parolaMD5;

                //salvam utilizatorul in baza de date
                _bazaDateAplicatie.Utilizatori.Add(newUtilizator);
                _bazaDateAplicatie.SaveChanges();

                Utilizatori.Add(newUtilizator);
            }
        }

        /// <summary>
        ///     Functie care sterge un utilizator din baza de date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStergeUtilizator_Click(object sender, RoutedEventArgs e)
        {
            if (lstUtilizatori.SelectedItems.Count > 0)
            {
                var utilizator = (Utilizator) lstUtilizatori.SelectedItems[0];

                if (!utilizator.IsDefault)
                {
                    //Attach leaga un utilizator inregistrat de conexiunea la baza de date ca sa poate fi sters 
                    _bazaDateAplicatie.Utilizatori.Attach(utilizator);

                    //Aici este marcat ca trebuie sa fie sters utilizatorul
                    _bazaDateAplicatie.Utilizatori.Remove(utilizator);

                    //aici se sterge efectiv
                    _bazaDateAplicatie.SaveChanges();
                    Utilizatori.Remove(utilizator);
                }
                else
                {
                    //daca utilizatorul este predefinit, el nu poate fi sters, trebuie macar sa fie un utilizator in baza de date
                    MessageBox.Show("Utilizatorul este predefinit, nu poate fi sters!");
                }
            }
        }
    }
}