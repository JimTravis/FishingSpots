using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FishingSpots.Model;
using FishingSpots.ViewModel;
using WPControls;
using System.Windows.Media;

namespace FishingSpots
{
    public partial class Species : PhoneApplicationPage
    {
        public Species()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;

            Cal.ColorConverter = new Converter();

        }

        private void newSpeciesAppBarButton_Click(object sender, EventArgs e)
        {
   
            NavigationService.Navigate(new Uri("/NewSpecies.xaml", UriKind.Relative));
        }

        private void Cal_MonthChanged(object sender, MonthChangedEventArgs e)
        {
           

            
        }

        private void Cal_MonthChanging(object sender, MonthChangedEventArgs e)
        {
            var dates = new DateTime[App.ViewModel.AllVisits.Count];
            int i = 0;
            foreach (DisplayVisit v in App.ViewModel.AllVisits)
            {
                dates[i++] = new DateTime(v.DT.Year, v.DT.Month, v.DT.Day);
            }

            ((Converter)Cal.ColorConverter).Dates = dates;
        }

        private void Cal_DateClicked(object sender, WPControls.SelectionChangedEventArgs e)
        {
            MessageBox.Show("Click" + e.SelectedDate);
        }
    }

   


   

}