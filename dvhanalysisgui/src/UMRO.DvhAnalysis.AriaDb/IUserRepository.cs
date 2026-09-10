namespace UMRO.DvhAnalysis.AriaDb
{
    public interface IUserRepository
    {
        User FindById(string userId);
    }
}
