using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.ExceptionsConfig
{
    public class CustomException
    {
        //404 Not Found
        public class NotFoundException : Exception
        {
            public NotFoundException() : base() { }
            public NotFoundException(string message) : base(message) { }
            public NotFoundException(string message, Exception inner)
                : base(message, inner) { }
        }

        //400 Bad Request
        public class BadRequestException : Exception
        {
            public BadRequestException() : base() { }
            public BadRequestException(string message) : base(message) { }
            public BadRequestException(string message, Exception inner)
                : base(message, inner) { }
        }

        //409 Conflict
        public class ConflictException : Exception
        {
            public ConflictException() : base() { }
            public ConflictException(string message) : base(message) { }
            public ConflictException(string message, Exception inner)
                : base(message, inner) { }
        }

        // 423 Locked
        public class LockedException : Exception
        {
            public LockedException() : base() { }
            public LockedException(string message) : base(message) { }
            public LockedException(string message, Exception inner)
                : base(message, inner) { }
        }
    }
}
