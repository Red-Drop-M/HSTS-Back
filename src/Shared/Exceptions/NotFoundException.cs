namespace Shared.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message, string resource) 
            : base(message, resource, 404) { }
    }
}
