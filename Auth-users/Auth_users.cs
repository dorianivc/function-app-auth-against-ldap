using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.DirectoryServices;
using System;

namespace Auth_users
{
    
    public static class Auth_users
    {
        [FunctionName("Auth_users")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string username = req.Query["username"];
            string password = req.Query["password"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            username = username ?? data?.username;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)){
                string responseMessage = "Parameters Missing";
                return new OkObjectResult(responseMessage);
            }
            
            

            bool authenticated = false;

            try
            {
                LdapDirectoryIdentifier ldi = new LdapDirectoryIdentifier(Environment.GetEnvironmentVariable("LDAP_SERVER"), 389);
                System.DirectoryServices.Protocols.LdapConnection ldapConnection =
                    new System.DirectoryServices.Protocols.LdapConnection(ldi);
                Console.WriteLine("LdapConnection is created successfully.");
                ldapConnection.AuthType = AuthType.Basic;
                ldapConnection.SessionOptions.ProtocolVersion = 3;
                NetworkCredential nc = new NetworkCredential("uid="+username+",ou=people,dc=eastus,dc=cloudapp,dc=azure,dc=com",
                    password); 
                ldapConnection.Bind(nc);
                Console.WriteLine("LdapConnection authentication success");
                ldapConnection.Dispose();
                authenticated = true;
                
            }
            catch (DirectoryServicesCOMException cex)
            {

                log.LogInformation(cex.ToString());
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.ToString());
            }



            if( authenticated != true){
                string Message = "USER NOT AUTHENTICATED";
                return new OkObjectResult(Message);
            }
            else
            {
                string Message = "User is Auth in this organization unit";
                return new OkObjectResult(Message);
            }
            
        
            
        }

        
       
          
        
    }
}
