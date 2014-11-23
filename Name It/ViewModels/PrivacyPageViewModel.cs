using Caliburn.Micro;

using NameIt.Common;
using NameIt.Resources;

namespace NameIt.ViewModels
{
    public class PrivacyPageViewModel : NameItViewModel
    {
        public PrivacyPageViewModel(BackgroundImageBrush backgroundImageBrush, INavigationService navigationService, ILog logger)
            : base(backgroundImageBrush, navigationService, logger)
        {
        }

        public string PageTitle
        {
            get { return AppResources.PrivacyPageTitle; }
        }
    }
}