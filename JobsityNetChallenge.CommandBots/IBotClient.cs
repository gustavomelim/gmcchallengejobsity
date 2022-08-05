using JobsityNetChallenge.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsityNetChallenge.CommandBots
{
    public interface IBotClient
    {
        public Task<string> ProcessCommand(User user, string code);
    }
}
