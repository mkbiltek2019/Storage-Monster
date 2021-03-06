﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StorageMonster.Services.Security;
using System.Runtime.Serialization;

namespace StorageMonster.Web.Services.Security
{
    [Serializable]
    public class PasswordsMismatchException : MonsterSecurityException
    {
        public PasswordsMismatchException()
        {
        }

        public PasswordsMismatchException(string message)
            : base(message)
        {
        }

        public PasswordsMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PasswordsMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

