using System;

namespace UMRO.Aria.Documents
{
    internal class TimeService : ITimeService
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }
}
