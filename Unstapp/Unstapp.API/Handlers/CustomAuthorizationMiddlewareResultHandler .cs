using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.API.Handlers
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Forbidden)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var error = new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Code = "FORBIDDEN",
                    Message = "No tenés permisos para realizar esta acción."
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(error));

                return;
            }

            if(authorizeResult.Challenged)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var error = new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "UNAUTHORIZED",
                    Message = "Tenés que iniciar sesión para realizar esta acción."
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(error));
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
