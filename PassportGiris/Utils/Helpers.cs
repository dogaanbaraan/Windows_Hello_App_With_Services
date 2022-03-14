using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace PassportGiris.Utils
{
    public static class Helpers
    {
        public static Guid GetDeviceId()
        {
            EasClientDeviceInformation deviceInformation = new EasClientDeviceInformation();
            return deviceInformation.Id;
        }
    }
}
