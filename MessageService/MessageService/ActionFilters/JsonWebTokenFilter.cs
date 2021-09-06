using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MessageService.InfraStructure.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MessageService.ActionFilters
{
    public class JsonWebTokenFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            try
            {
                string token = WebUtility.HtmlDecode(actionContext.ActionArguments["id"].ToString());
                string tokenKey = AppSettings.GetValue("JWTTokenKey");
                string jsonString = JsonWebToken.Decode(token, tokenKey);
                Dictionary<string, object> tokenDictionary = jsonString.ConvertToModel<Dictionary<string, object>>();
                CheckTokenValues(tokenDictionary);
                SetActionArgumentIDValue(tokenDictionary, actionContext.ActionArguments);
            }
            catch
            {
                actionContext.ActionArguments["id"] = null;
            }
        }

        private void CheckTokenValues(Dictionary<string, object> tokenDictionary)
        {
            string[] keys = { "Parameters", "OrgId", "CallBack" };

            foreach (var key in keys)
            {
                if (!tokenDictionary.Keys.Contains(key) || tokenDictionary[key] == null) throw new Exception();
            }
        }

        private void SetActionArgumentIDValue(Dictionary<string, object> tokenDictionary, IDictionary<string, object> parameters)
        {
            parameters["id"] = tokenDictionary["Parameters"];
        }
    }
}
