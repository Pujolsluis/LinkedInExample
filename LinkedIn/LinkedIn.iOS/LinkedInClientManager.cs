using System;
using System.Threading.Tasks;
using LinkedIn.iOS;
using LinkedIn.Models;
using LinkedIn.Services;

[assembly: Xamarin.Forms.Dependency(typeof(LinkedInClientManager))]
namespace LinkedIn.iOS
{
    class LinkedInClientManager : ILinkedInClientManager
    {
        public event EventHandler<LinkedInClientResultEventArgs<string>> OnLogin;
        public event EventHandler OnLogout;
        public Task<LinkedInResponse<string>> LoginAsync()
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public bool IsLoggedIn { get; }
    }
}