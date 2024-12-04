using System;
using Idibri.RevitPlugin.Common;

namespace Idibri.RevitPlugin.ConduitSizeCalculator
{
    public class BadCableDefinitionStringException : CommandExceptionBase
    {
        public BadCableDefinitionStringException() : base() { }
        public BadCableDefinitionStringException(string message) : base(message) { }
        public BadCableDefinitionStringException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class UnknownConduitTypeException : CommandExceptionBase
    {
        public UnknownConduitTypeException() : base() { }
        public UnknownConduitTypeException(string message) : base(message) { }
        public UnknownConduitTypeException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class NoSizeSpecifiedException : CommandExceptionBase
    {
        public NoSizeSpecifiedException() : base() { }
        public NoSizeSpecifiedException(string message) : base(message) { }
        public NoSizeSpecifiedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
