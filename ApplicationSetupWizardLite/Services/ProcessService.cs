using ApplicationSetupWizardLite.Context;
using System.IO;

namespace ApplicationSetupWizardLite.Services
{
    public class ProcessService
    {
        private readonly XmlHelper _xmlHelper;
        private readonly ProcessContext _procCtx;
        private readonly SetupContext _setupCtx;

        public ProcessService(
            XmlHelper xmlHelper,
            ProcessContext processContext,
            SetupContext setupContext)
        {
            _xmlHelper = xmlHelper;
            _procCtx = processContext;
            _setupCtx = setupContext;
        }

        /// <summary>
        /// 各ステップを進める。現在のステップに応じて必要なサービスを呼び出す。
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void NextProcess()
        {
            switch(_procCtx.currentStep)
            {
                case 0:
                    // Step 0: Welcome
                    _procCtx.NextStep();
                    break;
                case 1:
                    // Step 1: Installation Path
                    _procCtx.NextStep();
                    break;
                case 2:
                    // Step 2: Confirmation
                    _procCtx.NextStep();
                    break;
                case 3:
                    
                    // Step 3: Installation
                    _procCtx.NextStep();
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
            switch(_procCtx.currentStep)
            {
                case 0:
                    // Step 0: Welcome
                    // Do nothing or handle logic if needed
                    break;
                case 1:
                    // Step 1: Installation Path
                    _procCtx.PreviousStep();
                    break;
                case 2:
                    // Step 2: Confirmation
                    _procCtx.PreviousStep();
                    break;
                case 3:
                    // Step 3: Installation
                    _procCtx.PreviousStep();
                    break;
                case 4:
                    // Step 4: Completion
                    _procCtx.PreviousStep();
                    break;
                default:
                    throw new InvalidOperationException("Invalid step index.");
            }
        }

        public async Task InstallAsync(
            string installPath,
            IProgress<InstallProgress> progress,
            CancellationToken cancellationToken)
        {
            string payloadPath = Path.Combine(
                AppContext.BaseDirectory,
                "Payload");

            string? parentPath = Path.GetDirectoryName(
                Path.TrimEndingDirectorySeparator(installPath));

            if (string.IsNullOrWhiteSpace(parentPath))
            {
                throw new InvalidOperationException(
                    "インストール先の親フォルダを取得できません。");
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

                    File.Copy(file, destination);

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

                    await Task.Yield();
                }

                cancellationToken.ThrowIfCancellationRequested();

                // 正式な配置先が既に存在する場合は上書きしない
                if (Directory.Exists(installPath) || File.Exists(installPath))
                {
                    throw new IOException(
                        $"インストール先が既に存在します: {installPath}");
                }

                // 一時フォルダを正式な配置先へ切り替える
                Directory.Move(tempPath, installPath);
                copyCompleted = true;
            }
            finally
            {
                if (!copyCompleted && Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, recursive: true);
                }
            }
        }
    }
}
