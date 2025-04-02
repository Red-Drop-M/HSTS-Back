namespace Shared.Exceptions
{
    public class ConflictException : BaseException
    {
        public ConflictException(string message, string resource) 
            : base(message, resource, 409) { }
    }
}
