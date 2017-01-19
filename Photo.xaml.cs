using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace FishingSpots
{
    public partial class Photo : PhoneApplicationPage
    {
        CameraCaptureTask cct;

        public Photo()
        {
            InitializeComponent();

            cct = new CameraCaptureTask();
            cct.Completed += new EventHandler<PhotoResult>(cct_Completed);
            
        }

        void cct_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                // do stuff here, like preview the photo.
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            cct.Show();
        }
    }
}