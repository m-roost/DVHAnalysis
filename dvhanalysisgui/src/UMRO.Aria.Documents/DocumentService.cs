using System;
using System.Threading.Tasks;
using UMRO.Aria.Access.Rest;

namespace UMRO.Aria.Documents
{
    public class DocumentService
    {
        private const double SecondsToSubtract = 5.0;

        private readonly AriaAccessClient _ariaAccess;

        public DocumentService(string address, string username, string password, string apiKey)
        {
            _ariaAccess = new AriaAccessClient(address, username, password, apiKey);
        }

        public Task InsertAsync(string userId, string patientId, string documentPath, string documentType)
        {
            return InsertAsync(new Document(userId, patientId, documentPath, documentType));
        }

        public Task InsertAsync(Document document)
        {
            InsertRequest request = new InsertRequest
            {
                PatientId = CreatePatientId(document.PatientId),
                BinaryContent = document.BinaryContent,
                FileFormat = Convert(document.FileFormat),
                DateOfService = Adjust(document.DateOfService),
                IsMedOncDocument = false,
                AuthoredByUser = CreateDocumentUser(document.AuthoredByUserId),
                EnteredByUser = CreateDocumentUser(document.EnteredByUserId),
                SupervisedByUser = CreateDocumentUser(document.SupervisedByUserId),
                SignedByUser = CreateDocumentUser(document.SignedByUserId),
                IsSigned = !string.IsNullOrEmpty(document.SignedByUserId),
                ApprovedByUser = CreateDocumentUser(document.ApprovedByUserId),
                IsApproved = !string.IsNullOrEmpty(document.ApprovedByUserId),
                IsMarkedAsError = document.IsMarkedAsError,
                DateEntered = document.DateEntered,
                DateSigned = document.DateSigned,
                DateApproved = document.DateApproved,
                DocumentType = CreateDocumentType(document.DocumentType),
                TemplateName = document.TemplateName,
                IsCompleted = document.IsCompleted,
                PreviewText = document.PreviewText,
                PatientFirstName = document.PatientFirstName,
                PatientLastName = document.PatientLastName,
                DescriptionOfImage = document.DescriptionOfImage,
                IsPreApproved = document.IsPreApproved,
                IsPrivate = document.IsPrivate,
                IsSystemGen = document.IsSystemGen
            };
            return _ariaAccess.SendRequestAsync(request);
        }

        private PatientId CreatePatientId(string patientId)
        {
            return new PatientId(patientId);
        }

        private int Convert(FileFormat fileFormat)
        {
            return (int)fileFormat;
        }

        private DateTime Adjust(DateTime dateTime)
        {
            return dateTime - TimeSpan.FromSeconds(5.0);
        }

        private DocumentType CreateDocumentType(string type)
        {
            if (type == null)
            {
                return null;
            }
            return new DocumentType(type);
        }

        private DocumentUser CreateDocumentUser(string userId)
        {
            if (userId == null)
            {
                return null;
            }
            return new DocumentUser(userId);
        }
    }
}
