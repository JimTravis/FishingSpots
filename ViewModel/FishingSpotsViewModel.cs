using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.IO;
using System;
using System.Windows;
using System.Windows.Navigation;
using Windows.Devices.Geolocation;
using System.Device.Location;


// Directive for the data model.
using FishingSpots.Model;


namespace FishingSpots.ViewModel
{
    // Used for items to display 
    // in the AllVisits list
    // These are the fields that get shown
    // We need to join-in the spot name
    // as it's stored as an ID.
    // See LoadAllVisits().
    public class DisplayVisit
    {
        public string SpotName
        { get; set; }

        public string Species
        { get; set; }

        public DateTime DT
        { get; set; }

        public string Date
        { get; set; }

        public double Rating
        { get; set; }

        public int VisitID
        { get; set; }
    }


    public class FishingSpotsViewModel : INotifyPropertyChanged
    {
        // LINQ to SQL data context for the local database.
        internal FishingSpotsDataContext FishingSpotsDB;

        private AppSettings _settings;
        public AppSettings Settings
        {
            get { return _settings; }
        }

        private BackupMan _backupManager;
        public BackupMan BackupManager
        {
            get { return _backupManager; }
        }

        private GeoCoordinate _currentLocation;
        public GeoCoordinate CurrentLocation
        {
            get { return _currentLocation; }
        }

          
        // The current spot is set by a referring page, for example when
        // an item is selected in the list.
        // The details page can then use it to set data context.
        // It must always contain the most recent spot that a user
        // has selected.
        private SpotTable _currentSpot;
        public SpotTable CurrentSpot
        {
            get { return _currentSpot; }
            set
            {
                _currentSpot = value;
                NotifyPropertyChanged("CurrentSpot");
            }
        }

        // The current visit is set by a referring page, for example when
        // an item is selected in the list.
        // The details page can then use it to set data context.
        // It must always contain the most recent spot that a user
        // has selected.
        private VisitTable _currentVisit;
        public VisitTable CurrentVisit
        {
            get { return _currentVisit; }
            set
            {
                _currentVisit = value;
                NotifyPropertyChanged("CurrentVisit");
            }
        }

        

        // The first time we get a location event, we map to the user's current location.
        // After that, we update the location marker, but we don't want to center
        // the map on the current location automatically. This property tracks
        // whether we've mapped current location for the first time.
        private bool _firstMapLoadCompleted;
        public bool FirstMapLoadCompleted
        {
            get { return _firstMapLoadCompleted; }
            set
            {
                _firstMapLoadCompleted = value;
            }
        }

    

        // Class constructor, create the data context object.
        public FishingSpotsViewModel(string FishingSpotsDBConnectionString) 
        {
            // A bit of defensive programming.
            // At least we won't crash for one of these being null.
            _settings = new AppSettings();
            _currentVisit = new VisitTable();
            _currentSpot = new SpotTable();
            _currentLocation = new GeoCoordinate();
            _visitPhotos = new ObservableCollection<VisitPhotosTable>();
            _spotVisits = new ObservableCollection<VisitTable>();
            _allVisits = new ObservableCollection<DisplayVisit>();
            _allSpots = new ObservableCollection<SpotTable>();
            _allSpecies = new ObservableCollection<SpeciesTable>();
            _allBaitColors = new ObservableCollection<BaitColorTable>();
            _allBaits = new ObservableCollection<BaitTable>();
            _allRigs = new ObservableCollection<RigTable>();
            
            FishingSpotsDB = new FishingSpotsDataContext(FishingSpotsDBConnectionString);

            _backupManager = new BackupMan();
        }

        // All species
        private ObservableCollection<SpeciesTable> _allSpecies;
        public ObservableCollection<SpeciesTable> AllSpecies
        {
            get { return _allSpecies; }
            set
            {
                _allSpecies = value;
                NotifyPropertyChanged("AllSpecies");
            }
        }

        // All baits
        private ObservableCollection<BaitTable> _allBaits;
        public ObservableCollection<BaitTable> AllBaits
        {
            get { return _allBaits; }
            set
            {
                _allBaits = value;
                NotifyPropertyChanged("AllBaits");
            }
        }

        // All rigs
        private ObservableCollection<RigTable> _allRigs;
        public ObservableCollection<RigTable> AllRigs
        {
            get { return _allRigs; }
            set
            {
                _allRigs = value;
                NotifyPropertyChanged("AllRigs");
            }
        }

        // All bait colors
        private ObservableCollection<BaitColorTable> _allBaitColors;
        public ObservableCollection<BaitColorTable> AllBaitColors
        {
            get { return _allBaitColors; }
            set
            {
                _allBaitColors = value;
                NotifyPropertyChanged("AllBaitColors");
            }
        }


      
        // Add a species item to the database and collections.
        public void AddSpecies(SpeciesTable newSpecies)
        {
            // Add an item to the data context.
            FishingSpotsDB.SpeciesItems.InsertOnSubmit(newSpecies);

            // Save changes to the database.
            FishingSpotsDB.SubmitChanges(); 
          
            // Update the observable collection
            AllSpecies.Add(newSpecies);
        }

        // Remove species by id
        public void DeleteSpecies(int speciesID)
        {
            var q = from species in AllSpecies
                    where species.SpeciesItemId == speciesID
                    select species;

            AllSpecies.Remove(q.First() as SpeciesTable);

            var query = from species in FishingSpotsDB.SpeciesItems
                        where species.SpeciesItemId == speciesID
                        select species;

            foreach (SpeciesTable sp in query)
            {
                FishingSpotsDB.SpeciesItems.DeleteOnSubmit(sp);
            }

            FishingSpotsDB.SubmitChanges();
        }

        // Remove bait by id
        public void DeleteBait(int baitID)
        {
            var q = from bait in AllBaits
                    where bait.BaitItemId == baitID
                    select bait;

            AllBaits.Remove(q.First() as BaitTable);

            var query = from bait in FishingSpotsDB.BaitItems
                        where bait.BaitItemId == baitID
                        select bait;

            foreach (BaitTable sp in query)
            {
                FishingSpotsDB.BaitItems.DeleteOnSubmit(sp);
            }

            FishingSpotsDB.SubmitChanges();
        }

        // Remove rig by id
        public void DeleteRig(int rigID)
        {
            var q = from rig in AllRigs
                    where rig.RigItemId == rigID
                    select rig;

            AllRigs.Remove(q.First() as RigTable);

            var query = from rig in FishingSpotsDB.RigItems
                        where rig.RigItemId == rigID
                        select rig;

            foreach (RigTable sp in query)
            {
                FishingSpotsDB.RigItems.DeleteOnSubmit(sp);
            }

            FishingSpotsDB.SubmitChanges();
        }

        // Remove baitColor by id
        public void DeleteBaitColor(int baitColorID)
        {
            var q = from baitColor in AllBaitColors
                    where baitColor.BaitColorItemId == baitColorID
                    select baitColor;

            AllBaitColors.Remove(q.First() as BaitColorTable);

            var query = from baitColor in FishingSpotsDB.BaitColorItems
                        where baitColor.BaitColorItemId == baitColorID
                        select baitColor;

            foreach (BaitColorTable sp in query)
            {
                FishingSpotsDB.BaitColorItems.DeleteOnSubmit(sp);
            }

            FishingSpotsDB.SubmitChanges();
        }


        // Add a species name to the database and collections.
        public void AddSpecies(string newSpecies)
        {
            SpeciesTable sp = new SpeciesTable();
            sp.SpeciesName = newSpecies;

            // Add an item to the data context.
            FishingSpotsDB.SpeciesItems.InsertOnSubmit(sp);

            // Save changes to the database.
            FishingSpotsDB.SubmitChanges();

            // Update the observable collection
            AllSpecies.Add(sp);
        }

        // Add a bait name to the database and collections.
        public void AddBait(string newBait)
        {
            BaitTable bt = new BaitTable();
            bt.BaitName = newBait;

            // Add an item to the data context.
            FishingSpotsDB.BaitItems.InsertOnSubmit(bt);

            // Save changes to the database.
            FishingSpotsDB.SubmitChanges();

            // Update the observable collection
            AllBaits.Add(bt);
        }

        // Add a rig name to the database and collections.
        public void AddRig(string newRig)
        {
            RigTable bt = new RigTable();
            bt.RigName = newRig;

            // Add an item to the data context.
            FishingSpotsDB.RigItems.InsertOnSubmit(bt);

            // Save changes to the database.
            FishingSpotsDB.SubmitChanges();

            // Update the observable collection
            AllRigs.Add(bt);
        }

        // Add a BaitColor name to the database and collections.
        public void AddBaitColor(string newBaitColor)
        {
            BaitColorTable bt = new BaitColorTable();
            bt.BaitColorName = newBaitColor;

            // Add an item to the data context.
            FishingSpotsDB.BaitColorItems.InsertOnSubmit(bt);

            // Save changes to the database.
            FishingSpotsDB.SubmitChanges();

            // Update the observable collection
            AllBaitColors.Add(bt);
        }

        // Add a spot to the database and collections.
        public void AddSpot(SpotTable newSpot)
        {
            // Add a spot item to the data context.
            FishingSpotsDB.Spots.InsertOnSubmit(newSpot);

            _currentSpot = newSpot;

            // Save changes to the database.
            FishingSpotsDB.SubmitChanges();

            // Update the observable collection
            AllSpots.Add(newSpot);            
        }

        // Add a visit to the database and collections.
        public void AddVisit(VisitTable newVisit)
        {
            // Add a visit item to the data context.
            FishingSpotsDB.Visits.InsertOnSubmit(newVisit);
                        
            SpotTable spot = getSpotByID(newVisit.SpotID);
            spot.Rating = ComputeNewRating(newVisit.SpotID, newVisit.Rating);

            // Save changes to the database.
            FishingSpotsDB.SubmitChanges();

            CurrentVisit = newVisit;

            LoadAllVisits();
        }

        // Save a visit to the database and collections.
        public void SaveVisit(VisitTable newVisit)
        {
            Debug.Assert(newVisit != null);
            
            //  Query for the object to be updated
            var query = from visit in FishingSpotsDB.Visits
                        where visit.VisitItemId == newVisit.VisitItemId
                        select visit;

            foreach (VisitTable v in query)
            {
                //  Modify the object
                v.Bait = newVisit.Bait;
                v.Rig = newVisit.Rig;
                v.BaitColor = newVisit.BaitColor;
                v.Caught = newVisit.Caught;
                v.Depth = newVisit.Depth;
                v.Note = newVisit.Note;
                v.Rating = newVisit.Rating;
                v.Species = newVisit.Species;
                v.Temp = newVisit.Temp;
                v.Tide = newVisit.Tide;
                v.WaterTemp = newVisit.WaterTemp;
                v.Weather = newVisit.Weather;

                CurrentVisit = v;
            }

            SpotTable spot = getSpotByID(newVisit.SpotID);
            spot.Rating = ComputeNewRating(newVisit.SpotID, newVisit.Rating);

            // Save changes to the database.
            FishingSpotsDB.SubmitChanges();

            LoadAllVisits();
        }

        // Update the rating value for the spot each time we add a visit
        // This is a function to calculate that new rating
        // It is called before the latest rating is saved.
        public double ComputeNewRating(int spotID, double newRating)
        {
            double i = newRating;

            //  Query for the visits for this spot to be computed.
            var query = from visits in FishingSpotsDB.Visits
                        where visits.SpotID == spotID
                        select visits;

            int count = query.Count<VisitTable>() +1; // Add one for the new rating that isnt' saved yet.

            foreach (VisitTable dbVisit in query)
            {
                i = i + dbVisit.Rating;
            }

            return i / count; // it's an average
        }

        public void DeleteCurrentVisit()
        {
            //  Query for the visits for this spot to be deleted.
            var query = from visits in FishingSpotsDB.Visits
                        where visits.VisitItemId == CurrentVisit.VisitItemId
                        select visits;

            // Delete the visits
            foreach (VisitTable dbVisit in query)
            {
                FishingSpotsDB.Visits.DeleteOnSubmit(dbVisit);
            }

            FishingSpotsDB.SubmitChanges();

            LoadAllVisits();
        }

        public void SaveVisitPhotos(List<VisitPhotosTable> photoURLs)
        {
            foreach (VisitPhotosTable photo in photoURLs)
            {                
                photo.VisitID = CurrentVisit.VisitItemId;
                FishingSpotsDB.VisitPhotos.InsertOnSubmit(photo);
            }
           
            // Save changes to the database.
            FishingSpotsDB.SubmitChanges();

            LoadVisitPhotos();
        }

        // Delete photos from database and isolated storage
        public void DeleteVisitPhotos(List<VisitPhotosTable> vptList)
        {
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();

            foreach (VisitPhotosTable photo in vptList)
            {
                FishingSpotsDB.VisitPhotos.DeleteOnSubmit(photo);
                
                // Delete from isolated storage 
                if (iso.FileExists(photo.Photo))
                {
                    iso.DeleteFile(photo.Photo);
                }

                // Delete from isolated storage 
                if (iso.FileExists(photo.Thumb))
                {
                    iso.DeleteFile(photo.Thumb);
                }
            }

            // Save changes to the database.
            FishingSpotsDB.SubmitChanges();

            LoadVisitPhotos();
        }

        // Delete a spot from the database and collections.
        public void DeleteCurrentSpot()
        {
            Debug.Assert(_currentSpot != null);

            // Delete the pic/thumb from isolated storage
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (CurrentSpot.Photo != null && iso.FileExists(CurrentSpot.Photo) && !CurrentSpot.Photo.Contains("SmTile.png"))
            {
                iso.DeleteFile(CurrentSpot.Photo);
            }

            if (CurrentSpot.Thumb != null && iso.FileExists(CurrentSpot.Thumb) && !CurrentSpot.Thumb.Contains("SmTile.png"))
            {
                iso.DeleteFile(CurrentSpot.Thumb);
            }
    
 
            // Update the observable collection.
            AllSpots.Remove(_currentSpot);

            //  Query for the visits for this spot to be deleted.
            var query = from visits in FishingSpotsDB.Visits
                        where visits.SpotID == _currentSpot.SpotItemId
                        select visits;

            // Delete the visits
            foreach (VisitTable dbVisit in query)
            {
                FishingSpotsDB.Visits.DeleteOnSubmit(dbVisit);
            }

            //  Query for the spot to be deleted.
            var query2 = from spots in FishingSpotsDB.Spots
                        where spots.SpotItemId == _currentSpot.SpotItemId
                        select spots;

            // Delete it
            foreach (SpotTable dbSpot in query2)
            {
                FishingSpotsDB.Spots.DeleteOnSubmit(dbSpot);
            }

            FishingSpotsDB.SubmitChanges();

            // It's gone, so get rid of the current spot reference.
            _currentSpot = null;
        }

        // Get a spot by ID
        public SpotTable getSpotByID(long ID)
        {
            //  Query for the object to be updated
            var query = from spots in FishingSpotsDB.Spots
                        where spots.SpotItemId == ID
                        select spots;

            SpotTable spot = query.FirstOrDefault<SpotTable>();
            Debug.Assert (spot != null);
            return spot;
        }

        // Save the current spot to the database.
        // CurrentSpot is always already in the observable collection.
        public void SaveCurrentSpot()
        {
            Debug.Assert(_currentSpot != null);

            //  Query for the object to be updated
            var query = from spots in FishingSpotsDB.Spots
                        where spots.SpotItemId == _currentSpot.SpotItemId
                        select spots;

            foreach (SpotTable dbSpot in query)
            {
                //  Modify the object
                dbSpot.SpotName = _currentSpot.SpotName;
                dbSpot.Description = _currentSpot.Description;
                dbSpot.Rating = _currentSpot.Rating;
                dbSpot.VisitCount = _currentSpot.VisitCount;                
                dbSpot.Latitude = _currentSpot.Latitude;
                dbSpot.Longitude = _currentSpot.Longitude;
                dbSpot.Photo = _currentSpot.Photo;
                dbSpot.Thumb = _currentSpot.Thumb;
            }

            // Save changes to the database.
            FishingSpotsDB.SubmitChanges();

            LoadSpots();
        }

        // All spots
        private ObservableCollection<SpotTable> _allSpots;
        public ObservableCollection<SpotTable> AllSpots
        {
            get { return _allSpots; }
            set
            {
                _allSpots = value;
                NotifyPropertyChanged("AllSpots");
            }
        }

        // Spot visits
        private ObservableCollection<VisitTable> _spotVisits;
        public ObservableCollection<VisitTable> SpotVisits
        {
            get { return _spotVisits; }
            set
            {
                _spotVisits = value;
                NotifyPropertyChanged("SpotVisits");
            }
        }

        // All visits
        private ObservableCollection<DisplayVisit> _allVisits;
        public ObservableCollection<DisplayVisit> AllVisits
        {
            get { return _allVisits; }
            set
            {
                _allVisits = value;
                NotifyPropertyChanged("AllVisits");
            }
        }

        // Visit photos
        private ObservableCollection<VisitPhotosTable> _visitPhotos;
        public ObservableCollection<VisitPhotosTable> VisitPhotos
        {
            get { return _visitPhotos; }
            set
            {
                _visitPhotos = value;
                NotifyPropertyChanged("VisitPhotos");
            }
        }

        // Write changes in the data context to the database.
        public void SaveChangesToDB()
        {
            FishingSpotsDB.SubmitChanges();
        }

        // Query database and load the collections and lists
        public void LoadCollectionsFromDatabase()
        {
            try
            {
                LoadSpots();
                LoadSpecies();
                LoadBaits();
                LoadRigs();
                LoadBaitColors();
                LoadAllVisits();
            }
            catch (Exception e)
            {
                // silent fail
            }
        }

        public void LoadSpecies()
        {
            /*************** LOAD SPECIES  ****************/
            // Specify the query for all species in the database.
            var speciesInDB = from species in FishingSpotsDB.SpeciesItems
                              orderby species.SpeciesName
                              select species;
            // Query the database and load all species
            AllSpecies = new ObservableCollection<SpeciesTable>(speciesInDB);
        }

        public void LoadBaits()
        {
            /*************** LOAD BAIT  ****************/
            // Specify the query for all baits in the database.
            var baitsInDB = from bait in FishingSpotsDB.BaitItems orderby bait.BaitName
                              select bait;
            // Query the database and load all baits
            AllBaits = new ObservableCollection<BaitTable>(baitsInDB);
        }

        public void LoadRigs()
        {
            /*************** LOAD BAIT  ****************/
            // Specify the query for all rigs in the database.
            var rigsInDB = from rig in FishingSpotsDB.RigItems
                            orderby rig.RigName
                            select rig;
            // Query the database and load all rigs
            AllRigs = new ObservableCollection<RigTable>(rigsInDB);
        }

        public void LoadBaitColors()
        {
            /*************** LOAD BAITCOLORS  ****************/
            // Specify the query for all bait colors in the database.
            var baitColorsInDB = from baitColors in FishingSpotsDB.BaitColorItems
                            orderby baitColors.BaitColorName
                            select baitColors;
            // Query the database and load all baits
            AllBaitColors = new ObservableCollection<BaitColorTable>(baitColorsInDB);
        }

        public void LoadSpots()
        {
            /*************** LOAD SPOTS  ****************/
            // Specify the query for all spots in the database.
            var spotsInDB = from spots in FishingSpotsDB.Spots orderby spots.SpotName
                            select spots;
            // Query the database and load all species
            AllSpots = new ObservableCollection<SpotTable>(spotsInDB);
        }

        public void LoadVisits()
        {
            // Load the visits for the current spot
            var visitsForSpot = from visits in App.ViewModel.FishingSpotsDB.Visits
                                where visits.SpotID == CurrentSpot.SpotItemId
                                orderby visits.VisitDateTime
                                select visits;

            SpotVisits = new ObservableCollection<VisitTable>(visitsForSpot);
        }

        public void LoadVisitPhotos()
        {
            if (_currentVisit != null)
            {
                // Load the photos for the current visit
                var photosForVisit = from photos in App.ViewModel.FishingSpotsDB.VisitPhotos
                                     where photos.VisitID == CurrentVisit.VisitItemId
                                     select photos;

                VisitPhotos = new ObservableCollection<VisitPhotosTable>(photosForVisit);
            }
            else
            {
                VisitPhotos = new ObservableCollection<VisitPhotosTable>();
            }
        }

        public void LoadAllVisits()
        {
            // Load all visits from the database
            var allVisits = from spot in App.ViewModel.FishingSpotsDB.Spots
                           join visit in App.ViewModel.FishingSpotsDB.Visits
                            on spot.SpotItemId equals visit.SpotID 
                            orderby visit.VisitDateTime descending
                            select new DisplayVisit()
                            {SpotName = spot.SpotName, 
                             DT = visit.VisitDateTime,
                             Date = visit.VisitDateTime.Date.ToShortDateString(),
                             Rating = visit.Rating, 
                             VisitID = visit.VisitItemId,
                             Species = visit.Species};
                                

            AllVisits = new ObservableCollection<DisplayVisit>(allVisits);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the app that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}