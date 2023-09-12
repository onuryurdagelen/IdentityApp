using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Attributes
{
    public class CustomExceptionFilterAttribute:ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if(context.Exception != null)
            {
                var exception = context.Exception;
                var message = exception.Message;
                var path = context.HttpContext.Request.Path.Value;
                if (context.Exception.InnerException != null)
                {
                    var innerMessage = context.Exception.InnerException;
                    //catching the exception

                    context.Result = new ObjectResult(new
                    {
                        Title ="Unexpected error occurred in server",
                        Message = message,
                        Details = innerMessage.Message,
                        Path = path,
                    });
                }
                else
                {
                    context.Result = new ObjectResult(new
                    {
                        Title = "Unexpected error occurred in server",
                        Message = message,
                        Path = path,
                    });
                }
            }



            
        }
    }
}
