
using GymSystem.UI.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace GymSystem.UI.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the exception with details
        _logger.LogError(exception,
            "Unhandled exception occurred. Path: {Path}, Method: {Method}",
            httpContext.Request.Path,
            httpContext.Request.Method);

        // Store exception details for the error page
        var errorId = Guid.NewGuid().ToString();
        var errorViewModel = new ErrorViewModel
        {
            ErrorId = errorId,
            Message = exception.Message,
            StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
            Path = httpContext.Request.Path,
            Method = httpContext.Request.Method,
            Timestamp = DateTime.UtcNow
        };

        // Store in Items for the error page to access
        httpContext.Items["ErrorViewModel"] = errorViewModel;

        // Set response status code
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "text/html";

        // Render the error page
        await httpContext.Response.WriteAsync(
            GenerateErrorPageHtml(errorViewModel, _environment.IsDevelopment()));

        return true;
    }

    private string GenerateErrorPageHtml(ErrorViewModel model, bool isDevelopment)
    {
        return $@"
    <!DOCTYPE html>
    <html lang=""en"">
    <head>
        <meta charset=""utf-8"" />
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
        <title>Error - Power Fitness</title>
        <style>
            * {{ margin: 0; padding: 0; box-sizing: border-box; }}
            body {{
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                background: linear-gradient(135deg, #010d2e 0%, #01278b 50%, #0d3fa6 100%);
                min-height: 100vh;
                display: flex;
                align-items: center;
                justify-content: center;
                padding: 20px;
            }}
            .error-container {{
                max-width: 800px;
                width: 100%;
                background: white;
                border-radius: 24px;
                box-shadow: 0 25px 80px rgba(0,0,0,0.25);
                overflow: hidden;
            }}
            .error-header {{
                background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
                padding: 30px;
                text-align: center;
                color: white;
            }}
            .error-header h1 {{
                font-size: 48px;
                margin-bottom: 10px;
            }}
            .error-header p {{
                font-size: 18px;
                opacity: 0.9;
            }}
            .error-body {{
                padding: 30px;
            }}
            .error-details {{
                background: #f8fafc;
                border-radius: 12px;
                padding: 20px;
                margin-bottom: 20px;
                border-left: 4px solid #dc2626;
            }}
            .error-id {{
                font-family: monospace;
                background: #e2e8f0;
                padding: 4px 8px;
                border-radius: 6px;
                font-size: 12px;
            }}
            .stack-trace {{
                background: #1e1e1e;
                color: #d4d4d4;
                padding: 15px;
                border-radius: 8px;
                font-family: 'Consolas', monospace;
                font-size: 12px;
                overflow-x: auto;
                margin-top: 15px;
            }}
            .button {{
                display: inline-block;
                padding: 12px 24px;
                background: #01278b;
                color: white;
                text-decoration: none;
                border-radius: 8px;
                margin-top: 20px;
                transition: all 0.3s;
            }}
            .button:hover {{
                background: #011d6b;
                transform: translateY(-2px);
            }}
            hr {{
                margin: 20px 0;
                border: none;
                border-top: 1px solid #e2e8f0;
            }}
        </style>
    </head>
    <body>
        <div class=""error-container"">
            <div class=""error-header"">
                <h1>⚠️ Oops!</h1>
                <p>Something went wrong on our end</p>
            </div>
            <div class=""error-body"">
                <div class=""error-details"">
                    <strong>Error ID:</strong> <span class=""error-id"">{model.ErrorId}</span><br/>
                    <strong>Time:</strong> {model.Timestamp:yyyy-MM-dd HH:mm:ss}<br/>
                    <strong>Path:</strong> {model.Path}<br/>
                    <strong>Method:</strong> {model.Method}
                </div>
            
                <p><strong>Message:</strong> {model.Message}</p>
            
                {(isDevelopment ? $@"
                <div class=""stack-trace"">
                    <strong>Stack Trace:</strong><br/>
                    {model.StackTrace?.Replace("\n", "<br/>")}
                </div>
                " : "<p>Our team has been notified. Please try again later.</p>")}
            
                <hr/>
            
                <div style=""display: flex; gap: 15px; justify-content: center;"">
                    <a href=""/"" class=""button"">🏠 Go to Home</a>
                    <a href=""javascript:history.back()"" class=""button"" style=""background: #64748b;"">⬅️ Go Back</a>
                </div>
            </div>
        </div>
    </body>
    </html>";
    }
}
