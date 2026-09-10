using System.Threading.Tasks;
using UMRO.Aria.Documents;
using UMRO.DvhAnalysis.Logging;
using UMRO.DvhAnalysis.Logging.NLog;

namespace UMRO.DvhAnalysis.Script
{
    public class AriaDocumentClient
    {
        private static readonly ILogger _log = AppLog.GetLogger(nameof(AriaDocumentClient));

        private readonly DocumentService _documentService;

        // username is used to access the document service,
        // userId is the user who authored the document
        public AriaDocumentClient(string userId, string patientId)
        {
            var address = AssemblySettings.AriaDocAddress;
            var username = AssemblySettings.AriaDocUsername;
            var password = AssemblySettings.AriaDocPassword;
            var apiKey = AssemblySettings.AriaDocApiKey;

            _documentService = new DocumentService(address, username, password, apiKey);

            UserId = userId;
            PatientId = patientId;
        }

        public string UserId { get; }
        public string PatientId { get; }

        public async Task InsertDocumentAsync(string path, string fileFormat, string docType)
        {
            _log.Info($"Uploading '{docType}' document to ARIA for patient {PatientId} (file: {path})");
            await _documentService.InsertAsync(UserId, PatientId, path, docType);
            _log.Info($"Uploaded '{docType}' document to ARIA for patient {PatientId}");
        }
    }
}
