using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsityNetChallenge.Domain
{
    public class Message
    {
        [BsonId]
        public long Id { get; set; }
        public string User { get; set; }
        public string MessageContent { get; set; }
    }
}
