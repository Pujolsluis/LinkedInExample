using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using LinkedIn.Droid;
using LinkedIn.Platform;
using LinkedIn.Platform.Errors;
using LinkedIn.Platform.Listeners;
using LinkedIn.Platform.Utils;
using LinkedIn.Services;

[assembly: Xamarin.Forms.Dependency(typeof(LinkedInClientManager))]
namespace LinkedIn.Droid
{
    class LinkedInClientManager : Java.Lang.Object, ILinkedInClientManager, IAuthListener
    {
        // Class Debug Tag
        private static string Tag = typeof(LinkedInClientManager).FullName;
        public static int AuthActivityID = Tag.GetHashCode() % Int16.MaxValue;
        public static LISessionManager LinkedInSessionManager { get; set; }
        public static Activity CurrentActivity { get; set; }
        public List<string> FieldsList { get; set; }

        static TaskCompletionSource<LinkedInResponse<string>> _loginTcs;

        //public static LinkedInClientManager ManagerInstance { get; } = new LinkedInClientManager();

        public bool IsLoggedIn { get; }


        public static void Initialize(Activity activity)
        {
            CurrentActivity = activity;

            LinkedInSessionManager = LISessionManager.GetInstance(Application.Context);

        }

        private static EventHandler<LinkedInClientResultEventArgs<string>> _onLogin;
        public event EventHandler<LinkedInClientResultEventArgs<string>> OnLogin
        {
            add => _onLogin += value;
            remove => _onLogin -= value;
        }

        public event EventHandler OnLogout;
        


        public async Task<LinkedInResponse<string>> LoginAsync(List<string> fieldsList)
        {
            _loginTcs = new TaskCompletionSource<LinkedInResponse<string>>();
            FieldsList = fieldsList;
            LinkedInSessionManager.Init(CurrentActivity, BuildScope(), true, () =>
            {
                GetUserProfile(FieldsList);
            }, error =>
            {
                
            });

            return await _loginTcs.Task;
        }

        public void Logout()
        {
            LinkedInSessionManager.ClearSession();
        }

        private static Scope BuildScope()
        {
            return Scope.Build(Scope.RBasicprofile, Scope.REmailaddress);
        }


        public void Dispose()
        {

        }

        public IntPtr Handle { get; }
        public void OnAuthError(LIAuthError result)
        {
            System.Diagnostics.Debug.WriteLine(Tag + ": Connection to the client failed with error <" + result.ToString() + "> ");
        }

        public void OnAuthSuccess()
        {
             GetUserProfile(FieldsList);
        }

        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            LISessionManager.GetInstance(Application.Context).OnActivityResult(CurrentActivity, requestCode, (int) resultCode, data);
        }

        private void GetUserProfile(List<string> fieldsList)
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
            APIHelper.GetInstance(CurrentActivity).GetRequest(CurrentActivity, apiRequestUrl,
                apiResponse =>
                {
                    var linkedInArgs =
                        new LinkedInClientResultEventArgs<string>(apiResponse.ResponseDataAsString, LinkedInActionStatus.Completed, apiResponse.StatusCode.ToString());

                    // Send the result to the receivers
                    _onLogin.Invoke(this, linkedInArgs);
                    _loginTcs.TrySetResult(new LinkedInResponse<string>(linkedInArgs));
                }, 
                error =>
                {
                    //TODO REPLACE OR VERIFY IF THIS IS CORRECT
                    var linkedInArgs =
                        new LinkedInClientResultEventArgs<string>(null, LinkedInActionStatus.Completed, error.ApiErrorResponse.Status.ToString());

                    // Send the result to the receivers
                    _onLogin.Invoke(this, linkedInArgs);
                    _loginTcs.TrySetResult(new LinkedInResponse<string>(linkedInArgs));
                });
        }
    }
}