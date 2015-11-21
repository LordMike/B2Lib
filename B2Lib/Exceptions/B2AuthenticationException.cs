namespace B2Lib.Exceptions
{
    public class B2AuthenticationException : B2Exception
    {
        public string Code { get; set; }

        public B2AuthenticationException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}