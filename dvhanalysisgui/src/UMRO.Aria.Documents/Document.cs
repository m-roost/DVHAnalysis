using System;

namespace UMRO.Aria.Documents
{
    public class Document
    {
        private readonly IFileService _fileService;

        private readonly ITimeService _timeService;

        public string PatientId { get; set; }

        public byte[] BinaryContent { get; set; }

        public FileFormat FileFormat { get; set; }

        public DateTime DateOfService { get; set; }

        public string DocumentType { get; set; }

        public string AuthoredByUserId { get; set; }

        public string EnteredByUserId { get; set; }

        public DateTime? DateEntered { get; set; }

        public string SupervisedByUserId { get; set; }

        public string SignedByUserId { get; set; }

        public DateTime? DateSigned { get; set; }

        public string ApprovedByUserId { get; set; }

        public DateTime? DateApproved { get; set; }

        public bool IsMarkedAsError { get; set; }

        public string TemplateName { get; set; }

        public bool IsCompleted { get; set; }

        public string PreviewText { get; set; }

        public string PatientFirstName { get; set; }

        public string PatientLastName { get; set; }

        public string DescriptionOfImage { get; set; }

        public bool IsPreApproved { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsSystemGen { get; set; }

        public Document(string userId, string patientId, string path, string docType)
            : this(new FileService(), new TimeService(), userId, patientId, path, docType)
        {
        }

        public Document(IFileService fileService, ITimeService timeService, string userId, string patientId, string path, string docType)
        {
            _fileService = fileService;
            _timeService = timeService;
            PatientId = patientId;
            BinaryContent = GetContent(path);
            FileFormat = GetFormat(path);
            DateOfService = GetCurrentTime();
            DocumentType = docType;
            AuthoredByUserId = userId;
            EnteredByUserId = userId;
            DateEntered = GetCurrentTime();
            SupervisedByUserId = userId;
        }

        private byte[] GetContent(string path)
        {
            return _fileService.ReadAllBytes(path);
        }

        private FileFormat GetFormat(string path)
        {
            string extension = _fileService.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
            {
                throw new FileFormatException("The document path must have an file extension.");
            }
            if (extension.ToLower().EndsWith("pdf"))
            {
                return FileFormat.Pdf;
            }
            throw new FileFormatException($"The given document file format ({extension}) is not supported.");
        }

        private DateTime GetCurrentTime()
        {
            return _timeService.Now();
        }
    }
}
