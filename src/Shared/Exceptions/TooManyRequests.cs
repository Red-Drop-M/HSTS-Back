namespace Shared.Exceptions
{
    public class TooManyRequestsException : BaseException
    {
        public TooManyRequestsException(string resource) 
            : base("Too many requests. Please try again later.", resource, 429) { }
    }
}
