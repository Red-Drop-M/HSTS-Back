namespace Shared.Exceptions
{
    public abstract class BaseException : Exception
    {
        public int StatusCode { get; }
        public string Resource { get; }

        protected BaseException(string message, string resource, int statusCode) 
            : base(message)
        {
            Resource = resource;
            StatusCode = statusCode;
        }
    }
}
