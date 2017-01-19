using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FishingSpots.ViewModel;
using FishingSpots.Model;
using System.Diagnostics;

namespace FishingSpots
{
    public partial class VisitDetails : PhoneApplicationPage
    {
        SpotTable thisSpot;

        public VisitDetails()
        {
            InitializeComponent();

            DataContext = App.ViewModel;
        }


        private void okCheckinAppBarButton_Click(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }



        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            var query = from spot in App.ViewModel.AllSpots
                        where
                            App.ViewModel.CurrentVisit.SpotID == spot.SpotItemId
                        select spot;

            thisSpot = (query.First() as SpotTable);
            titleText.Text = thisSpot.SpotName;

            App.ViewModel.LoadVisitPhotos();
        }

        private void delCheckinAppBarButton_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Delete this checkin?\n\nAny photos will be deleted as well.\nPhotos in your camera roll are safe.", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
               
                ApplicationBar.IsVisible = false;

                App.ViewModel.DeleteCurrentVisit();
                App.ViewModel.DeleteVisitPhotos(App.ViewModel.VisitPhotos.ToList());

                NavigationService.GoBack();
            }
        }

        private void editCheckinAppBarButton_Click(object sender, EventArgs e)
        {
            App.ViewModel.CurrentSpot = thisSpot;
            NavigationService.Navigate(new Uri("/Visit.xaml?mode=edit", UriKind.Relative));
        }

        private void titleText_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.CurrentSpot = thisSpot;
            NavigationService.Navigate(new Uri("/Spot.xaml?mode=view", UriKind.Relative));
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {       

            
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        private void photoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (photoList.SelectedItem != null)
            {
                VisitPhotosTable vpt = photoList.SelectedItem as VisitPhotosTable;

                Debug.Assert(vpt != null);
                Debug.Assert(App.ViewModel.CurrentVisit != null);

                NavigationService.Navigate(new Uri("/PhotoViewer.xaml?spot=" + App.ViewModel.CurrentVisit.VisitDateTime + "&path=" + vpt.Photo, UriKind.Relative));

                // Clear the selection when done
                photoList.SelectedItem = null;
            }
        }
    }
}