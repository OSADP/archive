﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCZONE.Exceptions
{
    public class TIMException : Exception
    {
        public TIMException()
        {
        }

        public TIMException(string message)
            : base(message)
        {
        }

        public TIMException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
