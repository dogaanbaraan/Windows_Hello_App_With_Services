using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Security.Credentials;
using Windows.Storage;

namespace PassportGiris.AuthService
{
    public class MockStore
    {
        private const string USER_ACCOUNT_LIST_FILE_NAME = "userAccountsList.txt";

        //LocalFolder const olamaz çünkü local foldera çalışma anında erişilir.
        private string _userAccountListPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, USER_ACCOUNT_LIST_FILE_NAME);
        private List<UserAccount> _mockDatabaseUserAccountsList;
        public MockStore()
        {
            //_mockDatabaseUserAccountsList = new List& lt; UserAccount & gt; ();
            _mockDatabaseUserAccountsList = new List<UserAccount>();//kodda sorun var!!!!!!
            LoadAccountListAsync();
        }

        private void InitializeSampleUserAccounts()
        {
            // Create a sample Traditional User Account that only has a Username and Password
            // This will be used initially to demonstrate how to migrate to use Windows Hello

            //Sadece kullanıcı adı ve şifresi olan örnek bir Traditional Kullanıcı hesabı oluştur.
            //Bu başlangıçta windows helloya nasıl geçiş yapılacağını göstermek için kullanılıcaktır.

            UserAccount sampleUserAccount = new UserAccount()
            {
                UserId = Guid.NewGuid(),
                UserName = "sampleUsername",
                Password = "samplePassword",
            };

            // Add the sampleUserAccount to the _mockDatabase
            //oluşturulan accountı database'e ekler.
            _mockDatabaseUserAccountsList.Add(sampleUserAccount);
            SaveAccountListAsync();
        }

        public Guid GetUserId(string userName)
        {
            if(_mockDatabaseUserAccountsList.Any())
            {
                UserAccount account = _mockDatabaseUserAccountsList.FirstOrDefault(f => f.UserName.Equals(userName));
                if(account != null)
                {
                    return account.UserId;
                }
            }

            return Guid.Empty;  
        }
        public UserAccount GetUserAccount(Guid userId)
        {
            return _mockDatabaseUserAccountsList.FirstOrDefault(f => f.UserId.Equals(userId));
        }

        public List<UserAccount> GetUserAccountsForDevice(Guid deviceId)
        {
            List<UserAccount> usersForDevice = new List<UserAccount>();
            foreach (UserAccount account in _mockDatabaseUserAccountsList)
            {
                if(account.PassportDevices.Any(f=>f.DeviceId.Equals(deviceId)))
                {
                    usersForDevice.Add(account);
                }

            }
            return usersForDevice;
        }

        public byte[] GetPublicKey(Guid userId, Guid deviceId)
        {
            UserAccount account = _mockDatabaseUserAccountsList.FirstOrDefault(f => f.UserId.Equals(userId));
            if(account!=null)
            {
                if(account.PassportDevices.Any())
                {
                    return account.PassportDevices.FirstOrDefault(f => f.DeviceId.Equals(deviceId)).PublicKey;
                }
            }
            return null;
        }

        public UserAccount AddAccount(string userName)
        {
            UserAccount newAccount = null;
            try
            {
                newAccount = new UserAccount()
                {
                    UserId = Guid.NewGuid(),
                    UserName = userName,
                };
                _mockDatabaseUserAccountsList.Add(newAccount);
                SaveAccountListAsync();
            }
            catch (Exception)
            {

                throw;
            }
            return newAccount;
        }

        public bool RemoveAccount(Guid userId)
        {
            UserAccount userAccount = GetUserAccount(userId);
            if(userAccount!=null)
            {
                _mockDatabaseUserAccountsList.Remove(userAccount);
                SaveAccountListAsync();
            }
            return false;
        }

        public bool RemoveDevice(Guid userId, Guid deviceId)
        {
            UserAccount userAccount = GetUserAccount(userId);
            PassportDevice deviceToRemove = null;
            if(userAccount!=null)
            {
                foreach (PassportDevice device in userAccount.PassportDevices)
                {
                    if(device.DeviceId.Equals(deviceId))
                    {
                        deviceToRemove = device;
                        break;
                    }
                }
            }
            if(deviceToRemove!=null)
            {
                userAccount.PassportDevices.Remove(deviceToRemove);
                SaveAccountListAsync();
            }

            return true;
        }

        public void PassportUpdateDetails(Guid userId, Guid deviceId, byte[] publicKey, KeyCredentialAttestationResult keyAttestationResult)
        {
            UserAccount existingUserAccount = GetUserAccount(userId);
            if(existingUserAccount !=null)
            {
                if (!existingUserAccount.PassportDevices.Any(f => f.DeviceId.Equals(deviceId)))
                {
                    existingUserAccount.PassportDevices.Add(new PassportDevice()
                    {
                        DeviceId = deviceId,
                        PublicKey = publicKey,
                        //KeyAttestationResult = keyAttestationResult
                    });
                }
            }
            SaveAccountListAsync();
        }

        #region kaydetme ve yükleme yardımcıları
        //Bir userAccount liste dosyası oluşturun ve kaydedin (eskini değiştiren)
        private async void SaveAccountListAsync()
        {
            string accountsXml = SerializeAccountListToXml();
            if(File.Exists(_userAccountListPath))
            {
                StorageFile accountsFile = await StorageFile.GetFileFromPathAsync(_userAccountListPath);
                await FileIO.WriteTextAsync(accountsFile, accountsXml);
            }
            else
            {
                StorageFile accountsFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(USER_ACCOUNT_LIST_FILE_NAME);
                await FileIO.WriteTextAsync(accountsFile, accountsXml);
            }

        }
        //userAccount Listesi dosyasını alır ve userAccount listesindeki nesneleri XML ile seri hale getirir.
        private async void LoadAccountListAsync()
        {
            if(File.Exists(_userAccountListPath))
            {
                StorageFile accountsFile = await StorageFile.GetFileFromPathAsync(_userAccountListPath);

                string accountsXml = await FileIO.ReadTextAsync(accountsFile);
                DeserializeXmlToAccountList(accountsXml);
            }
            //UserAccountList sapmleUser'ı içermiyorsa sample usersları başlat
            //Bu, Hand of Labda olduğu için, bir kullanıcının geçiş yaptığını göstermek için gereklidir.
            //Normalde kullanıcı hesapları sadece veritabanında olması gerekirdi.
            if(!_mockDatabaseUserAccountsList.Any(f=>f.UserName.Equals("sampleUsername")))
            {
                
                InitializeSampleUserAccounts();
            }
        }

        //User Account Listesini alır ve listeyi temsil eden XML biçimli bir dize dönüştürür.
        private string SerializeAccountListToXml()
        {
            XmlSerializer xmlizer = new XmlSerializer(typeof(List<UserAccount>));
            StringWriter writer = new StringWriter();
            xmlizer.Serialize(writer, _mockDatabaseUserAccountsList);
            return writer.ToString();
        }

        //User accounts listesini temsil eden Xml dizesini alır ve accountların nesnelerini liste haline çevirir.
        private List<UserAccount> DeserializeXmlToAccountList(string listAsXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<UserAccount>));
            TextReader textReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(listAsXml)));
            return _mockDatabaseUserAccountsList = (xmlSerializer.Deserialize(textReader)) as List<UserAccount>;
        }
        #endregion
    }
}
