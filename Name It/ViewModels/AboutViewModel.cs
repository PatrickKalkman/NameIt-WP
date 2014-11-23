using System.Reflection;

using Caliburn.Micro;

using Microsoft.Phone.Tasks;

using NameIt.Common;
using NameIt.Resources;

namespace NameIt.ViewModels
{
    public class AboutViewModel : NameItViewModel
    {
        public AboutViewModel(BackgroundImageBrush backgroundImageBrush, INavigationService navigationService, ILog logger)
            : base(backgroundImageBrush, navigationService, logger)
        {
        }

        public string PageTitle
        {
            get { return AppResources.AboutPageTitle; }
        }


        public string Version
        {
            get
            {
                var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
                return nameHelper.Version.ToString();
            }
        }

        public string AppDescription
        {
            get
            {
                return "Challenge yourself by trying to recognize soundbites from the Nokia Mixes music library. Play using your favorite genre and try to beat your highest score.";
            }
        }

        public void RateThisApp()
        {
            var reviewTask = new MarketplaceReviewTask();
            reviewTask.Show();
        }

        public void SendAnEmail()
        {
            var emailTask = new EmailComposeTask();
            emailTask.To = "pkalkie@gmail.com";
            emailTask.Show();
        }


    }
}
