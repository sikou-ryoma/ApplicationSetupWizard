namespace ApplicationSetupWizardLite.Context
{
    public class ProcessContext
    {
        public int CurrentStep { get; private set; }
        public bool IsCompleted => CurrentStep >= 4;
        
        public ProcessContext()
        {
            CurrentStep = 0;
        }
        public void NextStep()
        {
            if (CurrentStep < 4)
            {
                CurrentStep++;
            }
        }

        public void PreviousStep()
        {
            if (CurrentStep > 0)
            {
                CurrentStep--;
            }
        }
    }
}
