using Newtonsoft.Json;

namespace UMRO.Aria.Access.Rest
{
    public class Request
    {
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            });
        }
    }
}
