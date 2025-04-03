#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Shared.Exceptions
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class TooManyRequestsException : BaseException
    {
        public TooManyRequestsException(string resource) 
            : base("Too many requests. Please try again later.", resource, 429) { }
    }
}
