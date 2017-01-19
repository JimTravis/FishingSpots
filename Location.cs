using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
// internal data modeling
using FishingSpots.Model;
using FishingSpots.ViewModel;
using FishingSpots;
// for location apis
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using System.Device.Location;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Maps.Controls;
using Windows.Foundation;

// This used to be a static class. However, backing up the database means we have to unload and reload the DB. 
// This has implications for compiled queries, i.e. once the query is compiled it references the database instance
// and if you invalidate that instance, the compiled query no longer works. Would that compiled queries would recompile
// rather than just crash in this instance, but no such luck.

namespace FishingSpots
{
    public /*static*/ class FishLocation
    {
        // constants
        const uint locAccuracy = 10; // location accuracy in meters  
 
        /*static*/ Geolocator geolocator;
        /*static*/ SpotsMap SM;
        PositionStatus currentStatus;
        bool bTracking = false;

        public FishLocation()
        {
            geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            geolocator.MovementThreshold = locAccuracy;
            geolocator.StatusChanged += geolocator_StatusChanged;
            geolocator.PositionChanged += geolocator_PositionChanged;

        }

        // We use these when the app is deactivate or activated
        public void StopEvents()
        {
            geolocator.StatusChanged -= geolocator_StatusChanged;
            geolocator.PositionChanged -= geolocator_PositionChanged;
        }

        public void StartEvents()
        {
            geolocator.StatusChanged += geolocator_StatusChanged;
            geolocator.PositionChanged += geolocator_PositionChanged;
        }
              
       
        // Required to prompt for consent to use location.
        internal /*static*/ bool PromptForConsent()
        {
            bool results = false; // return variable
            AppSettings appSettings = new AppSettings();

            if (appSettings.LocationConsent == true)
            {
                // User has opted in to Location. Return immediately.
                return true;
            }
            else
            {
                MessageBoxResult result =
                    MessageBox.Show("This app accesses your phone's location.\n\nIs that ok?",
                    "Location",
                    MessageBoxButton.OKCancel);

               
                results = (result == MessageBoxResult.OK ? true : false);                   
               
            }
            
            appSettings.LocationConsent = results;

            return results;           

        }


        // Get the current location sort of synchronously.
        internal /*static*/ async Task<Windows.Devices.Geolocation.Geoposition> getOneShotLocation()
        {    

            IAsyncOperation<Geoposition> locationTask = null;

            try
            {
                locationTask = geolocator.GetGeopositionAsync(
                    TimeSpan.FromMinutes(0.100),
                    TimeSpan.FromSeconds(10)
                     );
                return await locationTask;
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off

                    return null;
                }

            }
            finally
            {
                if (locationTask != null)
                {
                    if (locationTask.Status == AsyncStatus.Started)
                        locationTask.Cancel();

                    locationTask.Close();

                }
            }
               

            return null;
        }

    
        internal /*static*/ void TrackLocation(bool tracking, SpotsMap map)
        {
            if (tracking)
            {
                
                SM = map;
               

                //geolocator.StatusChanged += geolocator_StatusChanged;
                //geolocator.PositionChanged += geolocator_PositionChanged;
                bTracking = true;

                
            }
            else
            {
                //geolocator.PositionChanged -= geolocator_PositionChanged;
                //geolocator.StatusChanged -= geolocator_StatusChanged;
                bTracking = false;
            }
        }

        /*static*/ void geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            currentStatus = args.Status;

            if(currentStatus == PositionStatus.Disabled)
            {
                MessageBox.Show("You have disabled location services. Location functionality will not work until you enable location services in phone Settings.");
            }
        }

        // display an appropriate error message
        String getStatusString(PositionStatus locStatus)
        {

            switch (locStatus)
            {
                case Windows.Devices.Geolocation.PositionStatus.Ready:
                    // Location data is available
                    return "Location is available.";
                    

                case Windows.Devices.Geolocation.PositionStatus.Initializing:
                    // This status indicates that a GPS is still acquiring a fix
                    return "A GPS device is still initializing.";
                    

                case Windows.Devices.Geolocation.PositionStatus.NoData:
                    // No location data is currently available
                    return "Data from location services is currently unavailable.";
                   

                case Windows.Devices.Geolocation.PositionStatus.Disabled:
                    // The app doesn't have permission to access location,
                    // either because location has been turned off.
                    return "Your location is currently turned off. " +
                         "Change your settings through the Settings charm " +
                         " to turn it back on.";
                    

                case Windows.Devices.Geolocation.PositionStatus.NotInitialized:
                    // This status indicates that the app has not yet requested
                    // location data by calling GetGeolocationAsync() or
                    // registering an event handler for the positionChanged event.
                    return "Location status is not initialized because " +
                        "the app has not requested location data.";
                    

                case Windows.Devices.Geolocation.PositionStatus.NotAvailable:
                    // Location is not available on this version of Windows
                    return "You do not have the required location services " +
                        "present on your system.";
                    

                default:
                    return "Unknown status";

            }
        }


        /*static*/ void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            if (bTracking == true && SM != null)
            {
                GeoCoordinate g = new GeoCoordinate(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);
                SM.Dispatcher.BeginInvoke(() =>
                {
                    SM.MarkCurrentLocation(g);
                    SM.SpotMap.Center = g;
                });
            }
        }


        public /*static*/ LocationRectangle GetVisibleMapArea(Map mMap)
        {
            GeoCoordinate mCenter = mMap.Center;
            Debug.WriteLine(mCenter);
            System.Windows.Point pCenter = mMap.ConvertGeoCoordinateToViewportPoint(mCenter);
            
            // Calculate top-left and bottom-right as points
            System.Windows.Point pNW = new System.Windows.Point(pCenter.X - mMap.ActualWidth / 2, pCenter.Y - mMap.ActualHeight / 2);
            System.Windows.Point pSE = new System.Windows.Point(pCenter.X + mMap.ActualWidth / 2, pCenter.Y + mMap.ActualHeight / 2);

            Debug.WriteLine("pCenter: " + pCenter);
            Debug.WriteLine("Width: " + mMap.ActualWidth);
            Debug.WriteLine("Height: " + mMap.ActualHeight);
            Debug.WriteLine("pNW: " + pNW);
            Debug.WriteLine("pSE: " + pSE);

            GeoCoordinate gcNW = mMap.ConvertViewportPointToGeoCoordinate(pNW);
            GeoCoordinate gcSE = mMap.ConvertViewportPointToGeoCoordinate(pSE);
            Debug.WriteLine("NW: " + gcNW);
            Debug.WriteLine("SE: " + gcSE);

            return new LocationRectangle(gcNW, gcSE);
        }

        public /*static*/ Func<FishingSpotsDataContext, LocationRectangle, IQueryable<SpotTable>>
            SpotsInRect = CompiledQuery.Compile((FishingSpotsDataContext DB, LocationRectangle mapRect) =>
                from spot in DB.Spots
                where spot.Longitude > mapRect.West
                && spot.Longitude < mapRect.East
                && spot.Latitude > mapRect.South
                && spot.Latitude < mapRect.North
                select spot);

        //public /*static*/ List<SpotTable> SpotsInRect(FishingSpotsDataContext DB, LocationRectangle mapRect)
        //{
        //    return (from spot in DB.Spots
        //            where spot.Longitude > mapRect.West &&
        //                spot.Longitude < mapRect.East &&
        //                spot.Latitude > mapRect.South &&
        //                spot.Latitude < mapRect.North
        //            select spot).ToList();
        //}


  

        // Return all saved spots within 50km of the specified location.
        public /*static*/ ObservableCollection<Pushpin> GetNearbyPushpins(FishingSpotsDataContext DB, Map mMap)
        {  
            // Get the corner boundaries
            //GeoCoordinate point1 = CalculateGeoCoordinate(currentLocation, 45, 71);
            //GeoCoordinate point2 = CalculateGeoCoordinate(currentLocation, 225, 71);

            LocationRectangle mapRect = GetVisibleMapArea(mMap);            
         
            //double latmin = mapRect.South;
            //double latmax = mapRect.North;
            //double lonmin = mapRect.West;
            //double lonmax = mapRect.East;

            List<SpotTable> spotsInDB;
            spotsInDB = SpotsInRect(DB, mapRect).ToList();

            //spotsInDB.Sort(delegate(SpotTable s1, SpotTable s2)
            //{
            //    return (new GeoCoordinate(s1.Latitude, s1.Longitude)).GetDistanceTo(currentLocation).CompareTo((new GeoCoordinate(s1.Latitude, s1.Longitude)).GetDistanceTo(currentLocation));
            //});

            List<Pushpin> pins =  new List<Pushpin>();
            foreach (SpotTable spot in spotsInDB)
            {
                Pushpin pin = new Pushpin();
                GeoCoordinate geoC = new GeoCoordinate(spot.Latitude, spot.Longitude);
                pin.GeoCoordinate = geoC;
                pin.Content = spot.SpotName;
                if (spot.Rating > 0)
                {
                    // Add rating to pin text
                    pin.Content += "\n";
                    for (int i = 0; i < spot.Rating; i++)
                    {
                        pin.Content += "*";
                    }
                }
                pin.Name = spot.SpotItemId.ToString();
                // ?? Make these colors configurable in settings?
                pin.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                pins.Add(pin);
            }

            return new ObservableCollection<Pushpin>(pins);
        }

        // No longer using
        //public /*static*/ GeoCoordinate CalculateGeoCoordinate(GeoCoordinate point, double bearing, double distance)
        //{
        //    int R = 6371;  // radius of the earth in km ;-)

        //    bearing = (Math.PI / 180) * bearing;
        //    double lat1 = point.Latitude * (Math.PI / 180);
        //    double lon1 = point.Longitude * (Math.PI / 180);

        //    double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(distance / R) + Math.Cos(lat1) * Math.Sin(distance / R) * Math.Cos(bearing));
        //    double lon2 = lon1 + Math.Atan2(Math.Sin(bearing) * Math.Sin(distance / R) * Math.Cos(lat1), Math.Cos(distance / R) - Math.Sin(lat1) * Math.Sin(lat2));
        //    lon2 = (lon2 + 3 * Math.PI) % (2 * Math.PI) - Math.PI;

        //    lat2 = 180 * lat2 / Math.PI;
        //    lon2 = 180 * lon2 / Math.PI;

        //    return new GeoCoordinate(lat2, lon2);
        //}  

    }



}