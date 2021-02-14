using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Exceptions
{
    public class AuthenticationException : Exception
    {
        public IEnumerable<IdentityError> IdentityErrors { get; set; }

        public AuthenticationException(string message) : base(message) { }

        public AuthenticationException(IEnumerable<IdentityError> identityErrors) : base(string.Empty)
        {
            IdentityErrors = identityErrors;
        }
    }
}
