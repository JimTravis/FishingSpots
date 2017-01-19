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
using Windows.Storage;

namespace FishingSpots
{
    public class BackupMan
    {

        private LiveConnectClient client = null;
        private LiveAuthClient authClient = null;
        private LiveConnectSession session = null;
        private string strSkyDriveFolderName = "FishingSpotsAppBackup"; // The folder name for backups
        private string strSkyDriveFolderID = string.Empty;              // The id of the folder name for backups
        private IsolatedStorageFileStream readStream = null;            // The stream for restoring data 
        private string strSkyDriveDBName = "FishingSpots.sdf";          // The name of the database sdf file
        private Boolean backupInProgress = false;
        

    //    public void Backup()
    //    {
    //        CopyFiles();
    //    }

    //    private async void CopyFiles()
    //    {
    //        string fileName;

    //        try
    //        {
    //            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
    //            {
    //                int numFiles = iso.GetFileNames().Length;
    //                int count = 0;

    //                // Get the local folder.
    //                StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;

    //                // Create a new folder name DataFolder.
    //                var dataFolder = await local.CreateFolderAsync("Backups",
    //                    CreationCollisionOption.OpenIfExists);


    //                // Upload many files. 
    //                foreach (string itemName in iso.GetFileNames())
    //                {
    //                    fileName = "/Backups" + itemName;

    //                    count++;

    //                    if (itemName == strSkyDriveDBName)
    //                    {
    //                        // Close the database.
    //                        App.ViewModel.FishingSpotsDB.Dispose();
    //                        App.ViewModel.FishingSpotsDB = null;
    //                    }

    //                    // Copy the file to the transfers folder
    //                    using (IsolatedStorageFileStream stream = iso.OpenFile(itemName, FileMode.Open))
    //                    {
    //                        byte[] bytes = new byte[stream.Length];

    //                        stream.Read(bytes, 0, bytes.Length);

    //                        using (FileStream outStream = iso.OpenFile(fileName, FileMode.OpenOrCreate))
    //                        {
    //                            outStream.Write(bytes, 0, bytes.Length);
    //                        }

    //                    }

    //                    if (fileName == strSkyDriveDBName) // reinitialize
    //                    {
    //                        // Create the ViewModel object.
    //                        App.ViewModel.FishingSpotsDB = new FishingSpotsDataContext(App.DBConnectionString);

    //                        // The old instance of FishLocation uses a compiled query that
    //                        // still is compiled against the old DB instance. Refresh it.
    //                        App.FLocation = new FishLocation();

    //                        // Query the local database and load observable collections.
    //                        App.ViewModel.LoadCollectionsFromDatabase();
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show("Error accessing IsolatedStorage. Please close the app and re-open it, and then try backing up again.", "Backup Failed", MessageBoxButton.OK);
    //        }
    //    }

    //    private async void GetOrCreateFolderAsync()
    //    {
    //        // Get the root folder
    //        LiveOperationResult opRes = await client.GetAsync("me/skydrive");
    //        dynamic result = opRes.Result;
    //        string sdRoot = result.id;
    //        int sdCount = result.count;

    //        opRes = await client.GetAsync(sdRoot + "/files");
    //        result = opRes.Result;

    //        List<object> folders = (List<object>)result.data;

    //        // Loop all folders to check if the isolatedstoragefolder exists. 
    //        foreach (dynamic item in folders)
    //        {
    //            if (item.name == strSkyDriveFolderName)
    //            {
    //                strSkyDriveFolderID = item.id;
                   
    //            }
    //        }

    //        // If the IsolatedStorageFolder does not exist, create it. 
    //        if (strSkyDriveFolderID == string.Empty)
    //        {    

    //            Dictionary<string, object> skyDriveFolderData = new Dictionary<string, object>();
    //            skyDriveFolderData.Add("name", strSkyDriveFolderName);

    //            opRes = await client.PostAsync("me/skydrive", skyDriveFolderData);
    //            result = opRes.Result;

    //            strSkyDriveFolderID = result.id;
    //        }
    //    }

        
           
    }
}
