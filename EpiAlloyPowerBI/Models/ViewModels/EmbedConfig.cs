﻿using System;

namespace EpiAlloyPowerBI.Models.ViewModels
{
    public class EmbedConfig
    {
        public string Id { get; set; }

        public string EmbedUrl { get; set; }

        public string EmbedToken { get; set; }

        //public int MinutesToExpiration
        //{
        //    get
        //    {
        //        var minutesToExpiration = EmbedToken.Expiration.Value - DateTime.UtcNow;
        //        return minutesToExpiration.Minutes;
        //    }
        //}

        public bool? IsEffectiveIdentityRolesRequired { get; set; }

        public bool? IsEffectiveIdentityRequired { get; set; }

        public bool EnableRLS { get; set; }

        public string Username { get; set; }

        public string Roles { get; set; }

        public string ErrorMessage { get; internal set; }
        public string EmbedTokenString { get; set; }
    }
}