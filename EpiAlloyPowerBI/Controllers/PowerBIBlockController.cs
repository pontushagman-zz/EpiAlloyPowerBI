using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using EpiAlloyPowerBI.Business.PowerBI;
using EpiAlloyPowerBI.Models.Blocks;
using EpiAlloyPowerBI.Models.ViewModels;
using EPiServer.Web.Mvc;
using Microsoft.Rest;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using RestSharp;
using RestSharp.Deserializers;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace EpiAlloyPowerBI.Controllers
{
    public class PowerBIBlockController : BlockController<PowerBIBlock>
    {
        private static readonly string Username = ConfigurationManager.AppSettings["pbiUsername"];
        private static readonly string Password = ConfigurationManager.AppSettings["pbiPassword"];
        private static readonly string AuthorityUrl = ConfigurationManager.AppSettings["authorityUrl"];
        private static readonly string ResourceUrl = ConfigurationManager.AppSettings["resourceUrl"];
        private static readonly string ClientId = ConfigurationManager.AppSettings["clientId"];
        private static readonly string ApiUrl = ConfigurationManager.AppSettings["apiUrl"];
        private static readonly string GroupId = ConfigurationManager.AppSettings["groupId"];
        private static readonly string EmbedUrlBase = ConfigurationManager.AppSettings["embedUrlBase"];

        private string _reportId = ConfigurationManager.AppSettings["reportId"];


        public override ActionResult Index(PowerBIBlock currentBlock)
        {
            if (!string.IsNullOrEmpty(currentBlock.ReportId))
            {
                _reportId = currentBlock.ReportId;
            }

            var result = new EmbedConfig();
            try
            {
                result = new EmbedConfig();
                var error = GetWebConfigErrors();
                if (error != null)
                {
                    result.ErrorMessage = error;
                    return PartialView(result);
                }

                // Create a user password cradentials.
                var credential = new UserPasswordCredential(Username, Password);

                //// Authenticate using created credentials
                var authenticationContext = new AuthenticationContext(AuthorityUrl);
                var authenticationResult =
                    Task.Run(async () => await authenticationContext.AcquireTokenAsync(ResourceUrl, ClientId, credential)).Result;


                if (authenticationResult == null)
                {
                    result.ErrorMessage = "Authentication Failed.";
                    return PartialView(result);
                }
                
                //Request embed token
                var requestEmbedTokenUrl = string.Format("{0}/v1.0/myorg/groups/{1}/reports/{2}/GenerateToken", ApiUrl,GroupId, _reportId);
                var restClient = new RestClient(requestEmbedTokenUrl);
                restClient.AddDefaultHeader("Authorization", string.Format("Bearer {0}", authenticationResult.AccessToken));
                var request = new RestRequest(Method.POST);
                request.AddParameter("accessLevel", "View"); 
                request.AddParameter("allowSaveAs", "false"); 
                // execute the request
                IRestResponse response = restClient.Execute(request);
                RestSharp.Deserializers.JsonDeserializer deserial = new JsonDeserializer();

                var embedToken = deserial.Deserialize<EpiAlloyPowerBI.Business.PowerBI.EmbedToken>(response);

                // Generate Embed Configuration.
                result.EmbedToken = embedToken;
                result.EmbedUrl = string.Format("{0}/reportEmbed?reportId={1}&groupId={2}", EmbedUrlBase, _reportId, GroupId);  
                result.Id = _reportId;

                return PartialView(result); 
            }
            catch (HttpOperationException exc)
            {
                result.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}", exc.Response.StatusCode, (int)exc.Response.StatusCode, exc.Response.Content, exc.Response.Headers["RequestId"].FirstOrDefault());
            }
            catch (Exception exc)
            {
                result.ErrorMessage = exc.ToString();
            }

            return PartialView(result);
        }


        /// <summary>
        /// Check if web.config embed parameters have valid values.
        /// </summary>
        /// <returns>Null if web.config parameters are valid, otherwise returns specific error string.</returns>
        private string GetWebConfigErrors()
        {
            // Client Id must have a value.
            if (string.IsNullOrEmpty(ClientId))
            {
                return "ClientId is empty. please register your application as Native app in https://dev.powerbi.com/apps and fill client Id in web.config.";
            }

            // Client Id must be a Guid object.
            Guid result;
            if (!Guid.TryParse(ClientId, out result))
            {
                return "ClientId must be a Guid object. please register your application as Native app in https://dev.powerbi.com/apps and fill client Id in web.config.";
            }

            // Group Id must have a value.
            if (string.IsNullOrEmpty(GroupId))
            {
                return "GroupId is empty. Please select a group you own and fill its Id in web.config";
            }

            // Group Id must be a Guid object.
            if (!Guid.TryParse(GroupId, out result))
            {
                return "GroupId must be a Guid object. Please select a group you own and fill its Id in web.config";
            }

            // Username must have a value.
            if (string.IsNullOrEmpty(Username))
            {
                return "Username is empty. Please fill Power BI username in web.config";
            }

            // Password must have a value.
            if (string.IsNullOrEmpty(Password))
            {
                return "Password is empty. Please fill password of Power BI username in web.config";
            }

            return null;
        }



    }
}
