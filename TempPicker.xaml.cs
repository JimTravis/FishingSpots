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
    public partial class TempPicker : PhoneApplicationPage
    {
        List<int> temps;

        public TempPicker()
        {
            InitializeComponent();

            temps = new List<int>();
        }

        private void LongListSelector_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = -50; i < 130; i++)
            {
                temps.Add(i);
            }

            TempsList.ItemsSource = temps;
            TempsList.SelectedItem = temps[120];
            TempsList.ScrollTo(TempsList.SelectedItem);
        }

        private void TempsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TempsList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //App.ViewModel.Temperature = Int16.Parse((sender as LongListSelector).SelectedItem.ToString());
            NavigationService.GoBack();
        }
    }
}