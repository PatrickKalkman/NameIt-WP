namespace NameIt.Common
{
    public class NameItSettingsManager
    {
        private readonly SettingsHelper settingsHelper;

        public NameItSettingsManager(SettingsHelper settingsHelper)
        {
            this.settingsHelper = settingsHelper;
        }

        private const string UseLearningModeKey = "UseLearningMode";

        public bool UseLearningMode
        {
            get
            {
                return this.settingsHelper.GetSetting(UseLearningModeKey, false);
            }

            set
            {
                this.settingsHelper.UpdateSetting(UseLearningModeKey, value);
            }
        }

        private const string UseVoiceRecognitionKey = "UseVoiceRecognition";

        public bool UseVoiceRecognition
        {
            get
            {
                return this.settingsHelper.GetSetting(UseVoiceRecognitionKey, true);
            }

            set
            {
                this.settingsHelper.UpdateSetting(UseVoiceRecognitionKey, value);
            }
        }


    }
}