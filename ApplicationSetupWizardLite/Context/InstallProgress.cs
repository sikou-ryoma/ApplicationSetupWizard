using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationSetupWizardLite.Context
{
    public class InstallProgress
    {
        public int Percent { get; set; }
        public string Status { get; set; } = "";
        public string Detail { get; set; } = "";
    }
}
