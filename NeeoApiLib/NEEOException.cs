using System;
using Microsoft.Extensions.Logging;

namespace Home.Neeo
{
    public class NEEOException : Exception
    {
        static EventId eventId = new EventId(0, "NEEOException");
        public NEEOException (string message, Exception innerNEEOException = null) : base(message, innerNEEOException)
        {
            NEEOEnvironment.Logger.LogError(eventId, innerNEEOException, message);
        }
    }
}
