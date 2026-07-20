namespace UMRO.DvhAnalysis.AriaDb
{
    public interface IOncologistRepository
    {
        Oncologist FindById(string oncologistId);
    }
}
