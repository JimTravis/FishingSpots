using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FishingSpots.Model;
using System.Collections;



namespace FishingSpots
{
    public partial class Spots : PhoneApplicationPage
    {
        ApplicationBarIconButton speciesButton;
        ApplicationBarIconButton allButton;
        Mode viewMode;
        List<String> species;

        

        enum Mode
        {
            all,
            species,
            SpeciesSpots
        }

        public Spots()
        {
            InitializeComponent();
            this.DataContext = App.ViewModel;

            

            ApplicationBar = new ApplicationBar();

            speciesButton = new ApplicationBarIconButton();
            speciesButton.IconUri = new Uri("/Images/appbar.filter.png", UriKind.Relative);
            speciesButton.Text = "by species";
            speciesButton.Click += new EventHandler(speciesButton_Click);

            allButton = new ApplicationBarIconButton();
            allButton.IconUri = new Uri("/Images/appbar.list.png", UriKind.Relative);
            allButton.Text = "show all";
            allButton.Click += new EventHandler(allButton_Click);
        }

        private void newSpotAppBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Spot.xaml?mode=add", UriKind.Relative));
        }

        private void spotsItemListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {         

            if (spotsItemListBox.SelectedItem != null)
            {
                App.ViewModel.CurrentSpot = (SpotTable)e.AddedItems[0];

                NavigationService.Navigate(new Uri("/Spot.xaml?mode=view", UriKind.Relative));

                // Clear the selection when done
                spotsItemListBox.SelectedItem = null;
            }
        }

        private void speciesButton_Click(object sender, EventArgs e)
        {           

            SetMode(Mode.species);
        }

        private void allButton_Click(object sender, EventArgs e)
        {
            SetMode(Mode.all);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SetMode(Mode.all);
        }

        // Species spots
        private ObservableCollection<SpotTable> _speciesSpots;
        public ObservableCollection<SpotTable> SpeciesSpots
        {
            get { return _speciesSpots; }
            set
            {
                _speciesSpots = value;

            }
        }

        private void SetMode(Mode theMode)
        {
            viewMode = theMode;

            IList Buttons = ApplicationBar.Buttons;
            switch (theMode)
            {
                case Mode.all:
                    if(Buttons.Contains(allButton))
                        Buttons.Remove(allButton);
                    if(!Buttons.Contains(speciesButton))
                        Buttons.Add(speciesButton);

                    spotsItemListBox.ItemsSource = App.ViewModel.AllSpots;

                    spotsItemListBox.Visibility = System.Windows.Visibility.Visible;
                    speciesItemListBox.Visibility = System.Windows.Visibility.Collapsed;
                  
                    break;
                case Mode.species:
                    if(Buttons.Contains(speciesButton))
                        Buttons.Remove(speciesButton);
                    if(!Buttons.Contains(allButton))
                        Buttons.Add(allButton);

                    // Get the species from the actual checkins, not the viewmodel list
                    // Reason is that viemodel list editable and species could be deleted
                    // while still being in checkins in the database
                    // Also, this will give a scoped list.
                    var query = from visit in App.ViewModel.FishingSpotsDB.Visits
                                select visit.Species;
                    species = new List<String>(query.Distinct());
                    speciesItemListBox.ItemsSource = species;

                    spotsItemListBox.Visibility = System.Windows.Visibility.Collapsed;
                    speciesItemListBox.Visibility = System.Windows.Visibility.Visible;
                     
                    break;
                case Mode.SpeciesSpots:

                    if (SpeciesSpots.Count() == 0)
                    {
                        MessageBox.Show("No spots found for that species.");
                        return;
                    }

                    if(Buttons.Contains(speciesButton))
                        Buttons.Remove(speciesButton);
                    if(!Buttons.Contains(allButton))
                        Buttons.Add(allButton);

                    spotsItemListBox.ItemsSource = SpeciesSpots;

                    spotsItemListBox.Visibility = System.Windows.Visibility.Visible;
                    speciesItemListBox.Visibility = System.Windows.Visibility.Collapsed;

                    break;

            }
        }

        private void speciesItemListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (speciesItemListBox.SelectedItem == null)
                return;

            String species = speciesItemListBox.SelectedItem.ToString();
            _speciesSpots = new ObservableCollection<SpotTable>();
          

            var query = from visit in App.ViewModel.FishingSpotsDB.Visits
                        where visit.Species == species
                        select visit.SpotID;

            List<int> spotIDs = new List<int>(query.Distinct());

            foreach (int id in spotIDs)
            {
                var query2 = from spot in App.ViewModel.FishingSpotsDB.Spots
                             where spot.SpotItemId == id
                             select spot;

                SpeciesSpots.Add((SpotTable)(query2.FirstOrDefault()));
            }

            speciesItemListBox.SelectedItem = null;

            SetMode(Mode.SpeciesSpots);
        }
       
        
    }
}