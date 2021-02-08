﻿using System;

namespace Authentication.Exceptions
{
    public class InvalidUserException : Exception
    {
        public InvalidUserException(string message) : base(message) { }
    }
}