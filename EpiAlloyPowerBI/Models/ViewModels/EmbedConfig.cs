using System;
using EpiAlloyPowerBI.Business.PowerBI;

namespace EpiAlloyPowerBI.Models.ViewModels
{
    public class EmbedConfig
    {
        public string Id { get; set; }

        public string EmbedUrl { get; set; }

        public EmbedToken EmbedToken { get; set; }

        public int MinutesToExpiration
        {
            get
            {
                var minutesToExpiration = EmbedToken.Expiration - DateTime.UtcNow;
                return minutesToExpiration.Minutes;
            }
        }


        public bool? IsEffectiveIdentityRolesRequired { get; set; }
    
        public bool? IsEffectiveIdentityRequired { get; set; }

        public string Username { get; set; }

        public string Roles { get; set; }

        public string ErrorMessage { get; internal set; }
    }
}