using System.Windows.Media;

using Caliburn.Micro;

using NameIt.Common;
using NameIt.Resources;

namespace NameIt.ViewModels
{
    public class NameItViewModel : Screen
    {
        protected readonly BackgroundImageBrush backgroundImageBrush;
        protected readonly INavigationService navigationService;
        protected readonly ILog logger;

        public NameItViewModel(BackgroundImageBrush backgroundImageBrush, INavigationService navigationService, ILog logger)
        {
            this.backgroundImageBrush = backgroundImageBrush;
            this.navigationService = navigationService;
            this.logger = logger;
        }

        public ImageBrush BackgroundImageBrush
        {
            get { return backgroundImageBrush.GetBackground(); }
        }

        public string ApplicationName
        {
            get { return AppResources.ApplicationTitle; }
        }

        private bool isBusy = false;

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
                NotifyOfPropertyChange(() => IsBusy);
            }
        }
    }
}