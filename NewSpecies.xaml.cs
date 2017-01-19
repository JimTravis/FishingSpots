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

namespace FishingSpots
{
    public partial class NewSpecies : PhoneApplicationPage
    {
        public NewSpecies()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;
        }

        private void appBarOkButton_Click(object sender, EventArgs e)
        {
            // Confirm text in box
            if (newSpeciesNameTextBox.Text.Length > 0)
            {
                // Create the new species
                SpeciesTable newSpeciesTable = new SpeciesTable
                { 
                    SpeciesName = newSpeciesNameTextBox.Text
                };

                // Add the species to the view model
                App.ViewModel.AddSpecies(newSpeciesTable);

                // Return
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }

        }

        private void appBarCancelButton_Click(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}