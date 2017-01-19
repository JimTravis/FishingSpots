using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace FishingSpots
{
    public partial class AllVisits : PhoneApplicationPage
    {
        public AllVisits()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void visitsItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


    }
}