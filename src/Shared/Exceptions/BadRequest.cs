namespace Shared.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException(string message, string resource) 
            : base(message, resource, 400) { }
    }
}
