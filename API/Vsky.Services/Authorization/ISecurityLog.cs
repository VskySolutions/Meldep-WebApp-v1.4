using System;
using Vsky.Models;

namespace Vsky.Services.Authorization
{
    /// <summary>Writes security events to the Log table. A logging failure never affects a request.</summary>
    public interface ISecurityLog
    {
        /// <summary>Returns the new row's ID to use as a reference, or null if it couldn't be written.</summary>
        string Write(LogLevel level, string shortMessage, string fullMessage = "", string userId = null);

        /// <summary>Records an exception once, and returns its reference. An exception that's already recorded gets its existing reference.</summary>
        string WriteError(string description, Exception exception, string userId = null);
    }
}
