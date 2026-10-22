
namespace ApplicationSetupWizardLite.Context
{
    public enum SetupStatus
    {
        Success,
        Failed,
        Cancelled
    }

    // ショートカット用
    public enum ShortcutStatus
    {
        NotRequested,
        Created,
        Failed
    }

    public class SetupResult
    {
        // インストール結果
        public SetupStatus Status { get; set; }

        public bool IsSuccess => Status == SetupStatus.Success;

        public string StatusText { get; set; } = "";
        public string DetailText { get; set; } = "";

        // ショートカット作成
        public ShortcutStatus ShortcutStatus { get; set; }
            = ShortcutStatus.NotRequested;

        public string ShortcutDetailText { get; set; } = "";
    }
}
