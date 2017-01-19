using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;


namespace FishingSpots
{
    public partial class PhotoViewer : PhoneApplicationPage
    {
        public PhotoViewer()
        {
            InitializeComponent();

            DataContext = App.ViewModel.CurrentSpot;

   
        }


        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {           
        }

        private void doneAppBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void imgFull_ImageOpened(object sender, RoutedEventArgs e)
        {
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string path;
            string spot;

            // Determine what to show based on the query parameter
            NavigationContext.QueryString.TryGetValue("path", out path);
            NavigationContext.QueryString.TryGetValue("spot", out spot);

            if (!String.IsNullOrEmpty(path))
            {
                imgFull.Source = new BitmapImage(new Uri(path));
            }

            if (!String.IsNullOrEmpty(spot))
            {
                spotname.Text = spot;
            }
        }        
    }
}