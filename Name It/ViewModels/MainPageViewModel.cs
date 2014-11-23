using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using Windows.Networking.Connectivity;

using Caliburn.Micro;

using NameIt.Common;
using NameIt.Nokia;
using NameIt.Views;

using Nokia.Music.Tasks;
using Nokia.Music.Types;

using Telerik.Windows.Data;

namespace NameIt.ViewModels
{
    public class MainPageViewModel : NameItViewModel
    {
        private readonly BackgroundImageRotator backgroundImageRotator;
        private readonly GenreRetriever genreRetriever;
        private readonly LeaderboardManager leaderboardManager;
        private readonly FlipTileCreator flipTileCreator;
        private readonly DispatcherTimer backgroundChangeTimer = new DispatcherTimer();
        private readonly DispatcherTimer refreshGenresTimer = new DispatcherTimer();

        private ImageBrush panoramaBackground;

        public MainPageViewModel(
            INavigationService navigationService, 
            BackgroundImageRotator backgroundImageRotator, 
            BackgroundImageBrush backgroundImageBrush, 
            ILog logger, 
            GenreRetriever genreRetriever, 
            LeaderboardManager leaderboardManager,
            FlipTileCreator flipTileCreator)
            : base(backgroundImageBrush, navigationService, logger)
        {
            this.backgroundImageRotator = backgroundImageRotator;
            this.genreRetriever = genreRetriever;
            this.leaderboardManager = leaderboardManager;
            this.flipTileCreator = flipTileCreator;

            StartButtonText = "start";
            AboutButtonText = "about";
            PrivacyButtonText = "privacy";
            LoadingText = "Loading genres......";
            RotatePanoramaBackground();
            InitializeAndStartTimer();
        }

        private MainPage view;

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            this.view = view as MainPage;

            var genreDescriptor = new GenericGroupDescriptor<GenreScoreHistory, string>(s => s.GenreId);
            genreDescriptor.SortMode = ListSortMode.Ascending;
            this.view.Leaderboard.GroupDescriptors.Add(genreDescriptor);
            var genreSortDescriptor = new GenericSortDescriptor<GenreScoreHistory, int>(gs => gs.Score);
            genreSortDescriptor.SortMode = ListSortMode.Descending;
            this.view.Leaderboard.SortDescriptors.Add(genreSortDescriptor);
            Genres = new ObservableCollection<Genre>(await genreRetriever.GetAll());
            IsBusy = false;
            NetworkInformation.NetworkStatusChanged += NetworkInformationOnNetworkStatusChanged;
        }

        private async void NetworkInformationOnNetworkStatusChanged(object sender)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                LoadingText = "No network connection";
            }
            else
            {
                LoadingText = "Loading genres......";
                Genres = new ObservableCollection<Genre>(await genreRetriever.GetAll());
            }
        }

        protected async override void OnActivate()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show(
                    "Name It needs a internet connection to function properly, make sure that you are connected through a wifi or other data connection");
            }

            CreateLiveTile();
            base.OnActivate();
            SelectedGenre = null;
            Scores = new ObservableCollection<GenreScoreHistory>(await leaderboardManager.GetScores());
        }

        private async void CreateLiveTile()
        {
            string content;
            GenreScoreHistory highScore = await leaderboardManager.GetHighScore();
            if (highScore != null)
            {
                content = string.Format("High score for {0} is {1}", highScore.GenreId, highScore.Score);
            }
            else
            {
                content = string.Format("No high scores yet :(");
            }
            flipTileCreator.UpdateDefaultTile(content, content);
        }

        public void StartNokiaMixRadio()
        {
            var task = new LaunchTask();
            task.Show();
        }

        private ObservableCollection<Genre> genres;

        public ObservableCollection<Genre> Genres
        {
            get { return genres; }
            set
            {
                genres = value;
                NotifyOfPropertyChange(() => Genres);
            }
        }

        private Genre selectedGenre;

        public Genre SelectedGenre
        {
            get { return selectedGenre; }
            set
            {
                selectedGenre = value;
                NotifyOfPropertyChange(() => SelectedGenre);
                if (value != null)
                {
                    var uri = navigationService.UriFor<GamePageViewModel>().WithParam(g => g.GenreId, selectedGenre.Id).BuildUri();
                    navigationService.Navigate(uri);
                }
            }
        }

        private string loadingText;

        public string LoadingText
        {
            get { return loadingText; }
            set
            {
                loadingText = value;
                NotifyOfPropertyChange(() => LoadingText);
            }
        }


        private void InitializeAndStartTimer()
        {
            this.backgroundChangeTimer.Interval = TimeSpan.FromSeconds(10);
            this.backgroundChangeTimer.Tick += BackgroundChangeTimer_Tick;
            this.backgroundChangeTimer.Start();

            this.refreshGenresTimer.Interval = TimeSpan.FromSeconds(1);
            this.refreshGenresTimer.Tick += RefreshGenresTimerOnTick;
            this.refreshGenresTimer.Start();
        }

        private bool refreshBusy = false;

        private async void RefreshGenresTimerOnTick(object sender, EventArgs eventArgs)
        {
            if (!refreshBusy)
            {
                refreshBusy = true;
                if (Genres == null || Genres.Count == 0)
                {
                    Genres = new ObservableCollection<Genre>(await genreRetriever.GetAll());
                }
                else
                {
                    this.refreshGenresTimer.Stop();
                }
                refreshBusy = false;
            }
        }

        private void BackgroundChangeTimer_Tick(object sender, EventArgs e)
        {
            RotatePanoramaBackground();
        }

        public ImageBrush PanoramaBackground
        {
            get
            {
                return panoramaBackground;
            }

            set
            {
                panoramaBackground = value;
                NotifyOfPropertyChange(() => PanoramaBackground);
            }
        }

        private void RotatePanoramaBackground()
        {
            PanoramaBackground = backgroundImageRotator.Rotate();
        }

        public void Settings()
        {
            var uri = navigationService.UriFor<SettingsPageViewModel>().BuildUri();
            navigationService.Navigate(uri);
        }

        public void Privacy()
        {
            var uri = navigationService.UriFor<PrivacyPageViewModel>().BuildUri();
            navigationService.Navigate(uri);
        }

        public void Help()
        {
            var uri = navigationService.UriFor<HelpPageViewModel>().BuildUri();
            navigationService.Navigate(uri);
        }

        public void About()
        {
            var uri = navigationService.UriFor<AboutViewModel>().BuildUri();
            navigationService.Navigate(uri);
        }

        private List<GenericGroupDescriptor<GenreScoreHistory, string>> groupedGenres;

        public List<GenericGroupDescriptor<GenreScoreHistory, string>> GroupedGenres
        {
            get { return groupedGenres; }
            set
            {
                groupedGenres = value;
                NotifyOfPropertyChange(() => GroupedGenres);
            }
        }

        private ObservableCollection<GenreScoreHistory> scores = new ObservableCollection<GenreScoreHistory>();

        public ObservableCollection<GenreScoreHistory> Scores
        {
            get
            {
                return scores; 
            }
            set
            {
                scores = value;
                NotifyOfPropertyChange(() => Scores);
            }
        }

        private string aboutButtonText;

        public string AboutButtonText
        {
            get { return aboutButtonText; }
            set
            {
                aboutButtonText = value;
                NotifyOfPropertyChange(() => AboutButtonText);
            }
        }

        private string privacyButtonText;

        public string PrivacyButtonText
        {
            get { return privacyButtonText; }
            set
            {
                privacyButtonText = value;
                NotifyOfPropertyChange(() => PrivacyButtonText);
            }
        }

        private string startButtonText;

        public string StartButtonText
        {
            get { return startButtonText; }
            set
            {
                startButtonText = value;
                NotifyOfPropertyChange(() => StartButtonText);
            }
        }

        private Uri icon;

        public Uri Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                NotifyOfPropertyChange(() => Icon);
            }
        }
    }
}