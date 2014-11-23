using Caliburn.Micro;

using NameIt.Common;
using NameIt.Resources;

namespace NameIt.ViewModels
{
    public class HelpPageViewModel : NameItViewModel
    {
        public HelpPageViewModel(BackgroundImageBrush backgroundImageBrush, INavigationService navigationService, ILog logger)
            : base(backgroundImageBrush, navigationService, logger)
        {
        }

        public string PageTitle
        {
            get { return AppResources.HelpPageTitle; }
        }
    }
}