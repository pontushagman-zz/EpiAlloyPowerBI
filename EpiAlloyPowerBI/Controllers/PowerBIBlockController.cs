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
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
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
        private static readonly string ReportId = ConfigurationManager.AppSettings["reportId"];
        private static readonly string EmbedUrlBase = ConfigurationManager.AppSettings["embedUrlBase"];

        private static readonly string TokenUrl = ConfigurationManager.AppSettings["tokenUrl"];

        public override ActionResult Index(PowerBIBlock currentBlock)
        {
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

                var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");

                //// ... Target url.
                //string page = "https://api.powerbi.com/v1.0/myorg/groups/135b8d19-49b3-4b6e-a581-ecfb59b5cd13/reports/7dfeedd6-3bc5-4080-9210-73180b5aee61/GenerateToken";

                //// ... Use HttpClient.
                //using (HttpClient client = new HttpClient())
                //using (HttpResponseMessage response = Task.Run(async () => await client.GetAsync(page)).Result)
                //using (HttpContent content = response.Content)
                //{
                //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                //    string httpResult = Task.Run(async () => await content.ReadAsStringAsync()).Result;
                //}



                var restClient = new RestClient("https://api.powerbi.com/v1.0/myorg/groups/135b8d19-49b3-4b6e-a581-ecfb59b5cd13/reports/7dfeedd6-3bc5-4080-9210-73180b5aee61/GenerateToken");
                restClient.AddDefaultHeader("Authorization", string.Format("Bearer {0}", authenticationResult.AccessToken));
                var request = new RestRequest(Method.POST);
                request.AddParameter("accessLevel", "View"); // adds to POST or URL querystring based on Method
                request.AddParameter("allowSaveAs", "false"); // adds to POST or URL querystring based on Method
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

                // execute the request
                IRestResponse response = restClient.Execute(request);
                RestSharp.Deserializers.JsonDeserializer deserial = new JsonDeserializer();

                var embedToken = deserial.Deserialize<EpiAlloyPowerBI.Business.PowerBI.EmbedToken>(response);


                // Create a Power BI Client object. It will be used to call Power BI APIs.
                //using (var client = new PowerBIClient(new Uri(ApiUrl), tokenCredentials))
                //{
                //    // Get report
                //    var reports = Task.Run(async () => await client.Reports.GetReportsInGroupAsync(GroupId)).Result;

                //    Report report;
                //    report = reports.Value.FirstOrDefault(r => r.Id == ReportId);


                //    var datasets = Task.Run(async () => await client.Datasets.GetDatasetByIdInGroupAsync(GroupId, report.DatasetId)).Result;
                //    result.IsEffectiveIdentityRequired = datasets.IsEffectiveIdentityRequired;
                //    result.IsEffectiveIdentityRolesRequired = datasets.IsEffectiveIdentityRolesRequired;
                //    GenerateTokenRequest generateTokenRequestParameters;
                //    generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");

                //    var tokenResponse = Task.Run(async () => await client.Reports.GenerateTokenInGroupAsync(GroupId, report.Id, generateTokenRequestParameters)).Result;

                //    if (tokenResponse == null)
                //    {
                //        result.ErrorMessage = "Failed to generate embed token.";
                //        return PartialView(result);
                //    }

                //    // Generate Embed Configuration.
                //    result.EmbedTokenString = tokenResponse.Token;
                //    result.EmbedUrl = report.EmbedUrl;
                //    result.Id = report.Id;

                //    return PartialView(result);
                //}

                // Generate Embed Configuration.
                result.EmbedTokenString = embedToken.Token;
                result.EmbedUrl = string.Format("{0}/reportEmbed?reportId={1}&groupId={2}", EmbedUrlBase, ReportId, GroupId);  //https://app.powerbi.com/reportEmbed?reportId=7dfeedd6-3bc5-4080-9210-73180b5aee61&groupId=135b8d19-49b3-4b6e-a581-ecfb59b5cd13
                result.Id = ReportId;

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
