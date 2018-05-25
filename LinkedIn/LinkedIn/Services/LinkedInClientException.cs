using System;
namespace LinkedIn.Services
{
	public class  LinkedInClientBaseException : Exception
    {
        public static string SignInDefaultErrorMessage = "The LinkedIn Sign In could not complete it's process correctly.";

		public LinkedInClientBaseException() : base() { }
		public LinkedInClientBaseException(string message) : base(message) { }
		public LinkedInClientBaseException(string message, System.Exception inner) : base(message, inner) { }
    }
}
