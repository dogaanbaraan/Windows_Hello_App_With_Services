using PassportGiris.AuthService;
using PassportGiris.Models;
using PassportGiris.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PassportGiris.Utils
{
    //Windows hello'nun kullanıma hazır olup olmadığını kullanıcıya bildirmek için gerekli olan işlemler.

    public static class MicrosoftPassportHelper
    {
        //Pasaport bilgisinin hazır olup olmadığını kontrol et.
        //Pasaport bilgisi;
        // 1- Microsoft hesabıyla bağlı olması
        // 2- Localde tanımladığımız account için ayarlanmış bir Windows PIN'ine sahip olmasına bağlıdır.
      
        public static async Task<bool> MicrosoftPassportAvailableCheckAsync()
        {
            bool keyCredentialAvailable = await KeyCredentialManager.IsSupportedAsync();
            if (keyCredentialAvailable == false)
            {
                //Kullanıcın bir microsof hesabına bağlanması ve bağlantı için bir Pin seçmesi
                //gerektiğinden, anahtar kimlik bilgisi henüz etkinleştirilmemiştir.
                Debug.WriteLine("Microsoft Passport is not setup!\nPlease go to Windows Settings and set up a PIN to use it.");
                return false;
            }
            return true;
        }

        public static async Task<bool> GetPassportAuthenticationMessageAsync (UserAccount account)
        {
            KeyCredentialRetrievalResult openKeyResult = await KeyCredentialManager.OpenAsync(account.UserName);
            // Calling OpenAsync will allow the user access to what is available in the app and will not require user credentials again.
            // If you wanted to force the user to sign in again you can use the following:
            // var consentResult = await Windows.Security.Credentials.UI.UserConsentVerifier.RequestVerificationAsync(account.Username);
            // This will ask for the either the password of the currently signed in Microsoft Account or the PIN used for Microsoft Passport.

            if (openKeyResult.Status == KeyCredentialStatus.Success)
            {
                // If OpenAsync has succeeded, the next thing to think about is whether the client application requires access to backend services.
                // If it does here you would Request a challenge from the Server. The client would sign this challenge and the server
                // would check the signed challenge. If it is correct it would allow the user access to the backend.
                // You would likely make a new method called RequestSignAsync to handle all this
                // for example, RequestSignAsync(openKeyResult);
                // Refer to the second Microsoft Passport sample for information on how to do this.

                // For this sample there is not concept of a server implemented so just return true.
                return true;
            }
          
            else if (openKeyResult.Status == KeyCredentialStatus.NotFound)
            {
                //Bu aşamada _account bulunamazsa iki hatadan kaynaklı olabilir
                //1-Microsoft passportun devre dışı bırakılması
                //2-Microsof passport devre dışı bırakılıp tekrardan eşleştirildi ve bu microsoft passport anahtarının değişmesine sebep oldu.
                //createpassportkeyasync çağırıp ve hesaba girmek, hesapta mevcut olan Microsoft passport keyi değiştirmeye çalışır.
                // hata gerçekten microsoft passportün devre dışı bırakılmasıysa CreatePassportKey yöntemi bu hatayı verir.
                if(await CreatePassportKeyAsync(account.UserId,account.UserName))
                {
                    //Eğer passport key tekrardan başarıyla oluşturulduysa, microsoft passport sıfırlanmış demektir.
                    //Sonuç olarak _account üye olduğunda passport key sıfılanmıştır.
                    return await GetPassportAuthenticationMessageAsync(account);
                }
            }
            //Passportu şu an kullanamazsın tekrar dene.
            return false;
        }

        public static async void RemovePassportAccountAsync(UserAccount account)
        {
            //passport ile accountı aç
            KeyCredentialRetrievalResult keyOpenResult = await KeyCredentialManager.OpenAsync(account.UserName);
            if (keyOpenResult.Status == KeyCredentialStatus.Success)
            {
                //Normalde kaydı silmek için sunucuya anahtar bilgileri gönderilir. For Example: RemovePassportAccountOnServer(account)
                AuthService.AuthService.Instance.PassportRemoveUser(account.UserId);
            }
            //Passport Account listesinden hesabı sil.
            await KeyCredentialManager.DeleteAsync(account.UserName);
        }
        public static void RemovePassportDevice(UserAccount account, Guid deviceId)
        {
            AuthService.AuthService.Instance.PassportRemoveDevice(account.UserId, deviceId);
        }

        public static async Task<bool> CreatePassportKeyAsync(Guid userId, string userName)
        {
            KeyCredentialRetrievalResult keyCreationResult = await KeyCredentialManager.RequestCreateAsync(userName, KeyCredentialCreationOption.ReplaceExisting);

            switch (keyCreationResult.Status)
            {
                case KeyCredentialStatus.Success:
                    Debug.WriteLine("Successfully made key");
                    await GetKeyAttestationAsync(userId, keyCreationResult);
                    return true;

                case KeyCredentialStatus.UserCanceled:
                    Debug.WriteLine("User cancelled sign-in process.");
                    break;

                case KeyCredentialStatus.NotFound:
                    Debug.WriteLine("Microsoft Passport is not setup!\nPlease go to Windows Settings and set up a PIN to use it.");
                    break;
                default:
                    break;
            }
            return false;
        }

        private static async Task GetKeyAttestationAsync(Guid userId, KeyCredentialRetrievalResult keyCreationResult)
        {
            KeyCredential userKey = keyCreationResult.Credential;
            IBuffer publicKey = userKey.RetrievePublicKey();
            KeyCredentialAttestationResult keyAttestationResult = await userKey.GetAttestationAsync();
            IBuffer keyAttestation = null;
            IBuffer certificateChain = null;
            bool keyAttestationIncluded = false;
            bool keyAttestationCanBeRetrievedLater = false;
            KeyCredentialAttestationStatus keyAttestationRetryType = 0;

            if (keyAttestationResult.Status == KeyCredentialAttestationStatus.Success)
            {
                keyAttestationIncluded = true;
                keyAttestation = keyAttestationResult.AttestationBuffer;
                certificateChain = keyAttestationResult.CertificateChainBuffer;
                Debug.WriteLine("Successfully made key and attestation");
            }
            else if (keyAttestationResult.Status == KeyCredentialAttestationStatus.TemporaryFailure)
            {
                keyAttestationRetryType = KeyCredentialAttestationStatus.TemporaryFailure;
                keyAttestationCanBeRetrievedLater = true;
                Debug.WriteLine("Successfully made key but not attestation");
            }
            else if (keyAttestationResult.Status == KeyCredentialAttestationStatus.NotSupported)
            {
                keyAttestationRetryType = KeyCredentialAttestationStatus.NotSupported;
                keyAttestationCanBeRetrievedLater = false;
                Debug.WriteLine("Key created, but key attestation not supported");
            }

            Guid deviceId = Helpers.GetDeviceId();
            //Update the Pasport details with the information we have just gotten above.
            UpdatePassportDetails(userId, deviceId, publicKey.ToArray(), keyAttestationResult);
        }

        public static bool UpdatePassportDetails(Guid userId, Guid deviceId, byte[] publicKey, KeyCredentialAttestationResult keyAttestationResult)
        {
            //In the real world you would use an API to add Passport signing info to server for the signed in _account.
            //For this tutorial we do not implement a WebAPI for our server and simply mock the server locally 
            //The CreatePassportKey method handles adding the Windows Hello account locally to the device using the KeyCredential Manager

            //Using the userId the existing account should be found and updated.
            AuthService.AuthService.Instance.PassportUpdateDetails(userId, deviceId, publicKey, keyAttestationResult);
            return true;
        }

    }
}
