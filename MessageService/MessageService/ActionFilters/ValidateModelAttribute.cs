using System.Collections.Generic;
using MessageService.InfraStructure.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MessageService.ActionFilters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                List<string> errors = GetErrors(actionContext);

                var response = new { errors = errors };

                actionContext.Result = new JsonResult(APIResponse.BadRequest(response));
            }
        }

        private static List<string> GetErrors(ActionExecutingContext actionContext)
        {
            List<string> errors = new List<string>();
            foreach (KeyValuePair<string, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry> state in actionContext.ModelState)
            {
                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in state.Value.Errors)
                {
                    errors.Add(error.ErrorMessage.IsNotNullOrEmpty() ? error.ErrorMessage : error.Exception?.Message);
                }
            }

            return errors;
        }

    }
}
