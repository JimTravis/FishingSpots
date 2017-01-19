using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SysControls = System.Windows.Controls;


namespace FishingSpots
{
    public class Converter : IDateToBrushConverter
    {
        public IEnumerable<DateTime> Dates { get; set; }

        public Brush Convert(DateTime dateTime, bool isSelected, Brush defaultValue, BrushType brushType)
        {
            if (brushType == BrushType.Background)
            {
                if (Dates != null && Dates.Any(d => d == dateTime))
                {
                    return new SolidColorBrush(Colors.LightGray);
                }
                return defaultValue;

            }
            return defaultValue;
        }
    }

    public partial class Visits : PhoneApplicationPage
    {
        ApplicationBarIconButton byDateAppBarButton;
        ApplicationBarIconButton allAppBarButton;

        // UI modes for this page
        private enum Mode
        {
            all,
            byDate,
            Calendar
        }
        
        public Visits()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            Cal.ColorConverter = new Converter();

            //App.ViewModel.LoadAllVisits();

            // Create the application bar
            ApplicationBar = new ApplicationBar();

            byDateAppBarButton = new ApplicationBarIconButton();
            byDateAppBarButton.IconUri = new Uri("/Images/appbar.calendar.png", UriKind.Relative);
            byDateAppBarButton.Text = "choose date";
            byDateAppBarButton.Click += new EventHandler(byDateAppBarButton_Click);

            allAppBarButton = new ApplicationBarIconButton();
            allAppBarButton.IconUri = new Uri("/Images/appbar.list.png", UriKind.Relative);
            allAppBarButton.Text = "list all";
            allAppBarButton.Click += new EventHandler(allAppBarButton_Click);

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

        private void visitsItems_SelectionChanged(object sender, SysControls.SelectionChangedEventArgs e)
        {
            if (visitsItems.SelectedItem != null)
            {

                DisplayVisit dv = (DisplayVisit)e.AddedItems[0];
                int id = dv.VisitID;

                var checkin = from visit in App.ViewModel.FishingSpotsDB.Visits
                              where visit.VisitItemId == id
                              select visit;

                App.ViewModel.CurrentVisit = (VisitTable)checkin.FirstOrDefault();

                NavigationService.Navigate(new Uri("/VisitDetails.xaml", UriKind.Relative));

                // Clear the selection when done
                visitsItems.SelectedItem = null;
            }
        }

        private void byDateAppBarButton_Click(object sender, EventArgs e)
        {
            SetMode(Mode.Calendar);            
        }

        private void allAppBarButton_Click(object sender, EventArgs e)
        {
            SetMode(Mode.all);
        }

   
        // UI mode handling for this page
        private void SetMode(Mode theMode)
        {
            prog.Visibility = System.Windows.Visibility.Collapsed;

            switch (theMode)
            {
                case Mode.all:

                    visitsItems.ItemsSource = App.ViewModel.AllVisits;
                    visitsItems.Visibility = System.Windows.Visibility.Visible;
                    Cal.Visibility = System.Windows.Visibility.Collapsed;

                    if(!ApplicationBar.Buttons.Contains(byDateAppBarButton))
                        ApplicationBar.Buttons.Add(byDateAppBarButton);
                    if (ApplicationBar.Buttons.Contains(allAppBarButton))
                        ApplicationBar.Buttons.Remove(allAppBarButton);

                    break;
                case Mode.byDate:
                    visitsItems.ItemsSource = DateVisits;
                    visitsItems.Visibility = System.Windows.Visibility.Visible;
                    Cal.Visibility = System.Windows.Visibility.Collapsed;

                    if(!ApplicationBar.Buttons.Contains(byDateAppBarButton))
                        ApplicationBar.Buttons.Add(byDateAppBarButton);
                    if (!ApplicationBar.Buttons.Contains(allAppBarButton))
                        ApplicationBar.Buttons.Add(allAppBarButton);

                    
                    break;
                case Mode.Calendar:
                    visitsItems.Visibility = System.Windows.Visibility.Collapsed;
                    Cal.Visibility = System.Windows.Visibility.Visible;

                     if(ApplicationBar.Buttons.Contains(byDateAppBarButton))
                        ApplicationBar.Buttons.Remove(byDateAppBarButton);
                    if (!ApplicationBar.Buttons.Contains(allAppBarButton))
                        ApplicationBar.Buttons.Add(allAppBarButton);
                    break;
            }
        }

        // All visits
        private ObservableCollection<DisplayVisit> _dateVisits;
        public ObservableCollection<DisplayVisit> DateVisits
        {
            get { return _dateVisits; }
            set
            {
                _dateVisits = value;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SetMode(Mode.all);
        }

        private void Cal_DateClicked(object sender, WPControls.SelectionChangedEventArgs e)
        {            
            DateTime dt = e.SelectedDate;

            // Get the checkins from the AllVisits list that match the selected date
            var theVisits = from visit in App.ViewModel.AllVisits
                            where visit.DT.Date == dt.Date
                            select visit;
            _dateVisits = new ObservableCollection<DisplayVisit>(theVisits);

            if (DateVisits.Count == 0)
            {
                // No visits to show. Do nothing
                return;
            }

            // Update the UI
            SetMode(Mode.byDate);
        }
     
    }
}