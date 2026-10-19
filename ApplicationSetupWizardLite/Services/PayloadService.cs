
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace ApplicationSetupWizardLite.Services
{
    public class PayloadService
    {
        public string ExtractPayload()
        {
            string tempPath = Path.Combine(
                Path.GetTempPath(),
                "ApplicationSetupWizardLite",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(tempPath);

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                using Stream? stream = assembly.GetManifestResourceStream(
                    "ApplicationSetupWizardLite.Resources.Payload.zip");

                if (stream == null)
                {
                    throw new FileNotFoundException(
                        "埋め込みリソース Payload.zip が見つかりません。");
                }

                string zipPath = Path.Combine(tempPath, "Payload.zip");

                using (FileStream fileStream = File.Create(zipPath))
                {
                    stream.CopyTo(fileStream);
                }

                string extractPath = Path.Combine(tempPath, "Payload");

                ZipFile.ExtractToDirectory(
                    zipPath,
                    extractPath);

                File.Delete(zipPath);

                return extractPath;
            }
            catch
            {
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }

                throw;
            }
        }
    }
}
