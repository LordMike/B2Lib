using System;
using System.Net;

namespace B2Lib.Exceptions
{
    public class B2Exception : Exception
    {
        public HttpStatusCode? HttpStatusCode { get; set; }

        public B2Exception(string message) : base(message)
        {

        }
    }
}