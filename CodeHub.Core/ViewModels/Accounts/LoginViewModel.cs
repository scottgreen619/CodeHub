using System;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using System.Threading.Tasks;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class LoginViewModel : BaseViewModel
    {
        public const string ClientId = "72f4fb74bdba774b759d";
        public const string ClientSecret = "9253ab615f8c00738fff5d1c665ca81e581875cb";
        public static readonly string RedirectUri = "http://dillonbuchanan.com/";
        private readonly ILoginFactory _loginFactory;
        private readonly IFeaturesService _featuresService;

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            set
            {
                _isLoggingIn = value;
                RaisePropertyChanged(() => IsLoggingIn);
            }
        }

        public string LoginUrl
        {
            get
            {
                var web = WebDomain.TrimEnd('/');
                return string.Format(
                    web + "/login/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}", 
                    LoginViewModel.ClientId, 
                    Uri.EscapeDataString(LoginViewModel.RedirectUri),
                    Uri.EscapeDataString("user,repo,notifications,gist"));
            }
        }

        public GitHubAccount AttemptedAccount { get; private set; }

        public string WebDomain { get; set; }

        public ICommand GoBackCommand
        {
            get { return new MvxCommand(() => ChangePresentation(new MvxClosePresentationHint(this))); }
        }

        public LoginViewModel(ILoginFactory loginFactory, IFeaturesService featuresService)
        {
            _loginFactory = loginFactory;
            _featuresService = featuresService;
        }

        public void Init(NavObject navObject)
        {
            WebDomain = navObject.WebDomain ?? GitHubSharp.Client.AccessTokenUri;

            if (navObject.AttemptedAccountId >= 0)
            {
                AttemptedAccount = this.GetApplication().Accounts.Find(navObject.AttemptedAccountId);
            }
        }

        public async Task Login(string code)
        {
            LoginData loginData = null;

            try
            {
                IsLoggingIn = true;
                loginData = await _loginFactory.LoginWithToken(ClientId, ClientSecret, code, RedirectUri, WebDomain, GitHubSharp.Client.DefaultApi);
            }
            catch (Exception e)
            {
                DisplayAlert(e.Message);
                return;
            }
            finally
            {
                IsLoggingIn = false;
            }

            this.GetApplication().ActivateUser(loginData.Account, loginData.Client);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string WebDomain { get; set; }
            public int AttemptedAccountId { get; set; }

            public NavObject()
            {
                AttemptedAccountId = int.MinValue;
            }

            public static NavObject CreateDontRemember(GitHubAccount account)
            {
                return new NavObject
                { 
                    WebDomain = account.WebDomain, 
                    Username = account.Username,
                    AttemptedAccountId = account.Id
                };
            }
        }
    }
}

