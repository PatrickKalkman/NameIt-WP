using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Windows.Controls;

using Caliburn.Micro;
using Caliburn.Micro.BindableAppBar;

using Microsoft.Phone.Controls;

using NameIt.Common;
using NameIt.Nokia;
using NameIt.ViewModels;

using Nokia.Music;

using Telerik.Windows.Controls;

namespace NameIt
{
    public class Bootstrapper : PhoneBootstrapper
    {
        private const string SettingCountryCode = "countrycode";

        private PhoneContainer container;

        private RadDiagnostics diagnostics;
        
        public Bootstrapper()
        {
            LogManager.GetLog = type => new DebugLogger(type);
        }

        void diagnostics_ExceptionOccurred(object sender, ExceptionOccurredEventArgs e)
        { 
            GoogleAnalytics.EasyTracker.GetTracker().SendException(e.Exception.Message, true);
        }

        protected override PhoneApplicationFrame CreatePhoneApplicationFrame()
        {
            return new TransitionFrame();
        }

        protected override void Configure()
        {
            container = new PhoneContainer();

            container.RegisterPhoneServices(RootFrame);

            container.PerRequest<BackgroundImageBrush>();
            container.PerRequest<FlipTileCreator>();
            container.PerRequest<PrivacyPageViewModel>();
            container.PerRequest<MainPageViewModel>();
            container.PerRequest<BackgroundImageRotator>();
            container.PerRequest<GenreRetriever>();
            container.PerRequest<RandomSongListSelector>();
            container.PerRequest<GamePageViewModel>();
            container.PerRequest<CachingService>();
            container.PerRequest<SettingsHelper>();
            container.PerRequest<NameItSettingsManager>();
            container.PerRequest<HelpPageViewModel>();
            container.PerRequest<SettingsPageViewModel>();
            container.PerRequest<AboutViewModel>();
            container.PerRequest<LeaderboardManager>();

            container.RegisterSingleton(typeof(LeaderboardManager), "", typeof(LeaderboardManager));
            container.RegisterSingleton(typeof(ILog), "", typeof(DebugLogger));

            AddCustomConventions();

            StartMusicClient();

            //Creates an instance of the Diagnostics component.
            diagnostics = new RadDiagnostics();
            diagnostics.EmailTo = "pkalkie@gmail.com";
            diagnostics.ExceptionOccurred += diagnostics_ExceptionOccurred;
            diagnostics.Init();

            ApplicationUsageHelper.Init("1.0");
        }

        private async void StartMusicClient()
        {
            string countryCode = GetSettingsCountryCode();
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName.ToLower();
                var resolver = new CountryResolver(ApiKeys.ClientId);
                bool available = await resolver.CheckAvailabilityAsync(countryCode);
                if (available)
                {
                    SaveCountryCode(countryCode);
                }
            }
            MusicClient client = CreateApiClient(countryCode);
            container.RegisterInstance(typeof(MusicClient), null, client);
        }

        private MusicClient CreateApiClient(string countryCode)
        {
            if (!string.IsNullOrEmpty(countryCode))
            {
                return new MusicClient(ApiKeys.ClientId, countryCode);
            }
            return null;
        }

        public string GetSettingsCountryCode()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(SettingCountryCode))
            {
                return IsolatedStorageSettings.ApplicationSettings[SettingCountryCode] as string;
            }
            return null;
        }

        public void SaveCountryCode(string countryCode)
        {
            IsolatedStorageSettings.ApplicationSettings[SettingCountryCode] = countryCode;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        static void AddCustomConventions()
        {
            ConventionManager.AddElementConvention<BindableAppBarButton>(
                Control.IsEnabledProperty, "DataContext", "Click");
            ConventionManager.AddElementConvention<BindableAppBarMenuItem>(
                Control.IsEnabledProperty, "DataContext", "Click");
        }

        protected override void OnActivate(object sender, Microsoft.Phone.Shell.ActivatedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Bootstrapper", "Activate", null, 0);
            base.OnActivate(sender, e);
        }

        protected override void OnLaunch(object sender, Microsoft.Phone.Shell.LaunchingEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Bootstrapper", "Launch", null, 0);
            base.OnLaunch(sender, e);
        }

        protected override void OnDeactivate(object sender, Microsoft.Phone.Shell.DeactivatedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Bootstrapper", "Deactivate", null, 0);
            base.OnDeactivate(sender, e);
        }

        protected override void OnClose(object sender, Microsoft.Phone.Shell.ClosingEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Bootstrapper", "Close", null, 0);
            base.OnClose(sender, e);
        }

        protected override void OnUnhandledException(object sender, System.Windows.ApplicationUnhandledExceptionEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendException(e.ExceptionObject.Message, false);
            base.OnUnhandledException(sender, e);
        }
        
        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}