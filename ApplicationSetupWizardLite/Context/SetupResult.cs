using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationSetupWizardLite.Context
{
    public class SetupResult
    {
        public bool IsSuccess { get; set; } = false;
        public string StatusText { get; set; } = "";
        public string DetailText { get; set; } = "";
    }
}
