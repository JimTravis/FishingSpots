using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq.Mapping;
using System.IO.IsolatedStorage;
using System.IO;
using FishingSpots.Resources;
using System.Windows;


namespace FishingSpots.Model
{
    public class FishingSpotsDataContext : DataContext
    {
        // Pass the connection string to the base class.
        public FishingSpotsDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify tables
        public Table<SpotTable> Spots;
        public Table<SpeciesTable> SpeciesItems;
        public Table<BaitTable> BaitItems;
        public Table<RigTable> RigItems;
        public Table<BaitColorTable> BaitColorItems;
        public Table<VisitTable> Visits;
        public Table<VisitPhotosTable> VisitPhotos;

        public void CopyFiles()
        {
            // Obtain the virtual store for the application.
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();

            // Create a stream for the file in the installation folder.
            using (Stream input = Application.GetResourceStream(new Uri("Images/SmTile.png", UriKind.Relative)).Stream)
            {
                // Create a stream for the new file in the local folder.
                using (IsolatedStorageFileStream output = iso.CreateFile("SmTile.png"))
                {
                    // Initialize the buffer.
                    byte[] readBuffer = new byte[4096];
                    int bytesRead = -1;

                    // Copy the file from the installation folder to the local folder. 
                    while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        output.Write(readBuffer, 0, bytesRead);
                    }

                    AppSettings appSet = new AppSettings();

                    appSet.DefaultImage = output.Name;
                }
            }
        }


        // Pre-populate tables with example data
        public void CreateDefaultData()
        {
           
            // Bait types
            List<string> baitlist = new List<string>();
            baitlist.Add("crankbait");
            baitlist.Add("spinnerbait");
            baitlist.Add("minnow");
            baitlist.Add("plastic worm");
            baitlist.Add("rubber frog");
            baitlist.Add("buzzbait");
            baitlist.Add("lure");
            baitlist.Add("swimbait");
            baitlist.Add("shad");
            baitlist.Add("nightcrawler");
            baitlist.Add("dodger");
            baitlist.Add("grub");

            foreach (string s in baitlist)
            {
                BaitTable bt = new BaitTable
                {
                    BaitName = s
                };

                this.BaitItems.InsertOnSubmit(bt);
                
            }

            // Rig types
            List<string> riglist = new List<string>();
            riglist.Add("dropshot");
            riglist.Add("carolina");
            riglist.Add("texas jig");
            riglist.Add("floating");
            riglist.Add("wedding ring");
            riglist.Add("bobber");
            riglist.Add("casting jig");
            riglist.Add("sliding");
            riglist.Add("2-hook pilchard");
            riglist.Add("ledger");

            foreach (string s in riglist)
            {
                RigTable bt = new RigTable
                {
                    RigName = s
                };

                this.RigItems.InsertOnSubmit(bt);

            }

            // Bait colors           
            List<string> colorslist = new List<string>();
            colorslist.Add("white");
            colorslist.Add("yellow");
            colorslist.Add("red");
            colorslist.Add("green");
            colorslist.Add("pink");
            colorslist.Add("purple");
            colorslist.Add("crawdad");
            colorslist.Add("green pumpkin");
            colorslist.Add("orange");
            colorslist.Add("chartreuse");
            colorslist.Add("gold");
            colorslist.Add("silver");

            foreach (string s in colorslist)
            {
                BaitColorTable bct = new BaitColorTable
                {
                    BaitColorName = s
                };

                this.BaitColorItems.InsertOnSubmit(bct);
            }

            // Species
            List<string> specieslist = new List<string>();
            specieslist.Add("bass (smallmouth)");
            specieslist.Add("bass (largemouth)");
            specieslist.Add("trout (rainbow)");
            specieslist.Add("trout (cutthroat)");
            specieslist.Add("salmon (pink)");
            specieslist.Add("salmon (coho)");
            specieslist.Add("steelhead");
            specieslist.Add("perch (yellow)");
            specieslist.Add("walleye");
            specieslist.Add("crappie");
            specieslist.Add("salmon (sockeye)");
            specieslist.Add("salmon (chinook)");

            foreach (string s in specieslist)
            {
                SpeciesTable st = new SpeciesTable
                {
                    SpeciesName = s
                };

                this.SpeciesItems.InsertOnSubmit(st);
            }
            this.SubmitChanges();

            // Add the app image to the isolated storage
            CopyFiles();
        }

       
    }

    

    [Index(Columns = "Latitude, Longitude")]
    [Table]
    public class SpotTable : INotifyPropertyChanged, INotifyPropertyChanging
    {

        // Define ID: private field, public property, and database column.
        private int _spotItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int SpotItemId
        {
            get { return _spotItemId; }
            set
            {
                if (_spotItemId != value)
                {
                    NotifyPropertyChanging("SpotItemId");
                    _spotItemId = value;
                    NotifyPropertyChanged("SpotItemId");
                }
            }
        }

        // Define SpotName: private field, public property, and database column.
        private string _spotName;

        [Column]
        public String SpotName
        {
            get { return _spotName; }
            set
            {
                if (_spotName != value)
                {
                    NotifyPropertyChanging("SpotName");
                    _spotName = value;
                    NotifyPropertyChanged("SpotName");
                }
            }
        }

        // Define latitude: private field, public property, and database column.
        private double _latitude;

        [Column]
        public Double Latitude
        {
            get { return _latitude; }
            set
            {
                if (_latitude != value)
                {
                    NotifyPropertyChanging("Latitude");
                    _latitude = value;
                    NotifyPropertyChanged("Latitude");
                }
            }
        }

        // Define longitude: private field, public property, and database column.
        private double _longitude;

        [Column]
        public Double Longitude
        {
            get { return _longitude; }
            set
            {
                if (_longitude != value)
                {
                    NotifyPropertyChanging("Longitude");
                    _longitude = value;
                    NotifyPropertyChanged("Longitude");
                }
            }
        }

        // Define visitcount: private field, public property, and database column.
        private UInt16 _visitCount;

        [Column]
        public UInt16 VisitCount
        {
            get { return _visitCount; }
            set
            {
                if (_visitCount != value)
                {
                    NotifyPropertyChanging("VisitCount");
                    _visitCount = value;
                    NotifyPropertyChanged("VisitCount");
                }
            }
        }

        // Define Rating: private field, public property, and database column.
        private double _rating;

        [Column]
        public double Rating
        {
            get { return _rating; }
            set
            {
                if (_rating != value)
                {
                    NotifyPropertyChanging("Rating");
                    _rating = value;
                    NotifyPropertyChanged("Rating");
                }
            }
        }

       

        // Define Description: private field, public property, and database column.
        private string _description;

        [Column]
        public String Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    NotifyPropertyChanging("Description");
                    _description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        // Define Photo: private field, public property, and database column.
        // Path in isolated storage for picture.
        private string _photo;

        [Column]
        public String Photo
        {
            get { return _photo; }
            set
            {
                if (_photo != value)
                {
                    NotifyPropertyChanging("Photo");
                    _photo = value;
                    NotifyPropertyChanged("Photo");
                }
            }
        }

        // Define Thumb: private field, public property, and database column.
        // Path in isolated storage for picture.
        private string _thumb;

        [Column]
        public String Thumb
        {
            get { return _thumb; }
            set
            {
                if (_thumb != value)
                {
                    NotifyPropertyChanging("Thumb");
                    _thumb = value;
                    NotifyPropertyChanged("Thumb");
                }
            }
        }


        // Define completion value: private field, public property, and database column.
        private bool _isComplete;

        [Column]
        public bool IsComplete
        {
            get { return _isComplete; }
            set
            {
                if (_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    /// <summary>
    /// SpeciesTable 
    /// Contains the list of species.
    /// </summary>
    [Table]
    public class SpeciesTable : INotifyPropertyChanged, INotifyPropertyChanging
    {

        // Define ID: private field, public property, and database column.
        private int _SpeciesItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int SpeciesItemId
        {
            get { return _SpeciesItemId; }
            set
            {
                if (_SpeciesItemId != value)
                {
                    NotifyPropertyChanging("SpeciesItemId");
                    _SpeciesItemId = value;
                    NotifyPropertyChanged("SpeciesItemId");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private string _speciesName;

        [Column]
        public string SpeciesName
        {
            get { return _speciesName; }
            set
            {
                if (_speciesName != value)
                {
                    NotifyPropertyChanging("SpeciesName");
                    _speciesName = value;
                    NotifyPropertyChanged("SpeciesName");
                }
            }
        }

        // Define completion value: private field, public property, and database column.
        private bool _isComplete;

        [Column]
        public bool IsComplete
        {
            get { return _isComplete; }
            set
            {
                if (_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    /// <summary>
    /// BaitTable 
    /// Contains the list of bait types.
    /// </summary>
    [Table]
    public class BaitTable : INotifyPropertyChanged, INotifyPropertyChanging
    {

        // Define ID: private field, public property, and database column.
        private int _baitItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int BaitItemId
        {
            get { return _baitItemId; }
            set
            {
                if (_baitItemId != value)
                {
                    NotifyPropertyChanging("BaitItemId");
                    _baitItemId = value;
                    NotifyPropertyChanged("BaitItemId");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private string _baitName;

        [Column]
        public string BaitName
        {
            get { return _baitName; }
            set
            {
                if (_baitName != value)
                {
                    NotifyPropertyChanging("BaitName");
                    _baitName = value;
                    NotifyPropertyChanged("BaitName");
                }
            }
        }

        // Define completion value: private field, public property, and database column.
        private bool _isComplete;

        [Column]
        public bool IsComplete
        {
            get { return _isComplete; }
            set
            {
                if (_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    /// <summary>
    /// RigTable 
    /// Contains the list of rig types.
    /// </summary>
    [Table]
    public class RigTable : INotifyPropertyChanged, INotifyPropertyChanging
    {

        // Define ID: private field, public property, and database column.
        private int _rigItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int RigItemId
        {
            get { return _rigItemId; }
            set
            {
                if (_rigItemId != value)
                {
                    NotifyPropertyChanging("RigItemId");
                    _rigItemId = value;
                    NotifyPropertyChanged("RigItemId");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private string _rigName;

        [Column]
        public string RigName
        {
            get { return _rigName; }
            set
            {
                if (_rigName != value)
                {
                    NotifyPropertyChanging("RigName");
                    _rigName = value;
                    NotifyPropertyChanged("RigName");
                }
            }
        }

        // Define completion value: private field, public property, and database column.
        private bool _isComplete;

        [Column]
        public bool IsComplete
        {
            get { return _isComplete; }
            set
            {
                if (_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    /// <summary>
    /// BaitColorTable 
    /// Contains the list of BaitColor.
    /// </summary>
    [Table]
    public class BaitColorTable : INotifyPropertyChanged, INotifyPropertyChanging
    {

        // Define ID: private field, public property, and database column.
        private int _baitColorItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int BaitColorItemId
        {
            get { return _baitColorItemId; }
            set
            {
                if (_baitColorItemId != value)
                {
                    NotifyPropertyChanging("BaitColorItemId");
                    _baitColorItemId = value;
                    NotifyPropertyChanged("BaitColorItemId");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private string _baitColorName;

        [Column]
        public string BaitColorName
        {
            get { return _baitColorName; }
            set
            {
                if (_baitColorName != value)
                {
                    NotifyPropertyChanging("BaitColorName");
                    _baitColorName = value;
                    NotifyPropertyChanged("BaitColorName");
                }
            }
        }

        // Define completion value: private field, public property, and database column.
        private bool _isComplete;

        [Column]
        public bool IsComplete
        {
            get { return _isComplete; }
            set
            {
                if (_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    /// <summary>
    /// SpotVisits Table 
    /// Contains the spots correlated with visits
    /// </summary>
    [Table]
    public class SpotVisitsTable : INotifyPropertyChanged, INotifyPropertyChanging
    {

        // Define spot ID: private field, public property, and database column.
        private int _spotID;

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public int SpotID
        {
            get { return _spotID; }
            set
            {
                if (_spotID != value)
                {
                    NotifyPropertyChanging("SpotID");
                    _spotID = value;
                    NotifyPropertyChanged("SpotID");
                }
            }
        }

        // Define visit ID: private field, public property, and database column.
        private int _visitID;

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public int VisitID
        {
            get { return _visitID; }
            set
            {
                if (_visitID != value)
                {
                    NotifyPropertyChanging("VisitID");
                    _visitID = value;
                    NotifyPropertyChanged("VisitID");
                }
            }
        }        

        // Define completion value: private field, public property, and database column.
        private bool _isComplete;

        [Column]
        public bool IsComplete
        {
            get { return _isComplete; }
            set
            {
                if (_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    /// <summary>
    /// VisitTable 
    /// Contains the list of visits.
    /// </summary>
    [Index(Columns="VisitDateTime, Rating, Species")]
    [Table]
    public class VisitTable : INotifyPropertyChanged, INotifyPropertyChanging
    {

        // Define ID: private field, public property, and database column.
        private int _visitItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int VisitItemId
        {
            get { return _visitItemId; }
            set
            {
                if (_visitItemId != value)
                {
                    NotifyPropertyChanging("VisitItemId");
                    _visitItemId = value;
                    NotifyPropertyChanged("visitItemId");
                }
            }
        }

        // spot that was visited
        private int _spotID;
        [Column]
        public int SpotID
        {
            get { return _spotID; }
            set
            {
                if (_spotID != value)
                {
                    NotifyPropertyChanging("SpotID");
                    _spotID = value;
                    NotifyPropertyChanged("SpotID");
                }
            }
        }

        // Define item date/time: private field, public property, and database column.
        private DateTime _visitDateTime;

        [Column]
        public DateTime VisitDateTime
        {
            get { return _visitDateTime; }
            set
            {
                if (_visitDateTime != value)
                {
                    NotifyPropertyChanging("VisitDateTime");
                    _visitDateTime = value;
                    NotifyPropertyChanged("VisitDateTime");
                }
            }
        }

        // We'll use an enum for weather
        private string _weather;
        [Column]
        public string Weather
        {
            get { return _weather; }
            set
            {
                if (_weather != value)
                {
                    NotifyPropertyChanging("Weather");
                    _weather = value;
                    NotifyPropertyChanged("Weather");
                }
            }
        }

        // Air temperature
        private double _temp;
        [Column]
        public double Temp
        {
            get { return _temp; }
            set
            {
                if (_temp != value)
                {
                    NotifyPropertyChanging("Temp");
                    _temp = value;
                    NotifyPropertyChanged("Temp");
                }
            }
        }

        // Count of fish caught
        private int _caught;
        [Column]
        public int Caught
        {
            get { return _caught; }
            set
            {
                if (_caught != value)
                {
                    NotifyPropertyChanging("Caught");
                    _caught = value;
                    NotifyPropertyChanged("Caught");
                }
            }
        }

        // Fishing depth
        private double _depth;
        [Column]
        public double Depth
        {
            get { return _depth; }
            set
            {
                if (_depth != value)
                {
                    NotifyPropertyChanging("Depth");
                    _depth = value;
                    NotifyPropertyChanged("Depth");
                }
            }
        }

        // Water temperature
        private double _waterTemp;
        [Column]
        public double WaterTemp
        {
            get { return _waterTemp; }
            set
            {
                if (_waterTemp != value)
                {
                    NotifyPropertyChanging("WaterTemp");
                    _waterTemp = value;
                    NotifyPropertyChanged("WaterTemp");
                }
            }
        }

        // Define BaitColor: private field, public property, and database column.
        private string _baitColor;

        [Column]
        public string BaitColor
        {
            get { return _baitColor; }
            set
            {
                if (_baitColor != value)
                {
                    NotifyPropertyChanging("BaitColor");
                    _baitColor = value;
                    NotifyPropertyChanged("BaitColor");
                }
            }
        }

        // Define bait: private field, public property, and database column.
        private string _bait;

        [Column]
        public string Bait
        {
            get { return _bait; }
            set
            {
                if (_bait != value)
                {
                    NotifyPropertyChanging("Bait");
                    _bait = value;
                    NotifyPropertyChanged("Bait");
                }
            }
        }

        // Define rig: private field, public property, and database column.
        private string _rig;

        [Column]
        public string Rig
        {
            get { return _rig; }
            set
            {
                if (_rig != value)
                {
                    NotifyPropertyChanging("Rig");
                    _rig = value;
                    NotifyPropertyChanged("Rig");
                }
            }
        }

        // Define species: private field, public property, and database column.
        private string _species;

        [Column]
        public string Species
        {
            get { return _species; }
            set
            {
                if (_species != value)
                {
                    NotifyPropertyChanging("Species");
                    _species = value;
                    NotifyPropertyChanged("Species");
                }
            }
        }

        // Define tide: private field, public property, and database column.
        private string _tide;

        [Column]
        public string Tide
        {
            get { return _tide; }
            set
            {
                if (_tide != value)
                {
                    NotifyPropertyChanging("Tide");
                    _tide = value;
                    NotifyPropertyChanged("Tide");
                }
            }
        }

        // Define Rating: private field, public property, and database column.
        private double _rating;

        [Column]
        public double Rating
        {
            get { return _rating; }
            set
            {
                if (_rating != value)
                {
                    NotifyPropertyChanging("Rating");
                    _rating = value;
                    NotifyPropertyChanged("Rating");
                }
            }
        }

        // Define Note
        private string _note;

        [Column]
        public string Note
        {
            get { return _note; }
            set
            {
                if (_note != value)
                {
                    NotifyPropertyChanging("Note");
                    _note = value;
                    NotifyPropertyChanged("Note");
                }
            }
        }

        // Define completion value: private field, public property, and database column.
        private bool _isComplete;

        [Column]
        public bool IsComplete
        {
            get { return _isComplete; }
            set
            {
                if (_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    /// <summary>
    /// VisitPhotos Table 
    /// Contains the photos correlated with visits
    /// </summary>
    [Table]
    public class VisitPhotosTable : INotifyPropertyChanged, INotifyPropertyChanging
    {
        // Define visit ID: private field, public property, and database column.
        private int _visitID;

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public int VisitID
        {
            get { return _visitID; }
            set
            {
                if (_visitID != value)
                {
                    NotifyPropertyChanging("VisitID");
                    _visitID = value;
                    NotifyPropertyChanged("VisitID");
                }
            }
        }

        // Define Photo: private field, public property, and database column.
        // Path in isolated storage for picture.
        private string _photo;

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public String Photo
        {
            get { return _photo; }
            set
            {
                if (_photo != value)
                {
                    NotifyPropertyChanging("Photo");
                    _photo = value;
                    NotifyPropertyChanged("Photo");
                }
            }
        }

        // Define Thumb: private field, public property, and database column.
        // Path in isolated storage for picture thumbnail.
        private string _thumb;

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public String Thumb
        {
            get { return _thumb; }
            set
            {
                if (_thumb != value)
                {
                    NotifyPropertyChanging("Thumb");
                    _thumb = value;
                    NotifyPropertyChanged("Thumb");
                }
            }
        }

        // Define completion value: private field, public property, and database column.
        private bool _isComplete;

        [Column]
        public bool IsComplete
        {
            get { return _isComplete; }
            set
            {
                if (_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

}