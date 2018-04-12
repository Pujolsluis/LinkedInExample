using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LinkedIn.iOS;
using LinkedIn.Models;
using LinkedIn.Services;
using Xamarin.iOS.LinkedIn;

[assembly: Xamarin.Forms.Dependency(typeof(LinkedInClientManager))]
namespace LinkedIn.iOS
{
    class LinkedInClientManager : ILinkedInClientManager
    {
        public List<string> FieldsList { get; set; }

        static TaskCompletionSource<LinkedInResponse<string>> _loginTcs;

        private static EventHandler<LinkedInClientResultEventArgs<string>> _onLogin;
        public event EventHandler<LinkedInClientResultEventArgs<string>> OnLogin
        {
            add => _onLogin += value;
            remove => _onLogin -= value;
        }

        public async Task<LinkedInResponse<string>> LoginAsync(List<string> fieldsList)
        {
            _loginTcs = new TaskCompletionSource<LinkedInResponse<string>>();
            FieldsList = fieldsList;

            SessionManager.CreateSessionWithAuth(
                new[] { Permission.BasicProfile, Permission.EmailAddress },
                "state",
                true,
                returnState =>
                {
                    GetUserProfile(FieldsList);
                    Debug.WriteLine("Auth Successful");
                },
                error =>
                {
                    Debug.WriteLine("Auth Error: " + error.LocalizedFailureReason);
                });

            return await _loginTcs.Task;
        }

        static EventHandler _onLogout;
        public event EventHandler OnLogout
        {
            add => _onLogout += value;
            remove => _onLogout -= value;
        }


        protected virtual void OnLogoutCompleted(EventArgs e)
        {
            _onLogout?.Invoke(this, e);
        }

        public void Logout()
        {
            SessionManager.ClearSession();
            OnLogoutCompleted(EventArgs.Empty);
        }

        private void GetUserProfile(List<string> fieldsList)
        {
            if (SessionManager.HasValidSession)
            {
                string fields = "";

                for (int i = 0; i < fieldsList.Count; i++)
                {
                    if (i != fieldsList.Count - 1)
                    {
                        fields += fieldsList[i] + ",";
                    }
                    else
                    {
                        fields += fieldsList[i];
                    }
                }

                var apiRequestUrl =
                    "https://api.linkedin.com/v1/people/~:(" + fields + ")?format=json";

                ApiHelper.SharedInstance.GetRequest(
                    apiRequestUrl,
                    apiResponse => {
                        var linkedInArgs =
                            new LinkedInClientResultEventArgs<string>(apiResponse.Data.ToString(), LinkedInActionStatus.Completed, apiResponse.StatusCode.ToString());

                        // Send the result to the receivers
                        _onLogin.Invoke(this, linkedInArgs);
                        _loginTcs.TrySetResult(new LinkedInResponse<string>(linkedInArgs));
                    },
                    error => {
                        //TODO REPLACE OR VERIFY IF THIS IS CORRECT
                        var linkedInArgs =
                            new LinkedInClientResultEventArgs<string>(null, LinkedInActionStatus.Completed, error.LocalizedDescription);

                        // Send the result to the receivers
                        _onLogin.Invoke(this, linkedInArgs);
                        _loginTcs.TrySetResult(new LinkedInResponse<string>(linkedInArgs));
                    });
            }
        }

        public bool IsLoggedIn { get; }
    }
}