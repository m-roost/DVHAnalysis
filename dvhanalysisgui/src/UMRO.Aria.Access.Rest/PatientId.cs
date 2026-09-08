// NOTE: The original source code of this component was lost. This file was recovered from the
// compiled assembly UMRO.Aria.Access.Rest.dll (version 1.0.0.0) by decompilation (ILSpy) in September 2026.
// Copyright (C) The Regents of the University of Michigan. Licensed under GPL-3.0 (see LICENSE.txt).

using Newtonsoft.Json;

namespace UMRO.Aria.Access.Rest
{
    public class PatientId
    {
        [JsonProperty(PropertyName = "ID1")]
        public string Id1 { get; }

        public PatientId(string id1)
        {
            Id1 = id1;
        }
    }
}
