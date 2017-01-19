using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.ComponentModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FishingSpots.ViewModel;
using Microsoft.Live.Controls;
using System.IO.IsolatedStorage;
using System.IO;
using FishingSpots.Model;
using Microsoft.Live;
using Microsoft.Phone.Tasks;
using System.Threading;
using System.Threading.Tasks;



namespace FishingSpots
{
    public partial class Settings : PhoneApplicationPage, IProgress<LiveOperationProgress>
    {
        internal class UploadProgress : IProgress<LiveOperationProgress>
        {
            public void Report(LiveOperationProgress value)
            {
                throw new NotImplementedException();
            }
        }

        private LiveConnectClient client = null;
        private LiveAuthClient authClient = null;
        private LiveConnectSession session = null;
        private string strSkyDriveFolderName = "FishingSpotsAppBackup"; // The folder name for backups
        private string strSkyDriveFolderID = string.Empty;              // The id of the folder name for backups
        // private string fileID = string.Empty;                                   // The file id of the database backup file
        private IsolatedStorageFileStream readStream = null;            // The stream for restoring data 
        private string strSkyDriveDBName = "FishingSpots.sdf";                          // The name of the database sdf file
        private Boolean backupInProgress = false;
        private ApplicationBar applBar;
        private MarketplaceReviewTask mrt;
        private List<string> scopes;

        private CancellationTokenSource cts;


        public Settings()
        {
            InitializeComponent();
            DataContext = App.ViewModel.Settings;

    
           // SignIn button
           btnSignIn.ClientId = "000000004410EF6D";
           btnSignIn.Scopes = "wl.basic wl.signin wl.offline_access wl.skydrive wl.skydrive_update";
           btnSignIn.Branding = BrandingType.Skydrive;
           btnSignIn.TextType = ButtonTextType.SignIn;
           btnSignIn.SessionChanged += btnSignIn_SessionChanged;

           scopes = new List<string>();
           scopes.Add("wl.basic");
           scopes.Add("wl.signin");
           scopes.Add("wl.offline_access");
           scopes.Add("wl.skydrive_update");
           

           ProgressStack.Visibility = System.Windows.Visibility.Collapsed;

                
       }      
   

        private void ApplicationBarMenuItemAllSpecies_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ListEdit.xaml?mode=species", UriKind.Relative));
        }

        private void ApplicationBarMenuItemAllBaitColors_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ListEdit.xaml?mode=baitcolor", UriKind.Relative));
        }

        private void ApplicationBarMenuItemAllBaits_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ListEdit.xaml?mode=bait", UriKind.Relative));
        }

        private void ApplicationBarMenuItemAllRigs_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ListEdit.xaml?mode=rig", UriKind.Relative));
        }

        public void Report(LiveOperationProgress value)
        {
           // do nothing
        }



        private async void  btnSignIn_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            // If the user is signed in.
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                session = e.Session;
                client = new LiveConnectClient(e.Session);
                btnBackup.IsEnabled = true;
                btnRestore.IsEnabled = true;
                authClient = new LiveAuthClient("http://www.travisworld.com/redirect");
                authClient.InitializeAsync(scopes);

                //cts = new CancellationTokenSource();

                //client.BackgroundTransferPreferences = BackgroundTransferPreferences.None;

                //foreach (LivePendingUpload pendingUpload in this.client.GetPendingBackgroundUploads())
                //{
                //    try
                //    {
                //        LiveOperationResult operationResult = await pendingUpload.AttachAsync(cts.Token, this);
                //        dynamic result = operationResult.Result;
                //        MessageBox.Show("Upload successful. Uploaded to " + result.source);
                //    }
                //    catch (TaskCanceledException)
                //    {
                //        MessageBox.Show("Upload canceled.");
                //    }

                //}
            }
            else  // Otherwise the user isn't signed in.
            {
                client = null;
                btnBackup.IsEnabled = false;
                btnRestore.IsEnabled = false;
            }
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (client == null || client.Session == null)
            {
                MessageBox.Show("You must sign in first.");
            }
            else
            {
                if (MessageBox.Show("This will overwrite existing backup files. Don't interrupt the process. Connect your phone to its charger.", "Backup?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    backupInProgress = true;
                    SettingsStack.Visibility = System.Windows.Visibility.Collapsed;
                    ProgressStack.Visibility = System.Windows.Visibility.Visible;
                    this.ApplicationBar.IsVisible = false;


                    Backup();     
                }
            }
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {

            if (client == null || client.Session == null)
            {
                MessageBox.Show("You must sign in first.");
            }
            else
            {
                if (MessageBox.Show("This will overwrite your current database and other files. Don't interrupt the process. Connect your phone to its charger.", "Restore?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    backupInProgress = true;
                    SettingsStack.Visibility = System.Windows.Visibility.Collapsed;
                    ProgressStack.Visibility = System.Windows.Visibility.Visible;
                    this.ApplicationBar.IsVisible = false;

                    Restore();     
                    
                }
            }

        }

        private async void Restore()
        {
            int numfiles = 0;

            backupStatus.Text = "Looking for folder";
            pgBackup.Visibility = System.Windows.Visibility.Visible;

            // Get the root folder
            LiveOperationResult opRes = await client.GetAsync("me/skydrive");
            dynamic result = opRes.Result;
            string sdRoot = result.id;
            int sdCount = result.count;

            opRes = await client.GetAsync(sdRoot + "/files");
            result = opRes.Result;

            List<object> folders = (List<object>)result.data;

            // Loop all folders to check if the isolatedstoragefolder exists. 
            foreach (dynamic item in folders)
            {
                if (item.name == strSkyDriveFolderName)
                {
                    strSkyDriveFolderID = item.id;
                    backupStatus.Text = "Found folder";
                    numfiles = item.count;
                }
            }

            // If the IsolatedStorageFolder does not exist, bail.
            if (strSkyDriveFolderID == string.Empty)
            {
                backupStatus.Text = String.Empty;

                MessageBox.Show("Could not find backup folder named " + strSkyDriveFolderName + "." + "\nDid you move or delete it?");
                        
            }
            else
            {
                backupStatus.Text = "Downloading files";

                 // The folder must exist, it should have already been created. 
                if (strSkyDriveFolderID != string.Empty)
                {
                    using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        int filenum = 0;

                        try
                        {
                            backupInProgress = true;

                            backupStatus.Text = "Looking for existing backups";

                            // try to get the id of the existing database backup
                            opRes = await client.GetAsync(strSkyDriveFolderID + "/files");
                            result = opRes.Result;

                            List<object> files = (List<object>)result.data;

                            foreach (dynamic item in files)
                            {

                                if (item.name == strSkyDriveDBName)
                                {
                                    // Close the database.
                                    App.ViewModel.FishingSpotsDB.Dispose();
                                    App.ViewModel.FishingSpotsDB = null;
                                }

                                backupStatus.Text = "Restoring " + ++filenum + " of " + numfiles;
                                
                                LiveDownloadOperationResult dopRes = await client.DownloadAsync(string.Format("{0}/content",item.id));
                                Stream stream = dopRes.Stream;

                                using (readStream = iso.OpenFile(item.name, FileMode.Create, FileAccess.ReadWrite))
                                {
                                    stream.CopyTo(readStream);
                                    stream.Flush();
                                    stream.Close();
                                }

                                if (item.name == strSkyDriveDBName)
                                {
                                    // Create the ViewModel object.
                                    App.ViewModel.FishingSpotsDB = new FishingSpotsDataContext(App.DBConnectionString);

                                    // The old instance of FishLocation uses a compiled query that
                                    // still is compiled against the old DB instance. Refresh it.
                                    App.FLocation = new FishLocation();

                                    // Query the local database and load observable collections.
                                    App.ViewModel.LoadCollectionsFromDatabase();
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("There were errors when restoring.\n" + e.Message);
                        }
                    }
                }
            }
            ProgressStack.Visibility = System.Windows.Visibility.Collapsed;
            SettingsStack.Visibility = System.Windows.Visibility.Visible;
            this.ApplicationBar.IsVisible = true;
            backupInProgress = false;
        }

   
        private async void Backup()
        {
            backupStatus.Text = "Looking for folder";
            pgBackup.Visibility = System.Windows.Visibility.Visible;

            // Get the root folder
            LiveOperationResult opRes = await client.GetAsync("me/skydrive");
            dynamic result = opRes.Result;
            string sdRoot = result.id;
            int sdCount = result.count;

            opRes = await client.GetAsync(sdRoot + "/files");
            result = opRes.Result;
            
            List<object> folders = (List<object>)result.data;

            // Loop all folders to check if the isolatedstoragefolder exists. 
            foreach (dynamic item in folders)
            {
                if (item.name == strSkyDriveFolderName)
                {
                    strSkyDriveFolderID = item.id;
                    backupStatus.Text = "Found folder";
                }
            }

            // If the IsolatedStorageFolder does not exist, create it. 
            if (strSkyDriveFolderID == string.Empty)
            {
                backupStatus.Text = "Creating folder";

                Dictionary<string, object> skyDriveFolderData = new Dictionary<string, object>();
                skyDriveFolderData.Add("name", strSkyDriveFolderName);

                opRes = await client.PostAsync("me/skydrive", skyDriveFolderData);
                result = opRes.Result;

                strSkyDriveFolderID = result.id;
            }
            //else
            //{
            //    backupStatus.Text = "Looking for existing backups";

            //    // try to get the id of the existing database backup
            //    opRes = await client.GetAsync(strSkyDriveFolderID + "/files");
            //    result = opRes.Result;

            //    List<object> files = (List<object>)result.data;

            //    foreach (dynamic item in files)
            //    {
            //        if (item.name == strSkyDriveDBName)
            //        {
            //            fileID = item.id;
            //        }
            //    }
            //}

            UploadFiles();
        }

        private async void UploadFiles()
        {
            string fileName;

            backupStatus.Text = "Uploading files";
                        
            // The folder must exist, it should have already been created. 
            if (strSkyDriveFolderID != string.Empty)
            {
                try
                {
                    using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        int numFiles = iso.GetFileNames().Length;
                        int count = 0;

                        // Upload many files. 
                        foreach (string itemName in iso.GetFileNames())
                        {
                            fileName = itemName;

                            count++;

                            backupStatus.Text = "Uploading " + count + " of " + numFiles + " files";

                            if (fileName == strSkyDriveDBName)
                            {
                                // Close the database.
                                App.ViewModel.FishingSpotsDB.Dispose();
                                App.ViewModel.FishingSpotsDB = null;
                            }

                            readStream = iso.OpenFile(fileName, FileMode.Open, FileAccess.ReadWrite);
                            LiveOperationResult opRes = await client.UploadAsync(strSkyDriveFolderID, fileName, readStream, OverwriteOption.Overwrite);
                            readStream.Close();

                            if (fileName == strSkyDriveDBName)
                            {
                                // Create the ViewModel object.
                                App.ViewModel.FishingSpotsDB = new FishingSpotsDataContext(App.DBConnectionString);

                                // The old instance of FishLocation uses a compiled query that
                                // still is compiled against the old DB instance. Refresh it.
                                App.FLocation = new FishLocation();

                                // Query the local database and load observable collections.
                                App.ViewModel.LoadCollectionsFromDatabase();
                            }                          
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error accessing IsolatedStorage. Please close the app and re-open it, and then try backing up again.", "Backup Failed", MessageBoxButton.OK);
                } 
            }

            backupStatus.Text = String.Empty;
            pgBackup.Visibility = System.Windows.Visibility.Collapsed;
            backupInProgress = false;

            ProgressStack.Visibility = System.Windows.Visibility.Collapsed;
            SettingsStack.Visibility = System.Windows.Visibility.Visible;
            this.ApplicationBar.IsVisible = true;
        }

        private async void UploadOneFile(string fileName, IsolatedStorageFile iso)
        {

            //readStream = iso.OpenFile(fileName, FileMode.Open, FileAccess.ReadWrite);
            await client.BackgroundUploadAsync(strSkyDriveFolderID, new Uri(fileName, UriKind.Relative), OverwriteOption.Overwrite);
            //readStream.Close();

        }




        ///// <summary>
        ///// The IsolatedStorageData folder have been created.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void CreateFolder_Completed(object sender, LiveOperationCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        // Write message to the UI thread.
        //        UpdateUIThread(tbDebug, "Ready to backup.");
        //        UpdateUIThread(tbDate, "No previous backup available.");


        //        Dictionary<string, object> folder = (Dictionary<string, object>)e.Result;
        //        // Get the folder ID.
        //        strSkyDriveFolderID = folder["id"].ToString();
        //        btnBackup.IsEnabled = true;
        //    }
        //    else
        //    {
        //        MessageBox.Show(e.Error.Message);
        //    }
        //}


        ///// <summary>
        ///// Upload Files.
        ///// </summary>
        //public void UploadFile()
        //{
        //    // The folder must exist, it should have already been created.
        //    if (strSkyDriveFolderID != string.Empty)
        //    {
        //        this.client.UploadCompleted
        //            += new EventHandler<LiveOperationCompletedEventArgs>(IsFile_UploadCompleted);


        //        // Write message to the UI thread.
        //        UpdateUIThread(tbDebug, "Uploading backup...");
        //        UpdateUIThread(tbDate, "");


        //        try
        //        {
        //            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
        //            {
        //                // Upload many files.
        //                foreach (string itemName in iso.GetFileNames())
        //                {
        //                    fileName = itemName;
        //                    readStream = iso.OpenFile(fileName, FileMode.Open, FileAccess.Read);
        //                    client.UploadAsync(strSkyDriveFolderID, fileName, readStream, OverwriteOption.Overwrite, null);
        //                }


        //                // [-or-]


        //                // Upload one file.
        //                //readStream = iso.OpenFile(fileName, FileMode.Open, FileAccess.Read);
        //                //client.UploadAsync(strSkyDriveFolderID, fileName, readStream, OverwriteOption.Overwrite, null);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Error accessing IsolatedStorage. Please close the app and re-open it, and then try backing up again!", "Backup Failed", MessageBoxButton.OK);


        //            // Write message to the UI thread.
        //            UpdateUIThread(tbDebug, ex.Message + ".Close the app and start again.");
        //            UpdateUIThread(tbDate, "");
        //        }
        //    }
        //}


        ///// <summary>
        ///// Check if the backup have finished.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="args"></param>
        //private void IsFile_UploadCompleted(object sender, LiveOperationCompletedEventArgs args)
        //{
        //    if (args.Error == null)
        //    {

        //        // In order to prevent the crash when user clicks the backup button again, dispose the readStream.
        //        readStream.Dispose();
        //        // Write message to the UI thread.



        //        // Get the newly created fileID's (it will update the time too, and enable restoring).
        //        client = new LiveConnectClient(session);
        //        //client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(getFiles_GetCompleted);
        //        client.GetAsync(strSkyDriveFolderID + "/files");
        //    }
        //    else
        //    {
        //        // Write message to the UI thread.

        //    }
        //}

        // Bad things happen if we navigate to other pages during backup.
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (backupInProgress == true)
            {
                MessageBox.Show("Operation in progress. Please wait.", "Backup and restore", MessageBoxButton.OK);
                e.Cancel = true;
            }           
        }

        private void rateMeAppBarButton_Click(object sender, EventArgs e)
        {
            mrt = new MarketplaceReviewTask();

            mrt.Show();
        }

        private async void Backup2()
        {
            backupStatus.Text = "Looking for folder";
            pgBackup.Visibility = System.Windows.Visibility.Visible;

            // Get the root folder
            LiveOperationResult opRes = await client.GetAsync("me/skydrive");
            dynamic result = opRes.Result;
            string sdRoot = result.id;
            int sdCount = result.count;

            opRes = await client.GetAsync(sdRoot + "/files");
            result = opRes.Result;

            List<object> folders = (List<object>)result.data;

            // Loop all folders to check if the isolatedstoragefolder exists. 
            foreach (dynamic item in folders)
            {
                if (item.name == strSkyDriveFolderName)
                {
                    strSkyDriveFolderID = item.id;
                    backupStatus.Text = "Found folder";
                }
            }

            // If the IsolatedStorageFolder does not exist, create it. 
            if (strSkyDriveFolderID == string.Empty)
            {
                backupStatus.Text = "Creating folder";

                Dictionary<string, object> skyDriveFolderData = new Dictionary<string, object>();
                skyDriveFolderData.Add("name", strSkyDriveFolderName);

                opRes = await client.PostAsync("me/skydrive", skyDriveFolderData);
                result = opRes.Result;

                strSkyDriveFolderID = result.id;
            }
            //else
            //{
            //    backupStatus.Text = "Looking for existing backups";

            //    // try to get the id of the existing database backup
            //    opRes = await client.GetAsync(strSkyDriveFolderID + "/files");
            //    result = opRes.Result;

            //    List<object> files = (List<object>)result.data;

            //    foreach (dynamic item in files)
            //    {
            //        if (item.name == strSkyDriveDBName)
            //        {
            //            fileID = item.id;
            //        }
            //    }
            //}

            UploadFiles2();
        }

        private void UploadFiles2()
        {
            string fileName;

            backupStatus.Text = "Uploading files";

            // The folder must exist, it should have already been created. 
            if (strSkyDriveFolderID != string.Empty)
            {
                try
                {
                    using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        int numFiles = iso.GetFileNames().Length;
                        int count = 0;

                        // Upload many files. 
                        foreach (string itemName in iso.GetFileNames())
                        {
                            fileName = "/shared/transfers/" + itemName;

                            count++;

                            backupStatus.Text = "Uploading " + count + " of " + numFiles + " files";

                            if (itemName == strSkyDriveDBName)
                            {
                                // Close the database.
                                App.ViewModel.FishingSpotsDB.Dispose();
                                App.ViewModel.FishingSpotsDB = null;
                            }

                            // Copy the file to the transfers folder
                            using (IsolatedStorageFileStream stream = iso.OpenFile(itemName, FileMode.Open))
                            {
                                byte[] bytes = new byte[stream.Length];

                                stream.Read(bytes, 0, bytes.Length);

                                using (FileStream outStream = iso.OpenFile(fileName, FileMode.OpenOrCreate))
                                {
                                    outStream.Write(bytes, 0, bytes.Length);
                                }

                            }

   
                            if (fileName == strSkyDriveDBName)
                            {
                                // Create the ViewModel object.
                                App.ViewModel.FishingSpotsDB = new FishingSpotsDataContext(App.DBConnectionString);

                                // The old instance of FishLocation uses a compiled query that
                                // still is compiled against the old DB instance. Refresh it.
                                App.FLocation = new FishLocation();

                                // Query the local database and load observable collections.
                                App.ViewModel.LoadCollectionsFromDatabase();
                            }


                            //CancellationToken ct = new CancellationToken();
                            //UploadProgress up = new UploadProgress();

                            //readStream = iso.OpenFile(fileName, FileMode.Open, FileAccess.ReadWrite);

                            UploadOneFile(fileName, iso);                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error accessing IsolatedStorage. Please close the app and re-open it, and then try backing up again.", "Backup Failed", MessageBoxButton.OK);
                }
            }

            backupStatus.Text = String.Empty;
            pgBackup.Visibility = System.Windows.Visibility.Collapsed;
            backupInProgress = false;

            ProgressStack.Visibility = System.Windows.Visibility.Collapsed;
            SettingsStack.Visibility = System.Windows.Visibility.Visible;
            this.ApplicationBar.IsVisible = true;
        }

        
    }
}