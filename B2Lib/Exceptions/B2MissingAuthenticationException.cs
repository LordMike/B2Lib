namespace B2Lib.Exceptions
{
    public class B2MissingAuthenticationException : B2Exception
    {
        public B2MissingAuthenticationException( string message) : base(message)
        {
        }
    }
}