using System.Collections.Generic;
using Vsky.Models;

namespace Vsky.Services.Messages
{
    public interface IMessageTokenProvider
    {
        void AddUserTokens(IList<Token> tokens, ApplicationUser user);
        void AddEmployeeTokens(IList<Token> tokens, Employee employee);
    }
}