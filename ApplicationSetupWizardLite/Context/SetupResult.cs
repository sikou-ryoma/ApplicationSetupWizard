namespace ApplicationSetupWizardLite.Context
{
    public enum SetupStatus
    {
        Success,
        Failed,
        Cancelled
    }

    public class SetupResult
    {
        public SetupStatus Status { get; set; }

        public bool IsSuccess => Status == SetupStatus.Success;

        public string StatusText { get; set; } = "";
        public string DetailText { get; set; } = "";
    }
}