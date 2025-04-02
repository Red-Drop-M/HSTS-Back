namespace Shared.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message, string resource) 
            : base(message, resource, 401) { }
    }
}
