#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Shared.Exceptions
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class BadRequestException : BaseException
    {
        public BadRequestException(string message, string resource) 
            : base(message, resource, 400) { }
    }
}
