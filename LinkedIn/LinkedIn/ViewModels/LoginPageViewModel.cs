using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using LinkedIn.Models;
using LinkedIn.Services;
using Newtonsoft.Json.Linq;
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

        public Uri Picture
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

            LinkedInClientManager = DependencyService.Get<ILinkedInClientManager>();
        }

        public void LoginAsync()
        {
            LinkedInClientManager.OnLogin += OnLoginCompleted;
            List<string> fieldsList = new List<string> {"first-name", "last-name", "email-address", "picture-url"};
            LinkedInClientManager.LoginAsync(fieldsList);
        }

        private void OnLoginCompleted(object sender, LinkedInClientResultEventArgs<string> linkedInClientResultEventArgs)
        {
            if (linkedInClientResultEventArgs.Data != null)
            {
                Debug.WriteLine("JSON RESPONSE: " + linkedInClientResultEventArgs.Data);
                var data = JObject.Parse(linkedInClientResultEventArgs.Data);

                user.Name = data["firstName"] + " " + data["lastName"];
                user.Email = data["emailAddress"].ToString();
                user.Picture = new Uri(data["pictureUrl"].ToString());
               // App.Current.MainPage.DisplayAlert("Success", "It works!", "OK");
                IsLoggedIn = true;
            }
            else
            {
                App.Current.MainPage.DisplayAlert("Error", linkedInClientResultEventArgs.Message, "OK");
            }
            LinkedInClientManager.OnLogin -= OnLoginCompleted;
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

