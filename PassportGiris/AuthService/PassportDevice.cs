using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassportGiris.AuthService
{
    public class PassportDevice
    {
        public Guid DeviceId { get; set; }
        public byte[] PublicKey { get; set; }

       // public KeyCredentialAttestationResult KeyAttestationResult { get; set; }
    }
}
