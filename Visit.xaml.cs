using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FishingSpots.Resources;
using FishingSpots.Model;
using FishingSpots.ViewModel;
// camera
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Phone;
using System.Windows.Media.Imaging;

using System.Windows.Media.Animation;


namespace FishingSpots
{
    public partial class Visit : PhoneApplicationPage
    {        
        public ObservableCollection<string> Weather;
        // tide descriptors       
        public ObservableCollection<string> Tides;
        private string pagemode;

        // appbar button fields
        ApplicationBarIconButton appBarOkButton;
        ApplicationBarIconButton appBarCancelButton;
        ApplicationBarIconButton appBarPhotoButton;
        ApplicationBarIconButton appBarDeleteButton;

        CameraCaptureTask cct;
        PhotoChooserTask pct;
        string newPic = String.Empty; // Path to most recent pic from capture task 
        string newThumb = String.Empty; // Path to most recent thumb from capture task

        List<VisitPhotosTable> newPhotos;
        List<VisitPhotosTable> photosToRemove;

        // Need to delete added photos on cancellation
        bool bSaved = false;

        //Popup p; // photo task popup
        //TextBlock txtCam = new TextBlock();
        //TextBlock txtPick = new TextBlock();
            
        
        public Visit()
        {
            InitializeComponent();

            DataContext = App.ViewModel;
            

            BuildLists();

            CreateAppBar(); 
 
            // photos
            cct = new CameraCaptureTask();
            cct.Completed += new EventHandler<PhotoResult>(cct_Completed);

            pct = new PhotoChooserTask();
            pct.Completed += new EventHandler<PhotoResult>(pct_Completed);

            newPhotos = new List<VisitPhotosTable>();
            photosToRemove = new List<VisitPhotosTable>();

        }

        private void CreateAppBar()
        {
            // Create the application bar
            ApplicationBar = new ApplicationBar();

            appBarPhotoButton = new ApplicationBarIconButton();
            appBarPhotoButton.IconUri = new Uri("/Images/feature.camera.png", UriKind.Relative);
            appBarPhotoButton.Text = "check in";
            ApplicationBar.Buttons.Add(appBarPhotoButton);
            appBarPhotoButton.Click += new EventHandler(appBarPhotoButton_Click);

            appBarOkButton = new ApplicationBarIconButton();
            appBarOkButton.IconUri = new Uri("/Images/save.png", UriKind.Relative);
            appBarOkButton.Text = "save";
            appBarOkButton.Click += new EventHandler(saveCheckinAppBarButton_Click);
            ApplicationBar.Buttons.Add(appBarOkButton);

            appBarCancelButton = new ApplicationBarIconButton();
            appBarCancelButton.IconUri = new Uri("/Images/cancel.png", UriKind.Relative);
            appBarCancelButton.Text = "cancel";
            appBarCancelButton.Click += new EventHandler(cancelCheckinAppBarButton_Click);
            ApplicationBar.Buttons.Add(appBarCancelButton);
           

            appBarDeleteButton = new ApplicationBarIconButton();
            appBarDeleteButton.IconUri = new Uri("/Images/delete.png", UriKind.Relative);
            appBarDeleteButton.Text = "delete";
            appBarDeleteButton.Click += new EventHandler(appBarDeleteButton_Click);
        }

        private void appBarPhotoButton_Click(object sender, EventArgs e)
        {
            ApplicationBar.IsVisible = false;
            visitPivot.IsEnabled = false;
            TaskStack.Visibility = System.Windows.Visibility.Visible;
            

            Storyboard storyboard = new Storyboard();
            TranslateTransform trans = new TranslateTransform() { X = 1.0, Y = 1.0 };
            TaskStack.RenderTransformOrigin = new Point(0.5, 0.5);
            TaskStack.RenderTransform = trans;

            DoubleAnimation moveAnim = new DoubleAnimation();
            moveAnim.Duration = TimeSpan.FromMilliseconds(350);
            moveAnim.From = -400;
            moveAnim.To = 0;
            Storyboard.SetTarget(moveAnim, TaskStack);
            Storyboard.SetTargetProperty(moveAnim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            storyboard.Completed += new System.EventHandler(storyboard_Completed);
            storyboard.Children.Add(moveAnim);
            storyboard.Begin();

        }

        private void storyboard_Completed(object sender, EventArgs e)
        {
           
        }

        private void txtPick_tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            visitPivot.IsEnabled = true;
            TaskStack.Visibility = System.Windows.Visibility.Collapsed;

            pct.Show();
        }

        private void txtCam_tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            visitPivot.IsEnabled = true;
            TaskStack.Visibility = System.Windows.Visibility.Collapsed;

            cct.Show();
        }

 
        void cct_Completed(object sender, PhotoResult e)
        {
            SavePhoto(e);
        }

        private void pct_Completed(object sender, PhotoResult e)
        {
            SavePhoto(e);
        }

        private void SavePhoto(PhotoResult e)
        {
            if (e != null && e.TaskResult == TaskResult.OK)
            {
                try
                {
                    string photoName;
                    photoName = Path.GetFileName(e.OriginalFileName);
                    string thumbName;
                    thumbName = Path.GetFileNameWithoutExtension(e.OriginalFileName) + "_thumb.jpg";
                    IsolatedStorageFileStream targetStream;
                    BitmapImage bmi = new BitmapImage();

                    // Save photo as JPEG to the local folder.
                    using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (targetStream = isStore.OpenFile(photoName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            // Initialize the buffer for 4KB disk pages.
                            byte[] readBuffer = new byte[4096];
                            int bytesRead = -1;

                            // Copy the image to the local folder. 
                            while ((bytesRead = e.ChosenPhoto.Read(readBuffer, 0, readBuffer.Length)) > 0)
                            {
                                targetStream.Write(readBuffer, 0, bytesRead);
                            }

                            
                            newPic = targetStream.Name;
                            bmi.SetSource(targetStream);   
                           

                            // Save a thumb
                            WriteableBitmap bmImage = new WriteableBitmap(bmi);

                            using (IsolatedStorageFileStream thumbStream = isStore.OpenFile(thumbName, FileMode.OpenOrCreate))
                            {
                                // Shrink the pic to 10% original size
                                int width = bmi.PixelWidth / 10;
                                int height = bmi.PixelHeight / 10;

                                bmImage.SaveJpeg(
                                    thumbStream,
                                    width,
                                    height,
                                    0,
                                    70);

                                newThumb = thumbStream.Name;
                                thumbStream.Close();
                            }

                            // Add to the view model's collection, which is what the list in the UI is bound to
                            VisitPhotosTable vpt = new VisitPhotosTable();
                            vpt.VisitID = -1; // it's a dummy entry. We won't save this to the DB.
                            vpt.Photo = newPic;
                            vpt.Thumb = newThumb;
                            newPhotos.Add(vpt);
                            App.ViewModel.VisitPhotos.Add(vpt); // Just to add it to the displayed photos.new
                        }
                        targetStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error capturing photo. \n" + ex.Message);
                }
            }
    

            // Be nice and show the right pivot.
            visitPivot.SelectedItem = visitPhotosPivotItem;
            ApplicationBar.IsVisible = true;
        }

      
        private void BuildLists()
        {
            // Build the weather list
            Weather = new ObservableCollection<string>();
            Weather.Add(AppResources.WeatherClear);
            Weather.Add(AppResources.WeatherRain);
            Weather.Add(AppResources.WeatherHeavyRain);
            Weather.Add(AppResources.WeatherLightRain);
            Weather.Add(AppResources.WeatherOvercast);
            Weather.Add(AppResources.WeatherPartlyCloudy);
            Weather.Add(AppResources.WeatherSnow);
            Weather.Add(AppResources.WeatherSunny);

            WeatherLP.ItemsSource = Weather;

            // Build the tide list
            Tides = new ObservableCollection<string>();
            Tides.Add(AppResources.TideNontidal);
            Tides.Add(AppResources.TideHigh);
            Tides.Add(AppResources.TideLow);
            Tides.Add(AppResources.TideSlackHigh);
            Tides.Add(AppResources.TideSlackLow);
            Tides.Add(AppResources.TideIncoming);
            Tides.Add(AppResources.TideOutgoing);

            TideLP.ItemsSource = Tides;
        }

       

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the app that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion


        // Jump through hoops to make colors work in text boxes
        private void TempText_GotFocus(object sender, RoutedEventArgs e)
        {
            SolidColorBrush brush = (sender as TextBox).Foreground as SolidColorBrush;
            if (null != brush)
            {
                brush.Color = Colors.Black;
            }

        }

        private void TempText_LostFocus(object sender, RoutedEventArgs e)
        {
            SolidColorBrush brush = (sender as TextBox).Foreground as SolidColorBrush;
            if (null != brush)
            {
                brush.Color = Colors.White;
            }
        }

        private void TempText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void saveCheckinAppBarButton_Click(object sender, EventArgs e)
        {
            VisitTable visit;

            if (pagemode == "dirty")
            {
                // editing
                visit = App.ViewModel.CurrentVisit;
                Debug.Assert(visit != null);                
            }
            else
            {
                // Create a record
                visit = new VisitTable();
            }

            try
            {

                visit.Bait = (BaitLP.SelectedItem as BaitTable).BaitName;
                visit.Rig = (RigLP.SelectedItem as RigTable).RigName;
                visit.BaitColor = (BaitColorLP.SelectedItem as BaitColorTable).BaitColorName;
                visit.Caught = (CaughtText.Text != "") ? Int32.Parse(CaughtText.Text) : 0;
                visit.Depth = (DepthText.Text != "") ? Double.Parse(DepthText.Text) : 0;
                visit.Rating = visitRating.Value;
                visit.Species = (SpeciesLP.SelectedItem as SpeciesTable).SpeciesName;
                visit.SpotID = App.ViewModel.CurrentSpot.SpotItemId;
                visit.Temp = Double.Parse(TempText.Text);
                visit.Tide = TideLP.SelectedItem.ToString();
                visit.VisitDateTime = System.DateTime.Now;
                visit.WaterTemp = Double.Parse(WaterTempText.Text);
                visit.Weather = WeatherLP.SelectedItem.ToString();
                visit.Note = NoteText.Text;
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Empty values are not allowed.\nProvide estimates if you don't know exact values.");
                return;
            }

            if (pagemode == "dirty")
            {
                App.ViewModel.SaveVisit(visit);
            }
            else
            {
                App.ViewModel.AddVisit(visit);
            }

            // Save new photos
            App.ViewModel.SaveVisitPhotos(newPhotos);
            bSaved = true;

            if (photosToRemove.Count > 0)
            {
                // Delete photos marked for deletion
                App.ViewModel.DeleteVisitPhotos(photosToRemove);
            }

            // Return
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }  
        }

        private void cancelCheckinAppBarButton_Click(object sender, EventArgs e)
        {
            // Remove any new photos added.
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();

            foreach (VisitPhotosTable photo in newPhotos)
            {               
                // Delete from isolated storage 
                if (iso.FileExists(photo.Photo))
                {
                    iso.DeleteFile(photo.Photo);
                }

                // Delete from isolated storage 
                if (iso.FileExists(photo.Thumb))
                {
                    iso.DeleteFile(photo.Thumb);
                }
            }

            if (NavigationService.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (bSaved == false)
            {
                // Remove any new photos added.
                IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();

                foreach (VisitPhotosTable photo in newPhotos)
                {
                    // Delete from isolated storage 
                    if (iso.FileExists(photo.Photo))
                    {
                        iso.DeleteFile(photo.Photo);
                    }

                    // Delete from isolated storage 
                    if (iso.FileExists(photo.Thumb))
                    {
                        iso.DeleteFile(photo.Thumb);
                    }
                }
            }

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string mode;

            // Determine what to show based on the query parameter
            NavigationContext.QueryString.TryGetValue("mode", out mode);

            if (mode == "edit")
            {
                pagemode = "edit";
                // Don't want the param sticking around.
                NavigationContext.QueryString.Clear();
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = true;

            if (pagemode == "edit")
            {
                Debug.Assert(App.ViewModel.CurrentVisit != null);

                VisitTable v = App.ViewModel.CurrentVisit;

                TideLP.SelectedItem = v.Tide;
                WeatherLP.SelectedItem = v.Weather;

                // I'm not sure why, but these three need to use index.
                // The other two listpickers are populated directly in code.
                // These three use a binding. I imagine that has something 
                // to do with the differences...
                // It's possible that a value has been stored that's no longer in the DB
                try
                {
                    var current = from species in App.ViewModel.AllSpecies where species.SpeciesName == v.Species select species;
                    int index = App.ViewModel.AllSpecies.IndexOf(current.First() as SpeciesTable);
                    SpeciesLP.SelectedIndex = index;
                }
                catch (InvalidOperationException ioexc)
                {

                    MessageBox.Show("The species name " + v.Species + " is no longer valid.\nChoose another or use the Settings page first to edit the list.");
                }

                try
                {
                    var current = from bc in App.ViewModel.AllBaitColors where bc.BaitColorName == v.BaitColor select bc;
                    int index = App.ViewModel.AllBaitColors.IndexOf(current.First() as BaitColorTable);
                    BaitColorLP.SelectedIndex = index;
                }
                catch (InvalidOperationException ioexc)
                {

                    MessageBox.Show("The bait color name " + v.BaitColor + " is no longer valid.\nChoose another or use the Settings page first to edit the list.");
                }

                try
                {
                    var current = from bc in App.ViewModel.AllBaits where bc.BaitName == v.Bait select bc;
                    int index = App.ViewModel.AllBaits.IndexOf(current.First() as BaitTable);
                    BaitLP.SelectedIndex = index;
                }
                catch (InvalidOperationException ioexc)
                {

                    MessageBox.Show("The bait name " + v.Bait + " is no longer valid.\nChoose another or use the Settings page first to edit the list.");
                }

                try
                {
                    var current = from bc in App.ViewModel.AllRigs where bc.RigName == v.Rig select bc;
                    int index = App.ViewModel.AllRigs.IndexOf(current.First() as RigTable);
                    RigLP.SelectedIndex = index;
                }
                catch (InvalidOperationException ioexc)
                {

                    MessageBox.Show("The rig name " + v.Rig + " is no longer valid.\nChoose another or use the Settings page first to edit the list.");
                }


                TempText.Text = v.Temp.ToString();
                CaughtText.Text = v.Caught.ToString();
                visitRating.Value = v.Rating;
                NoteText.Text = v.Note == null ? String.Empty : v.Note;
                WaterTempText.Text = v.WaterTemp.ToString();
                DepthText.Text = v.Depth.ToString();

                pagemode = "dirty";
            }
            else if(pagemode == "") // it's a new visit
            {
                App.ViewModel.CurrentVisit = new VisitTable();
                App.ViewModel.CurrentVisit.VisitItemId = -1; // its temporary
            }

            
            App.ViewModel.LoadVisitPhotos();
          
        }

 
        // Configure the appbar buttons depending on the page mode
        private void ShowButtons(Modes mode)
        {
            if (mode == Modes.add)
            {            

                if ((!ApplicationBar.Buttons.Contains(appBarPhotoButton)))
                    ApplicationBar.Buttons.Add(appBarPhotoButton);

                if (!(ApplicationBar.Buttons.Contains(appBarOkButton)))
                    ApplicationBar.Buttons.Add(appBarOkButton);

                if (!(ApplicationBar.Buttons.Contains(appBarCancelButton)))
                    ApplicationBar.Buttons.Add(appBarCancelButton);

            }
            else if (mode == Modes.edit)
            {
                

                if ((!ApplicationBar.Buttons.Contains(appBarPhotoButton)))
                    ApplicationBar.Buttons.Add(appBarPhotoButton);

                if (!(ApplicationBar.Buttons.Contains(appBarOkButton)))
                    ApplicationBar.Buttons.Add(appBarOkButton);

                if (!(ApplicationBar.Buttons.Contains(appBarCancelButton)))
                    ApplicationBar.Buttons.Add(appBarCancelButton);


            }
            else if (mode == Modes.view)
            {

                if ((ApplicationBar.Buttons.Contains(appBarCancelButton)))
                    ApplicationBar.Buttons.Remove(appBarCancelButton);

                if ((ApplicationBar.Buttons.Contains(appBarPhotoButton)))
                    ApplicationBar.Buttons.Remove(appBarPhotoButton);

                if ((ApplicationBar.Buttons.Contains(appBarOkButton)))
                    ApplicationBar.Buttons.Remove(appBarOkButton);

 

            }
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            
        }

        // Cache the last picture tapped
        Grid lastGrid;

        // Grid background turns gray when selected.
        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid g = sender as Grid;

            SolidColorBrush b = g.Background as SolidColorBrush;

            Debug.Assert(b != null);

            if (g == lastGrid)
            {
                if (b.Color == Colors.LightGray)
                {
                    g.Background = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
            else if (lastGrid != null)
            {
                lastGrid.Background = new SolidColorBrush(Colors.Transparent);
                g.Background = new SolidColorBrush(Colors.LightGray);
                lastGrid = g;
            }
            else // There is no lastGrid, so this must be the first. Just go gray.
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                lastGrid = g;
            }

            b = g.Background as SolidColorBrush; // See what the background is now
            if (b.Color == Colors.LightGray)
            {
                if (!ApplicationBar.Buttons.Contains(appBarDeleteButton))
                    ApplicationBar.Buttons.Add(appBarDeleteButton);
            }
            else
            {
                if(ApplicationBar.Buttons.Contains(appBarDeleteButton))
                    ApplicationBar.Buttons.Remove(appBarDeleteButton);
            }
        }

        private void appBarDeleteButton_Click(object sender, EventArgs e)
        {
            VisitPhotosTable vpt = photoList.SelectedItem as VisitPhotosTable;

            if (vpt.VisitID == -1)
            {
                // It's a new pic
                newPhotos.Remove(vpt);

                // Remove the photo from iso store
                IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
                              
                // Delete from isolated storage 
                if (iso.FileExists(vpt.Photo))
                {
                    iso.DeleteFile(vpt.Photo);
                }

                // Delete from isolated storage 
                if (iso.FileExists(vpt.Thumb))
                {
                    iso.DeleteFile(vpt.Thumb);
                }
              
            }
            else
            {
                // It's in the database
                // Keep a list so we can delete
                // if user saves changes
                photosToRemove.Add(vpt);
            }

            // Either way, remove from listpicker in UI
            App.ViewModel.VisitPhotos.Remove(vpt);         
          }

        private void visitPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If user selected a photo and then swiped away from Photos, then hide the delete button.
            if(visitPivot.SelectedIndex != 1 && ApplicationBar.Buttons.Contains(appBarDeleteButton))
            {
                ApplicationBar.Buttons.Remove(appBarDeleteButton);
            }
            else if (visitPivot.SelectedIndex == 1)
            {
                // Remove the visual indication of a selection.
                if (lastGrid != null)
                {
                    lastGrid.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }

     
    }
}