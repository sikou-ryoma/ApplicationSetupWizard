using System.Diagnostics;
using System.IO;

namespace ApplicationSetupWizardLite.Services
{
    public class ApplicationLaunchService
    {
        public void Launch(string exePath)
        {
            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException(
                    "起動するアプリが見つかりません。",
                    exePath);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory =
                    Path.GetDirectoryName(exePath)!,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        // テスト用のメソッド。実際のアプリケーション起動ではなく、NotepadでexePathを開く。
        public void LaunchTest(string exePath)
        {
            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException(
                    "起動するアプリケーションが見つかりません。",
                    exePath);
            }

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "notepad.exe";
            startInfo.Arguments = exePath;

            Process.Start(startInfo);
        }

    }
}
