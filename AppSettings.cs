using System;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Collections.Generic;


namespace FishingSpots
{
    public class AppSettings
    {
        // Our settings
        IsolatedStorageSettings settings;

        // The key names of our settings
        const string LocServicesKeyName = "LocationConsent";
        const string TrackingKeyName = "Tracking";
        const string DefaultImageKeyName = "DefaultImage";
       

        // The default value of our settings
        const bool LocServicesDefault = false;
        const bool TrackingDefault = true;
        const string DefaultImageDefault = "";

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public AppSettings()
        {
            // Get the settings for this application.
            settings = IsolatedStorageSettings.ApplicationSettings;
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        void Save()
        {
            settings.Save();
        }

        /// <summary>
        /// Property to get and set a CheckBox Setting Key.
        /// </summary>
        public bool LocationConsent
        {
            get
            {
                return GetValueOrDefault<bool>(LocServicesKeyName, LocServicesDefault);
            }
            set
            {
                if (AddOrUpdateValue(LocServicesKeyName, value))
                {
                    Save();
                }
            }
        }

        /// <summary>
        /// Property to get and set a CheckBox Setting Key.
        /// </summary>
        public bool TrackingSetting
        {
            get
            {
                return GetValueOrDefault<bool>(TrackingKeyName, TrackingDefault);
            }
            set
            {
                if (AddOrUpdateValue(TrackingKeyName, value))
                {
                    Save();
                }
            }
        }

        public string DefaultImage
        {
            get
            {
                return GetValueOrDefault<string>(DefaultImageKeyName, DefaultImageDefault);
            }

            set
            {
                if (AddOrUpdateValue(DefaultImageKeyName, value))
                {
                    Save();
                }
            }
        }




       

    }
}
