using System.Collections.Generic;

namespace madoka
{
    public partial class Config
    {
        public override Dictionary<string, object> DefaultValues => new Dictionary<string, object>()
        {
            { nameof(MainWindowX), 200 },
            { nameof(MainWindowY), 100 },
            { nameof(MainWindowW), 1600 },
            { nameof(MainWindowH), 900 },
            { nameof(IsStartupWithWindows), false },
            { nameof(IsMinimizeStartup), false },
        };
    }
}
