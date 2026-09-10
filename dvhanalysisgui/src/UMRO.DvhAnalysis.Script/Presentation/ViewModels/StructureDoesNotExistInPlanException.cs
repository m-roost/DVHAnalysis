using System;
using System.Runtime.Serialization;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels
{
    public class StructureDoesNotExistInPlanException : Exception
    {
        public StructureDoesNotExistInPlanException() { }
        public StructureDoesNotExistInPlanException(string message) : base(message) { }
        public StructureDoesNotExistInPlanException(string message, Exception innerException) : base(message, innerException) { }
        protected StructureDoesNotExistInPlanException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}