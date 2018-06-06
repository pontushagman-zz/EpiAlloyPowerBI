using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestSharp;

namespace EpiAlloyPowerBI.Business.PowerBI
{
    public class EmbedToken
    {
        public string Token { get; set; }
        public string Expiration { get; set; }
    }
}