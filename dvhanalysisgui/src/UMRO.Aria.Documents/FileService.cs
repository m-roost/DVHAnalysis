// NOTE: The original source code of this component was lost. This file was recovered from the
// compiled assembly UMRO.Aria.Documents.dll (version 1.0.0.0) by decompilation (ILSpy) in September 2026.
// Copyright (C) The Regents of the University of Michigan. Licensed under GPL-3.0 (see LICENSE.txt).

using System.IO;

namespace UMRO.Aria.Documents
{
    internal class FileService : IFileService
    {
        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }
    }
}
