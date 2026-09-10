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
