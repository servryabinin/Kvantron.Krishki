using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrishkiForms.Authorization
{
    public class AuthConfig
    {
        public Dictionary<string, RoleInfo> Roles { get; set; }
        public Dictionary<string, int> AutoLogoutSeconds { get; set; }
    }
}
