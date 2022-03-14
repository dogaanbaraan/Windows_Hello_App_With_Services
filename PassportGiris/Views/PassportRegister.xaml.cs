using PassportGiris.Models;
using PassportGiris.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class PassportRegister : Page
    {
        public PassportRegister()
        {
            this.InitializeComponent();
        }
        
        private async void RegisterButton_Click_Async(object sender, RoutedEventArgs e)
        {

            ErrorMessage.Text = "";

            if(!string.IsNullOrEmpty(UsernameTextBox.Text))
            {
                AuthService.AuthService.Instance.Register(UsernameTextBox.Text);
                Guid userId= AuthService.AuthService.Instance.GetUserId(UsernameTextBox.Text);

                if(userId != Guid.Empty)
                {
                    bool isSuccesful = await MicrosoftPassportHelper.CreatePassportKeyAsync(userId, UsernameTextBox.Text);
                    if(isSuccesful)
                    {
                        Frame.Navigate(typeof(Welcome), AuthService.AuthService.Instance.GetUserAccount(userId));
                    }
                    else
                    {
                        AuthService.AuthService.Instance.PassportRemoveUser(userId);
                        ErrorMessage.Text = "Account Creation Failed";
                    }
                }
            }
            else
            {
                ErrorMessage.Text = "Please enter a username";
            }
            //if(!string.IsNullOrEmpty(UsernameTextBox.Text))
            //{
            //    _account = AccountHelper.AddAccount(UsernameTextBox.Text);
            //    await MicrosoftPassportHelper.CreatePassportKeyAsync(_account.UserName);
            //    Frame.Navigate(typeof(Welcome), _account);
            //}
            //else
            //{
            //    ErrorMessage.Text = "Please enter a username";
            //}

        }
    }
}
