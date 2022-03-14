using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace PassportGiris.AuthService
{
    public class AuthService
    {
        private static AuthService _instance;
        public static AuthService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AuthService();
                }
                return _instance;
            }
        }
        private MockStore _mockStore = new MockStore();

        public Guid GetUserId(string username)
        {
            return _mockStore.GetUserId(username);
        }
        public UserAccount GetUserAccount(Guid userId)
        {
            return _mockStore.GetUserAccount(userId);
        }
        public List<UserAccount> GetUserAccountsForDevice(Guid deviceId)
        {
            return _mockStore.GetUserAccountsForDevice(deviceId);
        }
        private AuthService()
        {

        }
        public void Register(string userName)
        {
            _mockStore.AddAccount(userName);
        }
        public bool PassportRemoveUser(Guid userId)
        {
            return _mockStore.RemoveAccount(userId);
        }
        public bool PassportRemoveDevice(Guid userId, Guid deviceId)
        {
            return _mockStore.RemoveDevice(userId, deviceId);
        }

        public void PassportUpdateDetails(Guid userId, Guid deviceId, byte[] publicKey, KeyCredentialAttestationResult keyAttestationResult)
        {
            _mockStore.PassportUpdateDetails(userId, deviceId, publicKey, keyAttestationResult);
        }

        public bool ValidateCredentials(string userName, string password)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                Guid userId = GetUserId(userName);
                if (userId != Guid.Empty)
                {
                    UserAccount account = GetUserAccount(userId);
                    if (account != null)
                    {
                        if (string.Equals(password, account.Password))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public IBuffer PassportRequestChallenge()
        {
            return CryptographicBuffer.ConvertStringToBinary("ServerChallenge", BinaryStringEncoding.Utf8);
        }

        public bool SendeServerSignedChallenge(Guid userId, Guid deviceId, byte[] signedChallenge)
        {
            byte[] userPublicKey = _mockStore.GetPublicKey(userId, deviceId);
            return true;
        }

    }

}
