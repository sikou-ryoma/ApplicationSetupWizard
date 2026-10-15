using UserControl = System.Windows.Controls.UserControl;

namespace ApplicationSetupWizardLite.Navigation
{
    public class StepViewState
    {
        public UserControl? View { get; set; }

        public string? NextButtonText { get; set; }
        public string? BackButtonText { get; set; }

        public bool CanGoNext { get; set; }
        public bool CanGoBack { get; set; }

        public bool ShowCancel { get; set; }
        public bool ShowBack { get; set; }

    }

}
