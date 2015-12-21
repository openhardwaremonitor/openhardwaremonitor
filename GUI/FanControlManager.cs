using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using OpenHardwareMonitor.Collections;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.GUI
{
    /// <summary>
    /// Provides CRUD operations on fan control profiles.
    /// </summary>
    public class FanControlManager
    {
        private readonly PersistentSettings settings;

        private readonly IEnumerable<ISensor> fanControls;

        private readonly ListSet<string> availableProfiles;

        private const string AllProfiles = "AllFanProfiles";

        private const string LoadedFanProfile = "LoadedFanProfile";

        private const string ProfileDelimiter = ";";

        public FanControlManager(PersistentSettings settings, IEnumerable<ISensor> fanControls)
        {
            this.settings = settings;
            this.fanControls = fanControls;
            this.availableProfiles = FindAllProfiles();
            var lastProfile = FindLastLoadedProfile();
            if (lastProfile != null)
            {
                LoadProfile(lastProfile);
            }
        }

        /// <summary>
        /// Returns value of LoadedFanProfile entry or null if none was found.
        /// </summary>
        private string FindLastLoadedProfile()
        {
            return settings.GetValue(LoadedFanProfile, null);
        }

        /// <summary>
        /// List of all profile names.
        /// </summary>
        public IEnumerable<string> AvailableProfiles
        {
            get
            {
                return availableProfiles;
            }
        }

        /// <summary>
        /// Name of the loaded profile. Null if no profile is loaded.
        /// </summary>
        public string LoadedProfile { get; private set; }

        /// <summary>
        /// Returns profile names of all profiles found in the settings.
        /// </summary>
        private ListSet<string> FindAllProfiles()
        {
            var allProfileNames = settings.GetValue(AllProfiles, string.Empty);
            var explodedProfiles = allProfileNames.Split(new []{ProfileDelimiter}, StringSplitOptions.RemoveEmptyEntries);

            var profileSet = new ListSet<string>();
            foreach (var profile in explodedProfiles)
            {
                profileSet.Add(profile);
            }

            return profileSet;
        }

        /// <summary>
        /// Creates a new fan control profile with the currently active fan control values.
        /// If a profile with the same name already exists, it will be overwritten.
        /// The new profile will be part of the persistent settings.
        /// </summary>
        /// <param name="profileName">Name of the profile.</param>
        /// <exception cref="ArgumentException">If invalid profile name.</exception>
        public void SaveCurrentSettingsAsProfile(string profileName)
        {
            CheckProfileName(profileName);

            availableProfiles.Add(profileName);
            SaveAllProfileNames();
            SaveProfileSettings(profileName);
        }

        /// <exception cref="ArgumentException">If profile name contains delimiter.</exception>
        private void CheckProfileName(string profileName)
        {
            if (string.IsNullOrEmpty(profileName) || profileName.Contains(ProfileDelimiter))
            {
                throw new ArgumentException("profile name invalid");
            }
        }

        /// <summary>
        /// Saves all settings of all fan controls under given profile.
        /// </summary>
        private void SaveProfileSettings(string profileName)
        {
            foreach (var fanControl in fanControls)
            {
                if (fanControl.Value.HasValue)
                {
                    var settingKey = ValueSettingKey(profileName, fanControl);
                    settings.SetValue(settingKey, fanControl.Value.Value);
                }
            }
        }

        /// <summary>
        /// Saves all profile names in persistent settings.
        /// </summary>
        private void SaveAllProfileNames()
        {
            var allProfiles = string.Join(ProfileDelimiter.ToString(), availableProfiles.ToArray());
            settings.SetValue(AllProfiles, allProfiles);
        }

        /// <summary>
        /// Loads fan control profile with given name and applies the settings.
        /// Also sets the LoadedProfile property and the associated setting.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if profile name is invalid or no profile is found under the name.</exception>
        public void LoadProfile(string profileName)
        {
            CheckProfileName(profileName);

            foreach (var fanControl in fanControls)
            {
                var valueSettingKey = ValueSettingKey(profileName, fanControl);

                var fanValue = settings.GetValue(valueSettingKey, -1.0f);
                if (fanValue != -1)
                {
                    fanControl.Control.SetSoftware(fanValue);
                }
            }

            LoadedProfile = profileName;
            settings.SetValue(LoadedFanProfile, LoadedProfile);
        }

        /// <summary>
        /// Returns key of value setting of a given fan.
        /// </summary>
        /// <example>profileName = "my profile" and fan = gpu will return "my profile.gpu".</example>
        private string ValueSettingKey(string profileName, ISensor fan)
        {
            return profileName + "." + fan.Name;
        }

        /// <summary>
        /// Deletes a profile from settings. Does not change any fan settings. 
        /// If profileName == LoadedProfile, LoadedProfile will be null afterwards.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if profile name is invalid or no profile is found under the name.</exception>
        public void DeleteProfile(string profileName)
        {
            CheckProfileName(profileName);

            if (!availableProfiles.Contains(profileName))
            {
                throw new ArgumentException("no profile found");
            }

            if (profileName == LoadedProfile)
            {
                LoadedProfile = null;
            }

            availableProfiles.Remove(profileName);
            SaveAllProfileNames();

            foreach (var fanControl in fanControls)
            {
                var valueSettingKey = ValueSettingKey(profileName, fanControl);
                settings.Remove(valueSettingKey);
            }
        }

        /// <summary>
        /// Overwrites the settings of the loaded profile with the current settings.
        /// </summary>
        /// <exception cref="InvalidOperationException">If no profile is currently loaded.</exception>
        public void UpdateLoadedProfileWithCurrentSettings()
        {
            if (LoadedProfile == null)
            {
                throw new InvalidOperationException("no profile loaded");
            }

            SaveCurrentSettingsAsProfile(LoadedProfile);
        }
    }
}