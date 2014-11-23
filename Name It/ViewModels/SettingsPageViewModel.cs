using System.Windows;

using Caliburn.Micro;

using NameIt.Common;
using NameIt.Nokia;

namespace NameIt.ViewModels
{
    public class SettingsPageViewModel : NameItViewModel
    {
        private readonly NameItSettingsManager settingsManager;
        private readonly LeaderboardManager leaderboardManager;
        private readonly GenreRetriever genreRetriever;

        public SettingsPageViewModel(BackgroundImageBrush backgroundImageBrush, INavigationService navigationService, ILog logger, NameItSettingsManager settingsManager, LeaderboardManager leaderboardManager, GenreRetriever genreRetriever) : base(backgroundImageBrush, navigationService, logger)
        {
            this.settingsManager = settingsManager;
            this.leaderboardManager = leaderboardManager;
            this.genreRetriever = genreRetriever;
        }

        public bool UseLearningMode
        {
            get { return settingsManager.UseLearningMode; }
            set 
            {
                settingsManager.UseLearningMode = value;
                NotifyOfPropertyChange(() => UseLearningMode);
            }
        }

        public bool UseVoiceRecognition
        {
            get { return settingsManager.UseVoiceRecognition; }
            set
            {
                settingsManager.UseVoiceRecognition = value;
                NotifyOfPropertyChange(() => UseVoiceRecognition);
            }
        }

        public void ClearLeaderBoard()
        {
            leaderboardManager.Clear();
            MessageBox.Show("The leaderboard has been reset");
        }

        public void ClearCache()
        {
            genreRetriever.ClearCache();
            MessageBox.Show("The cache has been clearerd");
        }
    }
}
