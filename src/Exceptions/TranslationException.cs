using System;

namespace MarksonDeckson.Exceptions
{
    public class TranslationException : Exception
    {
        public TranslationException(string message) : base(message)
        {
        }
    }
}
