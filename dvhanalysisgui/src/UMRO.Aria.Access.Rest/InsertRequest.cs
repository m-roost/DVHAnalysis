// NOTE: The original source code of this component was lost. This file was recovered from the
// compiled assembly UMRO.Aria.Access.Rest.dll (version 1.0.0.0) by decompilation (ILSpy) in September 2026.
// Copyright (C) The Regents of the University of Michigan. Licensed under GPL-3.0 (see LICENSE.txt).

using System;
using Newtonsoft.Json;

namespace UMRO.Aria.Access.Rest
{
    public class InsertRequest : Request
    {
        private const string Name = "InsertDocumentRequest";

        private const string Namespace = "http://services.varian.com/Patient/Documents";

        [JsonProperty(PropertyName = "__type")]
        public string Type => string.Format("{0}:{1}", "InsertDocumentRequest", "http://services.varian.com/Patient/Documents");

        public PatientId PatientId { get; set; }

        public byte[] BinaryContent { get; set; }

        public int FileFormat { get; set; }

        public DateTime DateOfService { get; set; }

        public bool IsMedOncDocument { get; set; }

        public DocumentUser AuthoredByUser { get; set; }

        public DocumentUser EnteredByUser { get; set; }

        public DocumentUser SupervisedByUser { get; set; }

        public DocumentUser SignedByUser { get; set; }

        public bool IsSigned { get; set; }

        public DocumentUser ApprovedByUser { get; set; }

        public bool IsApproved { get; set; }

        public bool IsMarkedAsError { get; set; }

        public DateTime? DateEntered { get; set; }

        public DateTime? DateSigned { get; set; }

        public DateTime? DateApproved { get; set; }

        public DocumentType DocumentType { get; set; }

        public string TemplateName { get; set; }

        public bool IsCompleted { get; set; }

        public string PreviewText { get; set; }

        public string PatientFirstName { get; set; }

        public string PatientLastName { get; set; }

        public string DescriptionOfImage { get; set; }

        public bool IsPreApproved { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsSystemGen { get; set; }
    }
}
