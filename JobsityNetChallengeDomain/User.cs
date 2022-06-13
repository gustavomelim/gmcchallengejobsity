using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsityNetChallenge.Domain
{
    public class User
    {
        [BsonId]
        public string Id { get; set; }
        public string Password { get; set; }
    }
}
