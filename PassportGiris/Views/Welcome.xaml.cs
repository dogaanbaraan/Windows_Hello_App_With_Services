using PassportGiris.AuthService;
using PassportGiris.Models;
using PassportGiris.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class Welcome : Page
    {
        private UserAccount _activeAccount;
        public Welcome()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _activeAccount = (UserAccount)e.Parameter;
            if(this._activeAccount != null)
            {
                UserAccount account = AuthService.AuthService.Instance.GetUserAccount(_activeAccount.UserId);
                if(account != null)
                {
                    UserListView.ItemsSource = account.PassportDevices;
                    UserNameText.Text = account.UserName;
                }
            }
        }

        private void Button_Restart_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserSelection));
        }

        private void Button_Forget_User_Click(object sender, RoutedEventArgs e)
        {
            //Microsoft passport'dan hesabı siler
            MicrosoftPassportHelper.RemovePassportAccountAsync(_activeAccount);

            //yerel account listesinden sil ve listeyi güncelle.
            Debug.WriteLine("User " + _activeAccount.UserName + "deleted.");

            //UserSelection ekranına geri dönüş yapar.
            Frame.Navigate(typeof(UserSelection));
        }

        private void Button_Forget_Device_Click(object sender, RoutedEventArgs e)
        {
            PassportDevice selectedDevice = UserListView.SelectedItem as PassportDevice;
            if(selectedDevice != null)
            {
                //Windows Hellodan sil
                MicrosoftPassportHelper.RemovePassportDevice(_activeAccount, selectedDevice.DeviceId);
                Debug.WriteLine("User" + _activeAccount.UserName + " deleted.");

                if(!UserListView.Items.Any())
                {
                    Frame.Navigate(typeof(UserSelection));
                }
            }
            else
            {
                ForgetDeviceErrorTextBlock.Visibility = Visibility.Visible;
            }
        }
    }
}
