using ApplicationSetupWizardLite.Context;
using System.IO;

namespace ApplicationSetupWizardLite.Service
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

        // TODO: インストール処理を実装する。ここでは、Payloadフォルダ内のファイルを指定されたインストールパスにコピーする簡単な例。
        public void Install(string installPath)
        {
            string payloadPath = Path.Combine(
                AppContext.BaseDirectory,
                "Payload");

            foreach (string file in Directory.GetFiles(
                payloadPath,
                "*",
                SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(
                    payloadPath,
                    file);

                string destination = Path.Combine(
                    installPath,
                    relativePath);

                Directory.CreateDirectory(
                    Path.GetDirectoryName(destination)!);

                File.Copy(file, destination, true);
            }
        }

        public async Task InstallAsync(
            string installPath,
            IProgress<InstallProgress> progress)
        {
            string payloadPath = Path.Combine(
                AppContext.BaseDirectory,
                "Payload");

            string[] files = Directory.GetFiles(
                payloadPath,
                "*",
                SearchOption.AllDirectories);

            int total = files.Length;
            int completed = 0;

            foreach (string file in files)
            {
                string relativePath = Path.GetRelativePath(
                    payloadPath, file);

                string destination = Path.Combine(
                    installPath, relativePath);

                Directory.CreateDirectory(
                    Path.GetDirectoryName(destination)!);

                File.Copy(file, destination, true);

                completed++;

                int percent = total == 0
                    ? 100
                    : completed * 100 / total;

                progress.Report(new InstallProgress
                {
                    Percent = percent,
                    Status = "ファイルをコピーしています...",
                    Detail = $"{completed} / {total} ファイル"
                });

                await Task.Yield();
            }
        }
    }
}
