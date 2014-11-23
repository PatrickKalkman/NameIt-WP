using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Windows.Phone.Speech.Recognition;

using Caliburn.Micro;

using Microsoft.Phone.Shell;

using NameIt.Common;
using NameIt.Nokia;
using NameIt.Views;

using Nokia.Music.Tasks;

using Telerik.Windows.Controls;

namespace NameIt.ViewModels
{
    public class GamePageViewModel : NameItViewModel
    {
        private readonly RandomSongListSelector randomSongSelector;
        private readonly NameItSettingsManager settingsManager;
        private readonly LeaderboardManager leaderboardManager;
        private readonly DispatcherTimer gameTimer = new DispatcherTimer();
        private readonly DispatcherTimer highResTimer = new DispatcherTimer();

        public GamePageViewModel(
            INavigationService navigationService,
            BackgroundImageBrush backgroundImageBrush,
            ILog logger,
            RandomSongListSelector randomSongSelector,
            NameItSettingsManager settingsManager, 
            LeaderboardManager leaderboardManager)
            : base(backgroundImageBrush, navigationService, logger)
        {
            this.randomSongSelector = randomSongSelector;
            this.settingsManager = settingsManager;
            this.leaderboardManager = leaderboardManager;
            IsBusy = true;

            gameTimer.Tick += gameTimer_Tick;
            gameTimer.Interval = new TimeSpan(0, 0, 0, 1);

            highResTimer.Tick += highResTimer_Tick;
            highResTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            Score = 0;
            IsSubmitAnswerButtonVisible = false;
            IsNextSongButtonVisible = false;
            songIndex = -1;
            ScoreInformation = "Score: ";
        }

        void highResTimer_Tick(object sender, EventArgs e)
        {
            TimeLeft = TimeLeft - 1;
        }

        private int numberOfSecondsToGo = 8;

        void gameTimer_Tick(object sender, EventArgs e)
        {
            if (numberOfSecondsToGo > 0)
            {
                numberOfSecondsToGo = numberOfSecondsToGo - 1;
            }
            else
            {
                IsSubmitAnswerButtonVisible = true;
                IsNextSongButtonVisible = false;
                view.TimeLeftProgressBar.Value = 0;
                gameTimer.Stop();
                highResTimer.Stop();
                view.MusicPlayer.Stop();
                if (settingsManager.UseVoiceRecognition)
                {
                    RecognizeSong(artists);
                }
            }
        }

        private List<Song> selectedSongs;

        private GamePage view;

        private int songIndex;

        private List<string> artists;

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            gameTimer.Stop();
            highResTimer.Stop();
            view.MusicPlayer.Stop();
        }

        protected async override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            this.view = view as GamePage;
            await LoadSongsAndStart();
            PageTitle = "Playing " + GenreId;

        }

        private async Task LoadSongsAndStart()
        {
            Cheat = string.Empty;
            var progressIndicator = new ProgressIndicator();
            progressIndicator.IsIndeterminate = true;
            progressIndicator.IsVisible = true;
            progressIndicator.Text = string.Format("Loading {0} songs", GenreId);

            SystemTray.SetProgressIndicator(this.view, progressIndicator);

            songIndex = -1;
            Score = 0;
            ScoreInformation = "Score: ";
            Counter = "";
            IsSubmitAnswerButtonVisible = false;
            selectedSongs = await randomSongSelector.SelectRandomSongList(GenreId, 10);
            artists = CreateArtistList(selectedSongs);
            if (SystemTray.ProgressIndicator != null)
            {
                SystemTray.ProgressIndicator.IsVisible = false;
            }
            IsNextSongButtonVisible = true;
        }

        private List<string> CreateArtistList(IEnumerable<Song> songs)
        {
            var artistAndSongs = new List<string>();
            foreach (var song in songs)
            {
                artistAndSongs.Add(song.ArtistName);
                artistAndSongs.Add(song.SongName);
            }
            return artistAndSongs;
        }

        public void PlayNextSong()
        {
            numberOfSecondsToGo = 8;
            TimeLeft = numberOfSecondsToGo * 10;
            IsNextSongButtonVisible = false;
            IsSubmitAnswerButtonVisible = false;
            IsNextSongButtonVisible = false;
            gameTimer.Start();
            highResTimer.Start();
            view.MusicPlayer.Stop();

            songIndex++;

            view.TimeLeftProgressBar.IsEnabled = true;
            view.TimeLeftProgressBar.IsIndeterminate = false;
            view.TimeLeftProgressBar.Maximum = 100;

            Counter = string.Format("Playing song {0} of {1}", (songIndex + 1).ToString(), selectedSongs.Count);
            view.MusicPlayer.Source = selectedSongs[songIndex].SongUri;
            Cheat = String.Format("{0} / {1}", selectedSongs[songIndex].ArtistName, selectedSongs[songIndex].SongName);
            Answer = string.Empty;
            view.MusicPlayer.Play();
        }

        public async void SubmitAnswer()
        {
            string filteredAnswer = Filter(Answer);
            string filteredSongName = Filter(selectedSongs[songIndex].SongName);
            string filteredArtist = Filter(selectedSongs[songIndex].ArtistName);

            if (string.Compare(filteredAnswer, filteredSongName, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(filteredAnswer, filteredArtist, StringComparison.OrdinalIgnoreCase) == 0)
            {
                string response = string.Format("Your response was correct!!!, the correct answer was \"{0}\" from \"{1}\"", filteredSongName, filteredArtist);
                MessageBox.Show(response);
                Score = Score + 30;
                ScoreInformation = "Score: " + Score.ToString();
            }
            else
            {
                string response = string.Format("Your response was not correct, the correct answer was \"{0}\" from \"{1}\"", filteredSongName, filteredArtist);
                MessageBox.Show(response);
            }

            if (songIndex >= selectedSongs.Count - 1)
            {
                if (Score > 0)
                {
                    leaderboardManager.StoreScore(GenreId, Score);
                }
                string information = string.Format("You scored {0} points!, do want to try again?", Score);
                MessageBoxClosedEventArgs result = await RadMessageBox.ShowAsync(information, "Game over", MessageBoxButtons.YesNo);
                if (result.ButtonIndex == 1)
                {
                    navigationService.GoBack();
                }
                else
                {
                    await LoadSongsAndStart();
                }
            }
            else
            {
                PlayNextSong();
                IsSubmitAnswerButtonVisible = false;
            }
        }

        private string Filter(string stringToFilter)
        {
            return stringToFilter.Replace(".", string.Empty).Replace("!", string.Empty);
        }

        public string GenreId { get; set; }

        private string counter;

        public string Counter
        {
            get { return counter; }
            set
            {
                counter = value;
                NotifyOfPropertyChange(() => Counter);
            }
        }

        
        private string scoreInformation;

        public string ScoreInformation
        {
            get { return scoreInformation; }
            set
            {
                scoreInformation = value;
                NotifyOfPropertyChange(() => ScoreInformation);
            }
        }

        private string songInformation;

        public string SongInformation
        {
            get { return songInformation; }
            set
            {
                songInformation = value;
                NotifyOfPropertyChange(() => SongInformation);
            }
        }

        private string answer;

        public string Answer
        {
            get { return answer; }
            set
            {
                answer = value;
                NotifyOfPropertyChange(() => Answer);
            }
        }

        private int score;

        public int Score
        {
            get { return score; }
            set
            {
                score = value;
                NotifyOfPropertyChange(() => Score);
            }
        }

        private int timeLeft;

        public int TimeLeft
        {
            get { return timeLeft; }
            set
            {
                timeLeft = value;
                NotifyOfPropertyChange(() => TimeLeft);
            }
        }

        private string cheat;

        public string Cheat
        {
            get { return cheat; }
            set
            {
                cheat = value;
                NotifyOfPropertyChange(() => Cheat);
            }
        }

        public bool IsCheatInformationVisible
        {
            get { return settingsManager.UseLearningMode; }
        }

        private bool isNextSongButtonVisible;

        public bool IsNextSongButtonVisible
        {
            get { return isNextSongButtonVisible; }
            set
            {
                isNextSongButtonVisible = value;
                NotifyOfPropertyChange(() => IsNextSongButtonVisible);
            }
        }

        private bool isSubmitAnswerButtonVisible;

        public bool IsSubmitAnswerButtonVisible
        {
            get { return isSubmitAnswerButtonVisible; }
            set
            {
                isSubmitAnswerButtonVisible = value;
                NotifyOfPropertyChange(() => IsSubmitAnswerButtonVisible);
            }
        }

        string pageTitle;

        public string PageTitle
        {
            get
            {
                return pageTitle; 
            }
            set
            {
                pageTitle = value;
                NotifyOfPropertyChange(() => PageTitle);
            }
        }

        public async void RecognizeSong(List<string> artistsAndSongs)
        {
            var speechRecognition = new SpeechRecognizerUI();
            speechRecognition.Settings.ListenText = "Name the artist or the song";
            speechRecognition.Settings.ReadoutEnabled = false;
            speechRecognition.Settings.ShowConfirmation = false;
            speechRecognition.Recognizer.Grammars.AddGrammarFromList("answer", artistsAndSongs);
            SpeechRecognitionUIResult recoResult = await speechRecognition.RecognizeWithUIAsync();

            if (recoResult.ResultStatus == SpeechRecognitionUIStatus.Succeeded)
            {
                Answer = recoResult.RecognitionResult.Text;
            }
        }
    }
}