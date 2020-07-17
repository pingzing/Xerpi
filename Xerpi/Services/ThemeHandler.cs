using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xerpi.Messages;
using Xerpi.Theme;

namespace Xerpi.Services
{
    public class ThemeHandler
    {
        private readonly IMessagingCenter _messagingService;
        private readonly ISettingsService _settingsService;

        public ThemeHandler(IMessagingCenter messagingService,
            ISettingsService settingsService)
        {
            _messagingService = messagingService;
            _settingsService = settingsService;
            _messagingService.Subscribe<object>(this, SimpleMessages.SystemThemeChanged, SystemThemeChanged);
            _messagingService.Subscribe<SettingsService, AppThemeChangedMessage>(this, "", UserThemeChanged);
        }

        public void Initialize()
        {
            if (_settingsService.SelectedTheme == AppTheme.Unspecified)
            {
                UseSystemTheme();
            }
            else
            {
                SwitchToTheme(_settingsService.SelectedTheme);
            }
        }

        private void SystemThemeChanged(object sender)
        {
            // Only do stuff if the user wants to use the System theme
            if (_settingsService.SelectedTheme != AppTheme.Unspecified)
            {
                return;
            }

            UseSystemTheme();
        }

        private void UserThemeChanged(ISettingsService sender, AppThemeChangedMessage message)
        {
            if (message.NewTheme == message.OldTheme)
            {
                return;
            }
            else if (message.NewTheme == AppTheme.Unspecified)
            {
                UseSystemTheme();
            }
            else if (message.NewTheme == AppTheme.Dark)
            {
                SwitchToTheme(AppTheme.Dark);
            }
            else if (message.NewTheme == AppTheme.Light)
            {
                SwitchToTheme(AppTheme.Light);
            }
        }

        private void UseSystemTheme()
        {
            if (AppInfo.RequestedTheme == AppTheme.Unspecified
                || AppInfo.RequestedTheme == AppTheme.Light)
            {
                SwitchToTheme(AppTheme.Light);
            }
            else if (AppInfo.RequestedTheme == AppTheme.Dark)
            {
                SwitchToTheme(AppTheme.Dark);
            }
        }

        private void SwitchToTheme(AppTheme theme)
        {
            if (theme == AppTheme.Unspecified)
            {
                Debug.WriteLine($"Invalid selection. Final switching of theme must always be either Dark or Light.");
                return;
            }

            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries == null)
            {
                Debug.WriteLine($"No merged dictionaries found!");
                return;
            }

            ResourceDictionary currentThemeDict = GetCurrentThemeDictionary(mergedDictionaries);
            if (currentThemeDict == null)
            {
                Debug.WriteLine($"Current theme dict not found!");
                return;
            }

            AppTheme activeTheme = GetCurrentDictTheme(currentThemeDict);
            if (activeTheme == theme)
            {
                // We're already set to the theme we want, bail out
                return;
            }

            // Remove current theme
            Application.Current.Resources.MergedDictionaries.Remove(currentThemeDict);

            // Create and add in the relevant theme
            if (theme == AppTheme.Dark)
            {
                Application.Current.Resources.MergedDictionaries.Add(new DarkTheme());
            }
            if (theme == AppTheme.Light)
            {
                Application.Current.Resources.MergedDictionaries.Add(new LightTheme());
            }
        }


        private ResourceDictionary GetCurrentThemeDictionary(ICollection<ResourceDictionary> mergedDictionaries)
        {
            ResourceDictionary dictionary;
            dictionary = mergedDictionaries.SingleOrDefault(x => x.ContainsKey("ThemeName"));
            if (dictionary == null)
            {
                // Try searching for a dict with source URL--if a MergedDictionary is declared in XAML, it will appear to have no Keys or
                // Values, but its source property will not be null.
                // The reverse is true if we assign one at runtime; it will haves keys and values, but no Source.
                dictionary = mergedDictionaries.SingleOrDefault(x => x.Source.OriginalString.Contains("Theme/"));
            }

            return dictionary;
        }
        private AppTheme GetCurrentDictTheme(ResourceDictionary currentThemeDict)
        {
            // Check for a Source--only XAML-added themes have this.
            if (currentThemeDict.Source != null)
            {
                if (currentThemeDict.Source.OriginalString.Contains("DarkTheme.xaml"))
                {
                    return AppTheme.Dark;
                }
                if (currentThemeDict.Source.OriginalString.Contains("LightTheme.xaml"))
                {
                    return AppTheme.Light;
                }
                throw new ArgumentException("currentThemeDict.Source is not null, but doesn't contain a LightTheme.xaml or a DarkTheme.xaml");
            }
            // Runtime-added themes have no source, but give us access to the Dictionary's keys and values
            else
            {
                if (currentThemeDict.TryGetValue("ThemeName", out object themeName))
                {
                    return (string)themeName switch
                    {
                        "Dark" => AppTheme.Dark,
                        "Light" => AppTheme.Light,
                        _ => throw new ArgumentException($"currentThemeDict has a ThemeName key, but an unrecognized theme name: {themeName}"),
                    };
                }
                throw new ArgumentException($"currentThemeDict has a ThemeName key, but an unrecognized theme name: {themeName}");
            }
        }
    }
}
