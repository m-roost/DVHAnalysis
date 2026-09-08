// NOTE: The original source code of this component was lost. This file was recovered from the
// compiled assembly UMRO.Utils.DVHViewer-0.9.3.0.dll (version 0.9.3.0) by decompilation (ILSpy) in September 2026.
// Copyright (C) The Regents of the University of Michigan. Licensed under GPL-3.0 (see LICENSE.txt).

using System;
using System.Reflection;

namespace UMRO.Utils.DVHViewer
{
    public class version
    {
        public static string getVersion()
        {
            Version version2 = Assembly.GetExecutingAssembly().GetName().Version;
            return version2.Major + "." + version2.Minor;
        }
    }
}
