﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsityNetChallenge.Domain
{
    public class QueueMessage
    {
        public string Message { get; set; }
        public User User { get; set; }
    }
}
