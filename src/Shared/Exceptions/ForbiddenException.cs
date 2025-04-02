namespace Shared.Exceptions
{
    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string message, string resource) 
            : base(message, resource, 403) { }
    }
}
