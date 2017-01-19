using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FishingSpots.Model;
using FishingSpots.ViewModel;
using Windows.Storage;



namespace FishingSpots
{
    public partial class MainHub : PhoneApplicationPage
    {
        
        private static bool testDB = false;

        public MainHub()
        {
            InitializeComponent();
        }

       

        private void MapTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SpotsMap.xaml", UriKind.Relative));
        }

        private void SpotsTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Spots.xaml", UriKind.Relative));
        }

        private void CheckinsTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Visits.xaml", UriKind.Relative));
        }

        private void SettingsTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private async void HelpTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var pdffile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"assets\docs\Help.htm");
                await Windows.System.Launcher.LaunchFileAsync(pdffile);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error opening Help file.\n" + exc.Message);
            }
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
           
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MapTile.IsFrozen = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MapTile.IsFrozen = true;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.DBLoaded == false)
            {
                pgBar.Visibility = System.Windows.Visibility.Visible;

                ContentPanel.Visibility = System.Windows.Visibility.Collapsed;

                // Run once when the app starts

                // Create the database if it does not exist.
                using (FishingSpotsDataContext db = new FishingSpotsDataContext(App.DBConnectionString))
                {
                    if (testDB == true)
                    {
                        if (db.DatabaseExists() == true)
                            db.DeleteDatabase();

                        // Perf testing
                        db.CreateDatabase();
                        db.CreateDefaultData();

                    }
                    else
                    {
                        // Normal execution
                        if (db.DatabaseExists() == false)
                        {
                            // Create the local database.
                            db.CreateDatabase();

                            // Load the default system data
                            db.CreateDefaultData();
                        }
                    }
                }

                // Create the ViewModel object.
                App.ViewModel = new FishingSpotsViewModel(App.DBConnectionString);

                // This used to be static, but now we need an instance. See the Location.cs file for details.
                App.FLocation = new FishLocation();

                // We'll always track by default. User can disable per session.
                App.ViewModel.Settings.TrackingSetting = true;

                // Query the local database and load observable collections.
                App.ViewModel.LoadCollectionsFromDatabase();

                if (testDB == true)
                {
                    TestDB tdb = new TestDB(50, 10);
                    tdb.PopulateDB();
                }

                App.DBLoaded = true;
                pgBar.Visibility = System.Windows.Visibility.Collapsed;
                ContentPanel.Visibility = System.Windows.Visibility.Visible;
            }

        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

      
        private void allSpotsTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            allSpotsTile.Background = new SolidColorBrush(Colors.Gray);
        }

        private void allSpotsTile_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            allSpotsTile.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x41, 0x69, 0xE1));
        }

        private void allSpotsTile_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            allSpotsTile.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x41, 0x69, 0xE1));
        }

        private void allCheckinsTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            allCheckinsTile.Background = new SolidColorBrush(Colors.Gray);
        }

        private void allCheckinsTile_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            allCheckinsTile.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x8B, 0x8B));
        }

        private void allCheckinsTile_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            allCheckinsTile.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x8B, 0x8B));
        }

        private void settingsTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            settingsTile.Background = new SolidColorBrush(Colors.Gray);
        }

        private void settingsTile_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            settingsTile.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2E, 0x8B, 0x57));
        }

        private void settingsTile_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            settingsTile.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2E, 0x8B, 0x57));
        }

        private void helpTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            helpTile.Background = new SolidColorBrush(Colors.Gray);
        }

        private void helpTile_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            helpTile.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x46, 0x82, 0xB4));
        }

        private void helpTile_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            helpTile.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x46, 0x82, 0xB4));
        }

        private void MapTile_Loaded(object sender, RoutedEventArgs e)
        {
            MapTile.Message = StaticUtils.GetRandomSaying();
        }

       

      
    }
}