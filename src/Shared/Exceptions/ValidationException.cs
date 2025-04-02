namespace Shared.Exceptions
{
    public class ValidationException : BaseException
    {
        public ValidationException(string message, string resource) 
            : base(message, resource, 400) { }
    }
}
