using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using LinkedIn.Models;
using Newtonsoft.Json.Linq;
using Plugin.LinkedInClient;
using Plugin.LinkedInClient.Shared;
using Xamarin.Forms;

namespace LinkedIn.ViewModels
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        public LinkedInUser user { get; set; } = new LinkedInUser();
        public event PropertyChangedEventHandler PropertyChanged;
        public ILinkedInClientManager LinkedInClientManager;

        public string Name
        {
            get { return user.Name; }
            set { user.Name = value; }
        }

        public string Email
        {
            get { return user.Email; }
            set { user.Email = value; }
        }

        public string Picture
        {
            get { return user.Picture; }
            set { user.Picture = value; }
        }

        public bool IsLoggedIn { get; set; }

        public ICommand LoginCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        public LoginPageViewModel()
        {
            LoginCommand = new Command(LoginAsync);
            LogoutCommand = new Command(Logout);
            IsLoggedIn = false;

            LinkedInClientManager = CrossLinkedInClient.Current;
        }

        public async void LoginAsync()
        {
            LinkedInClientManager.OnLogin += OnLoginCompleted;
			LinkedInClientManager.OnError += OnAuthError;
            List<string> fieldsList = new List<string> {"first-name", "last-name", "email-address", "picture-url"};
			try
			{
				await LinkedInClientManager.LoginAsync();
			}
			catch (LinkedInClientBaseException exception)
			{
				await App.Current.MainPage.DisplayAlert("Error", exception.Message, "OK");
				LinkedInClientManager.OnLogin -= OnLoginCompleted;
				LinkedInClientManager.OnError -= OnAuthError;
			}

			try
			{
				LinkedInClientManager.OnGetUserProfile += OnGetProfile;
				LinkedInClientManager.OnError += OnAuthError;
				var profile = await LinkedInClientManager.GetUserProfile(fieldsList);

			}
			catch (LinkedInClientApiHelperErrorException e) 
			{
				
			}
			finally
			{
				LinkedInClientManager.OnGetUserProfile += OnGetProfile;
                LinkedInClientManager.OnError += OnAuthError;
			}
        }

		private void OnGetProfile(object sender, LinkedInClientResultEventArgs<string> e)
		{
			if (e.Data != null)
			{
				var data = JObject.Parse(e.Data);
				user.Picture = data["pictureUrl"].ToString();
				user.Email = data["emailAddress"].ToString();	
			}
		}

		private void OnAuthError(object sender, LinkedInClientErrorEventArgs e)
		{
			App.Current.MainPage.DisplayAlert("Error", e.Message, "OK");
		}

		private void OnLoginCompleted(object sender, LinkedInClientResultEventArgs<string> linkedInClientResultEventArgs)
        {
            if (linkedInClientResultEventArgs.Data != null)
            {
                Debug.WriteLine("JSON RESPONSE: " + linkedInClientResultEventArgs.Data);
                var data = JObject.Parse(linkedInClientResultEventArgs.Data);

                user.Name = data["firstName"] + " " + data["lastName"];
				//List<string> fieldList = new List<string>();
				//fieldList.Add("pictureUrl");
				//CrossLinkedInClient.Current.GetUserProfile(fieldList);
                //user.Email = data["emailAddress"].ToString();
                //user.Picture = new Uri(data["pictureUrl"].ToString());
               // App.Current.MainPage.DisplayAlert("Success", "It works!", "OK");
                IsLoggedIn = true;
            }
            else
            {
                App.Current.MainPage.DisplayAlert("Error", linkedInClientResultEventArgs.Message, "OK");
            }
            LinkedInClientManager.OnLogin -= OnLoginCompleted;
			LinkedInClientManager.OnError -= OnAuthError;
        }

        public void Logout()
        {
            LinkedInClientManager.OnLogout += OnLogoutCompleted;
            LinkedInClientManager.Logout();
        }

        private void OnLogoutCompleted(object sender, EventArgs loginEventArgs)
        {
            IsLoggedIn = false;
            LinkedInClientManager.OnLogout -= OnLogoutCompleted;
        }

    }
}

