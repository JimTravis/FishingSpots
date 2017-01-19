using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;
using FishingSpots.Model;


namespace FishingSpots
{
    // Dynamic data templates.
    // This is the base implementation.
    public abstract class DataTemplateSelector : ContentControl
    {
        public virtual DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            ContentTemplate = SelectTemplate(newContent, this);
        }
    }

    // This is the custom implementation
    public class MyTemplateSelector : DataTemplateSelector
    {
        // These properties correspond to the custom template types.
        public DataTemplate Species
        {
            get;
            set;
        }

        public DataTemplate Bait
        {
            get;
            set;
        }

        public DataTemplate Rig
        {
            get;
            set;
        }

        public DataTemplate BaitColor
        {
            get;
            set;
        }

        // This gets called when content is loading to enable template selection.
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is FishingSpots.Model.SpeciesTable)
                return Species;
            else if (item is FishingSpots.Model.BaitTable)
                return Bait;
            else if (item is FishingSpots.Model.RigTable)
                return Rig;
            else if (item is FishingSpots.Model.BaitColorTable)
                return BaitColor;

            return base.SelectTemplate(item, container);
        }
      
    }


    public partial class ListEdit : PhoneApplicationPage
    {
        string mode;

        public ListEdit()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Determine what to show based on the query parameter
            NavigationContext.QueryString.TryGetValue("mode", out mode);

            switch (mode)
            {
                case "species":
                    ItemsList.ItemsSource = App.ViewModel.AllSpecies;                   
                    break;
                case "bait":
                    ItemsList.ItemsSource = App.ViewModel.AllBaits;
                    break;
                case "rig":
                    ItemsList.ItemsSource = App.ViewModel.AllRigs;
                    break;
                case "baitcolor":
                    ItemsList.ItemsSource = App.ViewModel.AllBaitColors;
                    break;
                default:

                    break;
            }
        }
       

        private void newItemAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (newItemTextBox.Text != "add new item")
            {
                switch (mode)
                {
                    case "species":
                        App.ViewModel.AddSpecies(newItemTextBox.Text);
                        break;
                    case "bait":
                        App.ViewModel.AddBait(newItemTextBox.Text);
                        break;
                    case "rig":
                        App.ViewModel.AddRig(newItemTextBox.Text);
                        break;
                    case "baitcolor":
                        App.ViewModel.AddBaitColor(newItemTextBox.Text);
                        break;
                    default:
                        break;


                }
                newItemTextBox.Text = "add new item";
            }
        }

        private void newItemTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Clear the text box when it gets focus.
            newItemTextBox.Text = String.Empty;

        }

   

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (ItemsList.SelectedItem != null)
            {
                if (MessageBox.Show("Delete this item?", "confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    switch (mode)
                    {
                        case "species":
                            SpeciesTable st = ItemsList.SelectedItem as SpeciesTable;
                            App.ViewModel.DeleteSpecies(st.SpeciesItemId);
                            break;
                        case "bait":
                            BaitTable bt = ItemsList.SelectedItem as BaitTable;
                            App.ViewModel.DeleteBait(bt.BaitItemId);
                            break;
                        case "rig":
                            RigTable rt = ItemsList.SelectedItem as RigTable;
                            App.ViewModel.DeleteRig(rt.RigItemId);
                            break;
                        case "baitcolor":
                            BaitColorTable bct = ItemsList.SelectedItem as BaitColorTable;
                            App.ViewModel.DeleteBaitColor(bct.BaitColorItemId);
                            break;
                        default:
                            break;
                    }

                    ItemsList.SelectedIndex = -1;
                    ItemsList.Focus();
                }
            }
            else
            {
                MessageBox.Show("Select an item to delete");
            }
        }

        private void newItemTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}