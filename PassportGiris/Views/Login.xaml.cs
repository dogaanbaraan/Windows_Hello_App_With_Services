using PassportGiris.AuthService;
using PassportGiris.Models;
using PassportGiris.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PassportGiris.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        private UserAccount _userAccount;
        private bool _isExistingAccount;
        public Login()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //Login sayfasında makinede Windows Hello olup olmadığını doğrulamak için OnNavigatedTo
            //olayını işlememiz gerekir.

            //Microsoft pasaportun kurulu ve kullanılabilir olduğunu kontrol etme
            if (await MicrosoftPassportHelper.MicrosoftPassportAvailableCheckAsync())
            {
                if(e.Parameter != null)
                {
                    _isExistingAccount = true;
                    //Girilen hesabı, mevcut hesaba atama işlemi.
                    _userAccount = (UserAccount)e.Parameter;    
                    UserNameTextBox.Text = _userAccount.UserName;
                    SignInPassport();
                }
            }
            //Yüklü bir microsoft pasaport bulunamadığından kullanıyı bilgilendirme
            else
            {
                PassportStatus.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 50, 170, 207));
                PassportStatusText.Text = "Microsoft Passport is not setup!\n" + "Please go to windows settings and set up a PIN to use it.";
                PassportSignInButton.IsEnabled = false;
            }

        }
        private void PassportSignInButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Text = "";
            SignInPassport();
        }

        private void RegisterButtonTextBlock_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ErrorMessage.Text = "";
            Frame.Navigate(typeof(PassportRegister));
        }

        private async void SignInPassport()
        {
            if(_isExistingAccount)
            {
                if(await MicrosoftPassportHelper.GetPassportAuthenticationMessageAsync(_userAccount))
                {
                    Frame.Navigate(typeof(Welcome), _userAccount);
                }
            }

            else if(AuthService.AuthService.Instance.ValidateCredentials(UserNameTextBox.Text, PasswordBox.Password))
            {
                Guid userId = AuthService.AuthService.Instance.GetUserId(UserNameTextBox.Text);
                if(userId != Guid.Empty)
                {
                    bool isSuccesfull = await MicrosoftPassportHelper.CreatePassportKeyAsync(userId, UserNameTextBox.Text);
                    if(isSuccesfull)
                    {
                        Debug.WriteLine("Succesfully signed in with Windows Hello!");
                        _userAccount = AuthService.AuthService.Instance.GetUserAccount(userId);
                        Frame.Navigate(typeof(Welcome), _userAccount);  
                    }

                    else
                    {
                        AuthService.AuthService.Instance.PassportRemoveUser(userId);
                        ErrorMessage.Text = "Account Creation Failed";
                    }
                }
                //yeni bir local hesap ekle ve oluştur.
                //_account = AccountHelper.AddAccount(UserNameTextBox.Text);
                //Debug.WriteLine("Succesfully signed in with traditional credentials and created local account instance!");
                //if (await MicrosoftPassportHelper.CreatePassportKeyAsync(UserNameTextBox.Text))
                //{
                //    Debug.WriteLine("Successfully signed in with Microsoft Passport!");
                //    Frame.Navigate(typeof(Welcome), _userAccount);
                //}
            }
            
            else
            {
                ErrorMessage.Text = "Invalid Credentials";
            }
        }

        
    }
}
