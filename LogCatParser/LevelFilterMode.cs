using System;

namespace LessShittyLogcat {
    [Flags]
    public enum LevelFilterMode
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Debug = 4,
        Error = 8,
        Assert = 16,
        Verbose = Info | Warning | Debug | Error | Assert
    }
}