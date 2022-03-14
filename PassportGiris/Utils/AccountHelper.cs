using PassportGiris.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace PassportGiris.Utils
{
    //AccountHelper, account listesini yerel olarak kaydetme ve yüklemek için gerekli yöntemleri barındıracak.
    public static class AccountHelper
    {
        private const string USER_ACCOUNT_LIST_FILE_NAME = "accountlist.txt";
        private static string _accountListPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, USER_ACCOUNT_LIST_FILE_NAME);
        public static List<Account> AccountList = new List<Account>();

        //Kullanıcı hesabı liste dosyası oluşturma ve kaydetme (eskisini güncelle)
        private static async void SaveAccountListAsync()
        {
            string accountsXml = SerializeAccountListToXml();

            if (File.Exists(_accountListPath))
            {
                StorageFile accountFile = await StorageFile.GetFileFromPathAsync(_accountListPath);
                await FileIO.WriteTextAsync(accountFile, accountsXml);
            }
            else
            {
                StorageFile accountFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(USER_ACCOUNT_LIST_FILE_NAME);
                await FileIO.WriteTextAsync(accountFile, accountsXml);
            }
        }



        //UserAccount liste dosyasını alır ve onu xml'den bir userAccount nesneleri listesi haline getirir.
        public static async Task<List<Account>> LoadAccountListAsync()
        {
            if(File.Exists(_accountListPath))
            {
                StorageFile accountsFile = await StorageFile.GetFileFromPathAsync(_accountListPath);

                string accountsXml = await FileIO.ReadTextAsync(accountsFile);
                DeserializeXmlToAccountList(accountsXml);
            }
            return AccountList;
        }

        //yerel Account listesini kullanır ve listeyi temsil eden XML biçimli bir diziye döndürür.
        public static string SerializeAccountListToXml()
        {
            XmlSerializer xmLizer = new XmlSerializer(typeof(List<Account>));
            StringWriter writer = new StringWriter();
            xmLizer.Serialize(writer, AccountList);

            return writer.ToString();
        }
        //Xml biçimli dizeyi alır ve accountların bir liste nesnesine döndürür.
        public static List<Account> DeserializeXmlToAccountList(string listAsXml)
        {
            XmlSerializer xmLizer = new XmlSerializer(typeof(List<Account>));
            TextReader textReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(listAsXml)));
            return AccountList = (xmLizer.Deserialize(textReader)) as List<Account>;
        }

        public static Account AddAccount(string username)
        {
            //kullanıcı adı ile yeni bir hesap oluştur.
            Account account = new Account() { UserName = username};
            //Accounts listesine ekle
            AccountList.Add(account);
            //Değişiklikleri kaydet ve accountu döndür.
            SaveAccountListAsync();
            return account;
        }

        public static void RemoveAccount(Account account)
        {
            //account listesinden account silme
            AccountList.Remove(account);
            //değişiklikleri kaydet
            SaveAccountListAsync();
        }

        //Gerçek dünyada, bu yöntem, hesabın var olduğunu ve geçerli olduğunu doğrulamak için sunucuyu çağırır.
        //Ancak bu eğitim için sadece "sampleUsername" olan mevcut bir örnek kullanıcımız olacak.
        //Kullanıcı adı boşsa veya "sampleUsername" ile eşleşmiyorsa, doğrulama başarısız olur.
        //Bu durumda kullanıcı yeni bir pasaport kullanıcısı kaydettirmelidir.
        public static bool ValidateAccountCredentials(string username)
        {
            if(string.IsNullOrEmpty(username))
            {
                return false;
            }
            if (!string.Equals(username, "sampleUsername"))
            {
                return false;
            }
            return true;
        }


    }
}
