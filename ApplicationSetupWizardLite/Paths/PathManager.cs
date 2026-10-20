using System.IO;

namespace ApplicationSetupWizardLite.Paths
{
    public class PathManager
    {
        public string BaseDirPath { get; }
        public string XmlFilePath { get; }
        public string DefaultInstallPath { get; }
        public string PayloadPath { get; }

        public PathManager(string baseDir)
        {
            BaseDirPath = baseDir;
            XmlFilePath = Path.Combine(baseDir, "Conf", "config.xml");
            // excelBookDirPath = Path.Combine(baseDir, "workbook");
            // logDirPath = Path.Combine(baseDir, "log");
            DefaultInstallPath = 
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                // Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            PayloadPath = Path.Combine(baseDir, "Payload");
        }
    }
}
