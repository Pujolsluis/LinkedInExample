using System;
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

        static TaskCompletionSource<LinkedInResponse<string>> _loginTcs;

        public static LinkedInClientManager ManagerInstance { get; } = new LinkedInClientManager();

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
        


        public async Task<LinkedInResponse<string>> LoginAsync()
        {
            _loginTcs = new TaskCompletionSource<LinkedInResponse<string>>();
            LinkedInSessionManager.Init(CurrentActivity, BuildScope(), ManagerInstance, false);

            return await _loginTcs.Task;
        }

        public void Logout()
        {

        }

        private static Scope BuildScope()
        {
            return Scope.Build(Scope.RBasicprofile);
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
             GetUserProfile();
        }

        //TODO ASK RENDY ABOUT TO CALLING CHANGING THIS Instead of doing directly in the activity?
        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            ManagerInstance.OnActivityResult(requestCode, resultCode, data);
        }

        private void GetUserProfile()
        {
            var apiRequestUrl =
                "https://api.linkedin.com/v1/people/~:(id,first-name,last-name,num-connections,picture-url,location,positions)?format=json";
            APIHelper.GetInstance(CurrentActivity).GetRequest(CurrentActivity, apiRequestUrl,
                apiResponse =>
                {
                    var linkedInArgs =
                        new LinkedInClientResultEventArgs<string>(apiResponse.ResponseDataAsString, LinkedInActionStatus.Completed, apiResponse.StatusCode.ToString());

                    // Send the result to the receivers
                    _onLogin.Invoke(ManagerInstance, linkedInArgs);
                    _loginTcs.TrySetResult(new LinkedInResponse<string>(linkedInArgs));
                }, 
                error =>
                {
                    //TODO REPLACE OR VERIFY IF THIS IS CORRECT
                    var linkedInArgs =
                        new LinkedInClientResultEventArgs<string>(error.ApiErrorResponse.Message, LinkedInActionStatus.Completed, error.ApiErrorResponse.Status.ToString());

                    // Send the result to the receivers
                    _onLogin.Invoke(ManagerInstance, linkedInArgs);
                    _loginTcs.TrySetResult(new LinkedInResponse<string>(linkedInArgs));
                });
        }
    }
}