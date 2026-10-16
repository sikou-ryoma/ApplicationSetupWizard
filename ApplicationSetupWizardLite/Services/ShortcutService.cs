using IWshRuntimeLibrary;
using System.IO;

namespace ApplicationSetupWizardLite.Services
{
    public class ShortcutService
    {
        public void CreateDesktopShortcut(
            string shortcutName,
            string targetPath)
        {
            if (!System.IO.File.Exists(targetPath))
            {
                throw new FileNotFoundException(
                    "ショートカットのリンク先が見つかりません。",
                    targetPath);
            }

            string desktopPath =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.DesktopDirectory);

            string shortcutPath = Path.Combine(
                desktopPath,
                $"{shortcutName}.lnk");

            var shell = new WshShell();

            IWshShortcut shortcut =
                (IWshShortcut)shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory =
                Path.GetDirectoryName(targetPath)!;
            shortcut.Description = shortcutName;
            shortcut.Save();
        }
    }
}
