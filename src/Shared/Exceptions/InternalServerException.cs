#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Shared.Exceptions
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class InternalServerException : BaseException
    {
        public InternalServerException(string resource) 
            : base("Internal Server Error", resource, 500) { }
    }
}
