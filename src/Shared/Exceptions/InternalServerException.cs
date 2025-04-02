namespace Shared.Exceptions
{
    public class InternalServerException : BaseException
    {
        public InternalServerException(string resource) 
            : base("Internal Server Error", resource, 500) { }
    }
}
