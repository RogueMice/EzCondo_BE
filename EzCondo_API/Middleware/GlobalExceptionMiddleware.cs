using System.Net;
using static EzConDo_Service.ExceptionsConfig.CustomException;

namespace EzCondo_API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionMiddleware> logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }


        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Default 500
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Internal Server Error";

            switch (ex)
            {
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;     // 404
                    message = ex.Message;
                    break;
                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest;   // 400
                    message = ex.Message;
                    break;
                case ConflictException:  
                    statusCode = HttpStatusCode.Conflict;     // 409
                    message = ex.Message;
                    break;
                case LockedException:                         //423
                    statusCode = HttpStatusCode.Locked;
                    message = ex.Message;
                    break;
                default:
                    logger.LogError(ex, ex.Message);
                    break;
            }

            //HTTP Status Code
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                StatusCode = (int)statusCode,
                Error = message
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
