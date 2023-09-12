using Api.Exceptions;
using Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace Api.Extensions
{
    public static class ConfigureExceptionHandlerExtension
    {
        public static void ConfigureExceptionHandler<T>(this WebApplication app,IWebHostEnvironment env, ILogger<T> logger)
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {

                    context.Response.ContentType = MediaTypeNames.Application.Json;

                    IExceptionHandlerPathFeature? contextFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (contextFeature != null)
                    {
                        //loglama
                        logger.LogError(contextFeature.Error.Message);
                        ErrorResponse errorResponse = new ErrorResponse();
                        errorResponse.Path = contextFeature.Path;
                        errorResponse.Title = "Error!";
                        Type exceptionType = contextFeature.Error.GetType();

                        if (exceptionType == typeof(AuthenticationErrorException))
                        {
                            errorResponse.StatusCode = 403;
                            errorResponse.Message = contextFeature.Error.Message;
                        }
                        else if (exceptionType == typeof(NotFoundException))
                        {
                            errorResponse.StatusCode = 404;
                            errorResponse.Message = contextFeature.Error.Message;
                        }
                        else if (exceptionType == typeof(UserCreateFailedException))
                        {
                            errorResponse.StatusCode = 1002;
                            errorResponse.Message = contextFeature.Error.Message;
                        }
                        else if (exceptionType == typeof(UnauthorizedAccessException))
                        {
                            errorResponse.StatusCode = 401;

                            errorResponse.Message = contextFeature.Error.Message;
                        }
                        else if (exceptionType == typeof(BadHttpRequestException))
                        {
                            errorResponse.StatusCode = 400;
                            errorResponse.Message = contextFeature.Error.Message;
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                            errorResponse.Message = contextFeature.Error.Message;
                        }
                        errorResponse.Details = env.IsDevelopment() ? contextFeature.Error.StackTrace : null;

                        //hata mesajı yolllama
                        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse
                        ));
                    }

                });
            });
        }
    }
}
