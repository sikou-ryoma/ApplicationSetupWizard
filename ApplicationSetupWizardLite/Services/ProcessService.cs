using ApplicationSetupWizardLite.Context;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ApplicationSetupWizardLite.Services
{
    public class ProcessService
    {
        private readonly XmlHelper _xmlHelper;
        private readonly ProcessContext _processContext;
        private readonly SetupContext _setupContext;
        private readonly ILogger<ProcessService> _logger;


        public ProcessService(
            XmlHelper xmlHelper,
            ProcessContext processContext,
            SetupContext setupContext,
            ILogger<ProcessService> logger)
        {
            _xmlHelper = xmlHelper;
            _processContext = processContext;
            _setupContext = setupContext;
            _logger = logger;
        }

        /// <summary>
        /// 各ステップを進める。現在のステップに応じて必要なサービスを呼び出す。
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void NextProcess()
        {
            switch(_processContext.CurrentStep)
            {
                case 0:
                    // Step 0: Welcome
                    _processContext.NextStep();
                    break;
                case 1:
                    // Step 1: Installation Path
                    _processContext.NextStep();
                    break;
                case 2:
                    // Step 2: Confirmation
                    _processContext.NextStep();
                    break;
                case 3:
                    
                    // Step 3: Installation
                    _processContext.NextStep();
                    break;
                case 4:
                    // Step 4: Completion
                    // Do nothing or handle completion logic if needed
                    break;
                default:
                    throw new InvalidOperationException("Invalid step index.");
            }
        }

        public void PreviousProcess()
        {
            switch(_processContext.CurrentStep)
            {
                case 0:
                    break;
                case 1:
                    _processContext.PreviousStep();
                    break;
                case 2:
                    _processContext.PreviousStep();
                    break;
                case 3:
                    _processContext.PreviousStep();
                    break;
                case 4:
                    _processContext.PreviousStep();
                    break;
                default:
                    throw new InvalidOperationException("Invalid step index.");
            }
        }
        
        public async Task InstallAsync(
            string payloadPath,
            string installPath,
            IProgress<InstallProgress> progress,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "インストールを開始します。インストール先: {InstallPath}",
                installPath);

            string? parentPath = Path.GetDirectoryName(
                Path.TrimEndingDirectorySeparator(installPath));

            if (string.IsNullOrWhiteSpace(parentPath))
            {
                throw new InvalidOperationException(
                    "インストール先の親フォルダを取得できません。");
            }

            // 正式な配置先が既に存在する場合は上書きしない
            if (Directory.Exists(installPath) || File.Exists(installPath))
            {
                throw new IOException(
                    $"インストール先が既に存在します: {installPath}");
            }

            string tempPath = installPath + ".installing";

            if (Directory.Exists(tempPath))
            {
                throw new IOException(
                    $"一時フォルダが既に存在します: {tempPath}");
            }

            Directory.CreateDirectory(parentPath);
            Directory.CreateDirectory(tempPath);

            bool copyCompleted = false;

            try
            {
                string[] files = Directory.GetFiles(
                    payloadPath,
                    "*",
                    SearchOption.AllDirectories);

                int total = files.Length;
                int completed = 0;

                foreach (string file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string relativePath = Path.GetRelativePath(
                        payloadPath,
                        file);

                    string destination = Path.Combine(
                        tempPath,
                        relativePath);

                    Directory.CreateDirectory(
                        Path.GetDirectoryName(destination)!);

                    await Task.Run(() =>
                    {
                        File.Copy(file, destination);
                    });
                    
                    completed++;

                    int percent = total == 0
                        ? 100
                        : completed * 100 / total;

                    progress.Report(new InstallProgress
                    {
                        Percent = percent,
                        Status = "一時フォルダにファイルをコピーしています...",
                        Detail = $"{completed} / {total} ファイル"
                    });

                    _logger.LogDebug(file, "ファイルをコピー");
                    await Task.Yield();
                }

                cancellationToken.ThrowIfCancellationRequested();


                // 一時フォルダを正式な配置先へ切り替える
                Directory.Move(tempPath, installPath);
                copyCompleted = true;
            }
            finally
            {
                if (!copyCompleted && Directory.Exists(tempPath))
                {
                    try
                    {
                        Directory.Delete(tempPath, recursive: true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "インストール一時フォルダの削除に失敗");
                    }
                }
            }
            _logger.LogInformation(
                "インストールが正常に完了しました。インストール先: {InstallPath}",
                installPath);
        }

        public long GetPayloadSize(string payloadPath)
        {
            if (!Directory.Exists(payloadPath))
            {
                throw new DirectoryNotFoundException(
                    $"Payloadフォルダが見つかりません: {payloadPath}");
            }

            return Directory
                .EnumerateFiles(
                    payloadPath,
                    "*",
                    SearchOption.AllDirectories)
                .Sum(file => new FileInfo(file).Length);
        }

        public string FormatFileSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };

            double size = bytes;
            int unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            // return $"{size:N2} {units[unitIndex]}";
            return $"約 {size:N0} {units[unitIndex]}";
        }
    }
}
