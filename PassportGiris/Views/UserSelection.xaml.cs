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
    public sealed partial class UserSelection : Page
    {
        public UserSelection()
        {
            this.InitializeComponent();
            Loaded += UserSelection_Loaded;
        }

        private void UserSelection_Loaded(object sender, RoutedEventArgs e)
        {
            List<UserAccount> accounts = AuthService.AuthService.Instance.GetUserAccountsForDevice(Helpers.GetDeviceId());
            if(accounts.Any())
            {
                UserListView.ItemsSource = accounts;
                UserListView.SelectionChanged += UserSelectionChanged;
            }
            else
            {
                Frame.Navigate(typeof(Login));
            }
            ////Eğer hesap bulunmuyorsa Giriş sayfasına yönlendirir.
            //if(AccountHelper.AccountList.Count == 0)
            //{
            //    Frame.Navigate(typeof(Login));
            //}

            //UserListView.ItemsSource = AccountHelper.AccountList;
            //UserListView.SelectionChanged += UserSelection_Changed;
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Kullanıcı seçme ekranına göndermeden önce yerel account listesini yükler.
            await AccountHelper.LoadAccountListAsync();
            Frame.Navigate(typeof(UserSelection));
        }

        private void UserSelectionChanged(object sender, RoutedEventArgs e)
        {
            if(((ListView)sender).SelectedValue != null)
            {
                UserAccount account = (UserAccount)((ListView)sender).SelectedValue;
                if(account != null)
                {
                    Debug.WriteLine("Account" + account.UserName + " selected!");
                }
                Frame.Navigate(typeof(Login), account);
            }
        }

        //Account listesinden bir account seçildiğinde fonksiyonu çağırır ve login sayfasına seçilen hesap ile geçiş yapar.
        private void UserSelection_Changed(object sender, RoutedEventArgs e)
        {
            if(((ListView)sender).SelectedValue != null)
            {
                Account account = (Account)((ListView)sender).SelectedValue;
                if(account != null)
                {
                    Debug.WriteLine("Account " + account.UserName + " selected!");
                }
                Frame.Navigate(typeof(Login), account);
            }
        }

        //Butonun işlevi yeni bir kullanıcı eklemektir. Login sayfasına içi boş bir şekilde döndürürülür.
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Login));
        }
    }
}
