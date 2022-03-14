using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassportGiris.AuthService
{
    public class UserAccount
    {
        [Key, Required]
        public Guid UserId { get; set; } //Guid: birbirine benzemeyen benzersiz değerler oluşturmak için kullanılır. Açılımı "Globally Unique Identifier."
        [Required]
        public string UserName { get; set; }
        public string Password { get; set; }

        public List<PassportDevice> PassportDevices = new List<PassportDevice>();
    }
}
