using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Data;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
// internal data modeling
using FishingSpots.Model;
using FishingSpots.ViewModel;
// for location api
using Windows.Devices.Geolocation;
using System.Device.Location;
// camera
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Phone;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using System.Windows.Media.Animation;



namespace FishingSpots
{
    enum Modes
    {
        add,
        mapadd,
        edit,
        view,
        camedit
    };
    
    public partial class Spot : PhoneApplicationPage
    {
        Modes viewMode;  // current page view mode
        Modes oldMode; // Cache the mode during cam capture.
        string oldPic = String.Empty; // Path to existing pic
        string newPic = String.Empty; // Path to most recent pic from capture task    
        string oldThumb = String.Empty; // Path to existing pic
        string newThumb = String.Empty; // Path to most recent pic from capture task     
        string photoPath = String.Empty; // Path to pic to save
        string thumbPath = String.Empty; // Path to thumbnail to save
       

        CameraCaptureTask cct;
        PhotoChooserTask pct;
       
        // location fields
        bool locPermitted = false;
        double latitude;
        double longitude;
        String locstring = String.Empty;
 
        // appbar button fields
        ApplicationBarIconButton appBarOkButton;
        ApplicationBarIconButton appBarCancelButton;
        ApplicationBarIconButton appBarEditButton;
        ApplicationBarIconButton appBarDeleteButton;
        ApplicationBarIconButton appBarMapButton;
        ApplicationBarIconButton appBarVisitButton;
        ApplicationBarIconButton appBarPhotoButton;

        // Flag to deal with the back button cancelling
        // the add or edit
        bool bSaved = false;

       
        public Spot()
        {
            InitializeComponent();
     
            // Create the application bar
            ApplicationBar = new ApplicationBar();

            appBarOkButton = new ApplicationBarIconButton();
            appBarOkButton.IconUri = new Uri("/Images/save.png", UriKind.Relative);
            appBarOkButton.Text = "save";
            appBarOkButton.Click += new EventHandler(appBarOkButton_Click);

            appBarCancelButton = new ApplicationBarIconButton();
            appBarCancelButton.IconUri = new Uri("/Images/cancel.png", UriKind.Relative);
            appBarCancelButton.Text = "cancel";
            appBarCancelButton.Click += new EventHandler(appBarCancelButton_Click);

            appBarEditButton = new ApplicationBarIconButton();
            appBarEditButton.IconUri = new Uri("/Images/edit.png", UriKind.Relative);
            appBarEditButton.Text = "edit";
            appBarEditButton.Click += new EventHandler(appBarEditButton_Click);

            appBarDeleteButton = new ApplicationBarIconButton();
            appBarDeleteButton.IconUri = new Uri("/Images/delete.png", UriKind.Relative);
            appBarDeleteButton.Text = "delete";
            appBarDeleteButton.Click += new EventHandler(appBarDeleteButton_Click);

            appBarMapButton = new ApplicationBarIconButton();
            appBarMapButton.IconUri = new Uri("/Images/appbar.map.gps.png", UriKind.Relative);
            appBarMapButton.Text = "map";
            appBarMapButton.Click += new EventHandler(appBarMapButton_Click);

            appBarVisitButton = new ApplicationBarIconButton();
            appBarVisitButton.IconUri = new Uri("/Images/appbar.check.png", UriKind.Relative);
            appBarVisitButton.Text = "check in";
            appBarVisitButton.Click += new EventHandler(appBarVisitButton_Click);

            appBarPhotoButton = new ApplicationBarIconButton();
            appBarPhotoButton.IconUri = new Uri("/Images/feature.camera.png", UriKind.Relative);
            appBarPhotoButton.Text = "check in";
            appBarPhotoButton.Click += new EventHandler(appBarPhotoButton_Click);

            // photos
            cct = new CameraCaptureTask();
            cct.Completed += new EventHandler<PhotoResult>(cct_Completed);

            pct = new PhotoChooserTask();
            pct.Completed += new EventHandler<PhotoResult>(pct_Completed);

            if (App.ViewModel.CurrentSpot != null)
            {
                photoPath = App.ViewModel.CurrentSpot.Photo;
                thumbPath = App.ViewModel.CurrentSpot.Thumb;
            }
           
        }

        private void appBarPhotoButton_Click(object sender, EventArgs e)
        {
            if (viewMode != Modes.camedit)
            {
                // Cache the old stuff
                oldMode = viewMode;
            }

            if (App.ViewModel.CurrentSpot != null)
            {
                oldPic = App.ViewModel.CurrentSpot.Photo;
                oldThumb = App.ViewModel.CurrentSpot.Thumb;

            }

            // Change the view mode to indicate that
            // we're doing a photo capture
            viewMode = Modes.camedit;

            // If we have cached a new pic, discard it
            // because the user wants to shoot another photo
            if (!String.IsNullOrEmpty(newPic))
            {
                try
                {
                    IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication();
                    isStore.DeleteFile(newPic);
                    isStore.DeleteFile(newThumb);
                }
                catch (IsolatedStorageException isoexc)
                {
                    Debug.Assert(false);
                    // okay, couldn't find the file
                }
            }

            spotpivot.IsEnabled = false;
            ApplicationBar.IsVisible = false;
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

            //cct.Show();
        }

        private void storyboard_Completed(object sender, EventArgs e)
        {

        }

        private void appBarVisitButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Visit.xaml", UriKind.Relative));
        }

        private void appBarMapButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(App.ViewModel.CurrentSpot != null);
            NavigationService.Navigate(new Uri("/SpotsMap.xaml?mode=mapspot", UriKind.Relative));
        }

        private void appBarDeleteButton_Click(object sender, EventArgs e)
        {
            MessageBoxResult result =
                    MessageBox.Show("Permanently delete this spot and its checkins?",
                    "Location",
                    MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                // Delete the current record.
                App.ViewModel.DeleteCurrentSpot();

                // Return
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }      

            }
        }

        // Determine whether a new pic/thumb should replace an existing pic
        private void CheckForNewPic()
        {
            if (!String.IsNullOrEmpty(newPic))
            {
                // We have new pics. 
                photoPath = newPic;
                thumbPath = newThumb;

                if (!String.IsNullOrEmpty(oldPic) && oldPic != newPic)
                {
                    // Delete the old ones
                    IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();

                    if (iso.FileExists(oldPic) && !oldPic.Contains("SmTile.png"))
                    {
                        iso.DeleteFile(oldPic);
                    }
                    if (iso.FileExists(oldThumb) && !oldThumb.Contains("SmTile.png"))
                    {
                        iso.DeleteFile(oldThumb);
                    }
                }

            }
        }


        private void appBarOkButton_Click(object sender, EventArgs e)
        { 
                if (viewMode == Modes.add)
                {
                    // Confirm text in name box at a minimum
                    if (NameTextBox.Text.Length > 0)
                    {
                        CheckForNewPic();

                        SpotTable newSpotTable;
                            
                        if (photoPath == null || photoPath == "")
                        {
                            photoPath = App.ViewModel.Settings.DefaultImage;                            
                        }

                        if (thumbPath == null || thumbPath == "")
                        {
                            thumbPath = App.ViewModel.Settings.DefaultImage;
                        }

                        try
                        {

                            // Create the new spot
                            newSpotTable = new SpotTable
                            {
                                SpotName = NameTextBox.Text,
                                Description = DescriptionTextBox.Text,
                                //Rating = RatingControl.Value,
                                VisitCount = 1,
                                Latitude = double.Parse(latBlock.Text),
                                Longitude = double.Parse(longBlock.Text),
                                Photo = photoPath,
                                Thumb = thumbPath
                            };

                        }
                        catch
                        {
                            MessageBox.Show("Can't save spot.\n Check your location values and try again.");
                            return;
                        }

                        // Add the spot to the view model
                        App.ViewModel.AddSpot(newSpotTable);

                        bSaved = true;

                        // Return
                        if (NavigationService.CanGoBack)
                        {
                            NavigationService.GoBack();
                        }  
                    }
                }
                else if (viewMode == Modes.view)
                {
                    // Return
                    if (NavigationService.CanGoBack)
                    {
                        NavigationService.GoBack();
                    }                  
                }
                else if (viewMode == Modes.edit)
                {
                    CheckForNewPic();

                    SpotTable currSpot = App.ViewModel.CurrentSpot;

                    Debug.Assert(currSpot != null);

                    currSpot.Latitude = double.Parse(latBlock.Text);
                    currSpot.Longitude = double.Parse(longBlock.Text);
                    currSpot.SpotName = NameTextBox.Text;                    
                    currSpot.Description = DescriptionTextBox.Text;
                    //currSpot.Rating = RatingControl.Value;
                    currSpot.Photo = photoPath;
                    currSpot.Thumb = thumbPath;

                    App.ViewModel.SaveCurrentSpot();

                    bSaved = true;
                
                    // Show the selected spot, not in edit mode
                    InputStack.Visibility = Visibility.Collapsed;
                    DisplayStack.Visibility = Visibility.Visible;

                    viewMode = Modes.view;
                    ShowButtons(viewMode);                   
                } 
        }

        private void appBarCancelButton_Click(object sender, EventArgs e)
        {
            // Discard any new pic/thumb
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso.FileExists(newPic) && newPic != oldPic)
            {
                iso.DeleteFile(newPic);

                if (iso.FileExists(newThumb))
                {
                    iso.DeleteFile(newThumb);
                }
            }            

            if (viewMode == Modes.add)
            {
                // Return
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
            else if (viewMode == Modes.view)
            {
                // Return
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
            else if (viewMode == Modes.edit)
            {
                // Show the selected spot, not in edit mode
                InputStack.Visibility = Visibility.Collapsed;
                DisplayStack.Visibility = Visibility.Visible;

                // Make sure the binding is set correctly.
                // We might have changed it during a camera capture during editing.
                imgThumb.SetBinding(Image.SourceProperty, new Binding("Thumb"));

                // We're back in "view" mode
                ShowButtons(Modes.view);
                viewMode = Modes.view;
            }
                
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (bSaved == false)
            {
                // Discard any new pic/thumb
                IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
                if (iso.FileExists(newPic) && newPic != oldPic)
                {
                    iso.DeleteFile(newPic);

                    if (iso.FileExists(newThumb))
                    {
                        iso.DeleteFile(newThumb);
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
           
            switch (mode)
            {
                case "view":
                    viewMode = Modes.view;
                    break;
                case "edit":
                    viewMode = Modes.edit;
                    break;
                case "add":
                    viewMode = Modes.add;                    
                    break;
                case "mapadd":
                    viewMode = Modes.mapadd;
                    break;
                default:
                    break;
            }

            // We don't want the cached mode hanging around during navigation
            // to and from Tasks, like the camera.
            NavigationContext.QueryString.Clear();

            // Need to update the thumbnail after a capture
            // when in edit mode.a
            if(viewMode == Modes.camedit)
            {
                // Bind to the current capture, for now.
                if (!String.IsNullOrEmpty(newThumb))
                {
                    imgThumb.Source = new BitmapImage(new Uri(newThumb));
                }
                viewMode = oldMode;
            }            
        }

        

        private void appBarEditButton_Click(object sender, EventArgs e)
        {
            // Go into edit mode
            InputStack.Visibility = Visibility.Visible;
            DisplayStack.Visibility = Visibility.Collapsed;
            ShowButtons(Modes.edit);

            viewMode = Modes.edit;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

                if (viewMode == Modes.add)
                {  
                    pgLoc.Visibility = System.Windows.Visibility.Visible;
                    doLocation();

                }
                else if (viewMode == Modes.mapadd)
                {                

                    latBlock.Text = App.ViewModel.CurrentSpot.Latitude.ToString();
                    longBlock.Text = App.ViewModel.CurrentSpot.Longitude.ToString();
                    pgLoc.Visibility = System.Windows.Visibility.Collapsed;
                    // Show the input controls
                    InputStack.Visibility = Visibility.Visible;
                    DisplayStack.Visibility = Visibility.Collapsed;

                    // adjust the app bar buttons
                    ShowButtons(Modes.add);
                    
                    // We don't need the "mapadd" mode after we've 
                    // bypassed doLocation().
                    viewMode = Modes.add;
                }
                else if (viewMode == Modes.view)
                {
                    // set the data contexts for the page                
                    pivotitem1.DataContext = App.ViewModel.CurrentSpot;
                    pivotitem2.DataContext = App.ViewModel;

                    // Show the selected spot, not in edit mode
                    InputStack.Visibility = Visibility.Collapsed;
                    DisplayStack.Visibility = Visibility.Visible;
                    ShowButtons(Modes.view);

                    App.ViewModel.LoadVisits();

                    // There is no binding to UI controls
                    photoPath = App.ViewModel.CurrentSpot.Photo;
                    thumbPath = App.ViewModel.CurrentSpot.Thumb;

                    Debug.Assert(App.ViewModel.CurrentSpot != null);

                }

           
           
        }

        // Get the latitude and longitude
        private async void doLocation()
        {
            var pos = await App.FLocation.getOneShotLocation();

            if (pos != null)
            {
                Geoposition g = (Geoposition)pos;

                latitude = g.Coordinate.Latitude;
                longitude = g.Coordinate.Longitude;

                // right now we're displaying on the page
                // in final version, we'll only use maps to show lat/long.
                latBlock.Text = latitude.ToString();
                longBlock.Text = longitude.ToString();
            }
            else if (App.ViewModel.CurrentSpot != null)
            {
                latBlock.Text = App.ViewModel.CurrentSpot.Latitude.ToString();
                longBlock.Text = App.ViewModel.CurrentSpot.Longitude.ToString();
            }
            else
            {
                latBlock.Text = "unavailable";
                longBlock.Text = "unavailable";
            }

            pgLoc.Visibility = System.Windows.Visibility.Collapsed;
            // Show the input controls
            InputStack.Visibility = Visibility.Visible;
            DisplayStack.Visibility = Visibility.Collapsed;

            // adjust the app bar buttons
            ShowButtons(Modes.add);
        }

        // Configure the appbar buttons depending on the page mode
        private void ShowButtons(Modes mode)
        {
            if (mode == Modes.add)
            {
                if ((ApplicationBar.Buttons.Contains(appBarEditButton)))
                    ApplicationBar.Buttons.Remove(appBarEditButton);

                if ((ApplicationBar.Buttons.Contains(appBarVisitButton)))
                    ApplicationBar.Buttons.Remove(appBarVisitButton);

                if ((ApplicationBar.Buttons.Contains(appBarDeleteButton)))
                    ApplicationBar.Buttons.Remove(appBarDeleteButton);

                if ((ApplicationBar.Buttons.Contains(appBarMapButton)))
                    ApplicationBar.Buttons.Remove(appBarMapButton);

                if ((!ApplicationBar.Buttons.Contains(appBarPhotoButton)))
                    ApplicationBar.Buttons.Add(appBarPhotoButton);

                if (!(ApplicationBar.Buttons.Contains(appBarOkButton)))
                    ApplicationBar.Buttons.Add(appBarOkButton);
                
                if (!(ApplicationBar.Buttons.Contains(appBarCancelButton)))
                    ApplicationBar.Buttons.Add(appBarCancelButton);               

                

                // no pivoting while adding
                spotpivot.IsLocked = true;

            }
            else if (mode == Modes.edit)
            {
                if ((ApplicationBar.Buttons.Contains(appBarEditButton)))
                    ApplicationBar.Buttons.Remove(appBarEditButton);

                if ((ApplicationBar.Buttons.Contains(appBarVisitButton)))
                    ApplicationBar.Buttons.Remove(appBarVisitButton);

                if ((ApplicationBar.Buttons.Contains(appBarDeleteButton)))
                    ApplicationBar.Buttons.Remove(appBarDeleteButton);

                if ((ApplicationBar.Buttons.Contains(appBarMapButton)))
                    ApplicationBar.Buttons.Remove(appBarMapButton);

                if ((!ApplicationBar.Buttons.Contains(appBarPhotoButton)))
                    ApplicationBar.Buttons.Add(appBarPhotoButton);
                
                if (!(ApplicationBar.Buttons.Contains(appBarOkButton)))
                    ApplicationBar.Buttons.Add(appBarOkButton);

                if (!(ApplicationBar.Buttons.Contains(appBarCancelButton)))
                    ApplicationBar.Buttons.Add(appBarCancelButton);                

                

                // no pivoting while editing
                spotpivot.IsLocked = true;

            }
            else if (mode == Modes.view)
            {              
                
                if ((ApplicationBar.Buttons.Contains(appBarCancelButton)))
                    ApplicationBar.Buttons.Remove(appBarCancelButton);

                if ((ApplicationBar.Buttons.Contains(appBarPhotoButton)))
                    ApplicationBar.Buttons.Remove(appBarPhotoButton);

                if ((ApplicationBar.Buttons.Contains(appBarOkButton)))
                    ApplicationBar.Buttons.Remove(appBarOkButton);

                if (!(ApplicationBar.Buttons.Contains(appBarEditButton)))
                    ApplicationBar.Buttons.Add(appBarEditButton);

                if ((!ApplicationBar.Buttons.Contains(appBarDeleteButton)))
                    ApplicationBar.Buttons.Add(appBarDeleteButton);

                if ((!ApplicationBar.Buttons.Contains(appBarMapButton)))
                    ApplicationBar.Buttons.Add(appBarMapButton);

                if ((!ApplicationBar.Buttons.Contains(appBarVisitButton)))
                    ApplicationBar.Buttons.Add(appBarVisitButton);

                // pivoting allowed while viewing
                spotpivot.IsLocked = false;

            }
        }

        private void spotpivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if ((string)(e.Item.Header) == "checkins")
            {
                // Don't show the appbar for the checkins pivot.
                ApplicationBar.IsVisible = false;
            }
            else
            {
                ApplicationBar.IsVisible = true;
            }
        }

        private void visitsItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (visitsItems.SelectedItem != null)
            {
                App.ViewModel.CurrentVisit = (VisitTable)e.AddedItems[0];

                NavigationService.Navigate(new Uri("/VisitDetails.xaml", UriKind.Relative));

                // Clear the selection when done
                visitsItems.SelectedItem = null;
            }
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
            // Save the pic.
            if (e.TaskResult == TaskResult.OK)
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

                            bmi.SetSource(targetStream);                            
                            newPic = targetStream.Name;
                        

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
                            targetStream.Close();  
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error capturing photo. \n" + ex.Message);
                }
            }
            ApplicationBar.IsVisible = true;
        }

        private void imgThumb_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (viewMode != Modes.add && viewMode != Modes.edit &&
                viewMode != Modes.camedit)
            {
                string path;

                if (!String.IsNullOrEmpty(newPic))
                {
                    path = newPic;
                }
                else
                {
                    path = oldPic;
                }

                if (((BitmapImage)(imgThumb.Source)).UriSource.ToString().Contains("SmTile.png"))
                {
                    return;  // Don't show the full size logo like it's a real pic
                }

                NavigationService.Navigate(new Uri("/PhotoViewer.xaml?spot=" + App.ViewModel.CurrentSpot.SpotName + "&path=" + path, UriKind.Relative));
            }
        }

        private void txtPick_tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            spotpivot.IsEnabled = true;
            TaskStack.Visibility = System.Windows.Visibility.Collapsed;

            pct.Show();
        }

        private void txtCam_tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            spotpivot.IsEnabled = true;
            TaskStack.Visibility = System.Windows.Visibility.Collapsed;

            cct.Show();
        }
    }
}