
using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace ApplicationSetupWizardLite.Services
{
    public class PayloadService
    {

        // 実行ファイルに埋め込んだPayload.zipを展開
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

        // 展開時に作成したtempを削除
        public void CleanupPayload(string? extractedPayloadPath)
        {
            if (string.IsNullOrWhiteSpace(extractedPayloadPath))
            {
                return;
            }

            // 展開先の親フォルダを取得する
            string? tempPath = Path.GetDirectoryName(
                Path.TrimEndingDirectorySeparator(extractedPayloadPath));

            if (string.IsNullOrWhiteSpace(tempPath))
            {
                return;
            }

            // 展開先が存在する場合だけ削除する
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, recursive: true);
            }
        }

    }
}
