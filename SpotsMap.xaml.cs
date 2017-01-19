using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;
using Windows.Devices.Geolocation;
using System.Device.Location;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FishingSpots.Model;
using FishingSpots.ViewModel;




namespace FishingSpots
{
    public partial class SpotsMap : PhoneApplicationPage
    {
   
        bool locPermitted = false;
        ObservableCollection<Pushpin> nearbySpots;
        ObservableCollection<DependencyObject> children;
        UserLocationMarker marker;
        Mode mapMode = Mode.Normal;

            enum Mode
            {
                Normal,
                Mapspot
            };


        public SpotsMap()
        {
            InitializeComponent();

            children = MapExtensions.GetChildren(SpotMap);
            marker = new UserLocationMarker();
            marker.GeoCoordinate = new GeoCoordinate();
            marker.Visibility = System.Windows.Visibility.Collapsed;
            children.Add(marker);
            SpotMap.LandmarksEnabled = true;
            SpotMap.CartographicMode = MapCartographicMode.Aerial;

         }

        //// Get the latitude and longitude
        private async void doLocation()
        {
            var pos = await App.FLocation.getOneShotLocation();

            if (pos != null)
            {
                GeoCoordinate g = new GeoCoordinate(((Geoposition)pos).Coordinate.Latitude, ((Geoposition)pos).Coordinate.Longitude);
                marker.GeoCoordinate = g;
      
                App.ViewModel.CurrentLocation.Latitude = marker.GeoCoordinate.Latitude;
                App.ViewModel.CurrentLocation.Longitude = marker.GeoCoordinate.Longitude;

                marker.Visibility = System.Windows.Visibility.Visible;

                MapFirst();
            }

            // Start the location events (or not)
            if (App.ViewModel.Settings.TrackingSetting == false)
            {
                textTracking.Foreground = new SolidColorBrush(Colors.Red);
                textTracking.Text = "not tracking";
                App.FLocation.TrackLocation(false, this);

            }
            else
            {
                textTracking.Foreground = new SolidColorBrush(Colors.Green);
                textTracking.Text = "tracking";
                App.FLocation.TrackLocation(true, this);
            }

            pg.Visibility = System.Windows.Visibility.Collapsed;
        }
   
        private void newSpotAppBarButton_Click(object sender, EventArgs e)
        {
            // Always stop tracking if the map isn't showing.
            App.FLocation.TrackLocation(false, this);
            NavigationService.Navigate(new Uri("/Spot.xaml?mode=add", UriKind.Relative));
        }

        // Get the latitude and longitude
        public /*async*/ void MarkCurrentLocation(GeoCoordinate g)
        {  
            // Mark the user's location            
            marker.GeoCoordinate = g;
            marker.Visibility = System.Windows.Visibility.Visible;
        }

        public void pin_Tap(object sender, RoutedEventArgs e)
        {
            Pushpin pin = (Pushpin)sender;
            String spotID = pin.Name;

            App.ViewModel.CurrentSpot = App.ViewModel.getSpotByID(Int64.Parse(spotID));
            NavigationService.Navigate(new Uri("/Spot.xaml?mode=view", UriKind.Relative));

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            pg.Visibility = System.Windows.Visibility.Visible;

            string mode;

            // Determine what to show based on the query parameter
            NavigationContext.QueryString.TryGetValue("mode", out mode);

            switch (mode)
            {
                case "mapspot":
                    mapMode = Mode.Mapspot;
                    break;
                default:
                    
                    break;
            }

            doLocation();
        }


        private void MapFirst()
        {
            if (marker.GeoCoordinate != null)
            {
                SpotMap.Center = marker.GeoCoordinate;
                ZoomMap();
                DoPushpins();
                App.ViewModel.FirstMapLoadCompleted = true;
                marker.Visibility = System.Windows.Visibility.Visible;
            }            
        }

        private void ZoomMap()
        {
            SpotMap.ZoomLevel = 13;
            
        }

        private void MapSpot()
        {
            SpotMap.Center = new GeoCoordinate(App.ViewModel.CurrentSpot.Latitude, App.ViewModel.CurrentSpot.Longitude);
            ZoomMap();
            DoPushpins();
            mapMode = Mode.Normal;            
        }

        private void DoPushpins()
        {
            // Remove the existing pushpins
            // Surely there's a more efficient way?
            // Perhaps fill up children with some pins and use visibility
            // while changing pushpin properties.

            // This code assumes marker is always at [0]. 
            for (int i = 1; i < children.Count; i++)
            {                
                children.RemoveAt(i);                
            }
            
            // Don't mark spots on huge areas of the globe.
            // This is roughly the size of Puget Sound
            //if (SpotMap.ZoomLevel >= 9)
            //{
                try
                {

                    // Mark up the nearby spots
                    nearbySpots = App.FLocation.GetNearbyPushpins(App.ViewModel.FishingSpotsDB, SpotMap);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Mapping error.\n" + e.Message);
                    return;
                }

                // Plot the pins
                foreach (Pushpin pin in nearbySpots)
                {                    
                    pin.Tap += pin_Tap;
                    children.Add(pin);                    
                }
            //}

        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

            locPermitted = App.FLocation.PromptForConsent();
            if (locPermitted == false)
            {
                //App.Current.Terminate();
                NavigationService.Navigate(new Uri("/MainHub.xaml", UriKind.Relative));
            }

        
        }

   
        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Always stop tracking if the map isn't showing.
            App.FLocation.TrackLocation(false, this);
            
        }

   

        private void SpotMap_ResolveCompleted(object sender, MapResolveCompletedEventArgs e)
        {
            // Get the user's current location 
            switch (mapMode)
            {
                case Mode.Normal:
                    DoPushpins();
                    break;
                case Mode.Mapspot:
                    MapSpot();
                    break;
                default:
                        
                    break;
            }
           
            ApplicationBar.IsVisible = true;
        }

        private void centerAppBarButton_Click(object sender, EventArgs e)
        {
            MapFirst();
            
        }

        

        private void SpotMap_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Point p =   e.GetPosition((UIElement)SpotMap);
            GeoCoordinate g = SpotMap.ConvertViewportPointToGeoCoordinate(p);
            App.ViewModel.CurrentSpot = new SpotTable();
            App.ViewModel.CurrentSpot.Latitude = g.Latitude;
            App.ViewModel.CurrentSpot.Longitude = g.Longitude;

            // Always stop tracking if the map isn't showing.
            App.FLocation.TrackLocation(false, this);

            NavigationService.Navigate(new Uri("/Spot.xaml?mode=mapadd", UriKind.Relative));

       

        }
            
      

        private void trackLocationAppBarButton_Click(object sender, EventArgs e)
        {
            App.ViewModel.Settings.TrackingSetting = !App.ViewModel.Settings.TrackingSetting;

            if (App.ViewModel.Settings.TrackingSetting == false)
            {
                textTracking.Foreground = new SolidColorBrush(Colors.Red);
                textTracking.Text = "not tracking";
                App.FLocation.TrackLocation(false, this);

            }
            else
            {
                textTracking.Foreground = new SolidColorBrush(Colors.Green);
                textTracking.Text = "tracking";
                App.FLocation.TrackLocation(true, this);
            }

        }

        private void SpotMap_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "7a6a933e-028d-4a65-9197-29d39bf89f08";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "DoWvxync_HCtzr1gMZF3Uw";

        }
               
    }
}