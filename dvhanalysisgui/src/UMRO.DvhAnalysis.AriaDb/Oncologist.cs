namespace UMRO.DvhAnalysis.AriaDb
{
    public class Oncologist
    {
        public Oncologist(string id, string fullName)
        {
            Id = id;
            FullName = fullName;
        }

        public string Id { get; }
        public string FullName { get; }
    }
}