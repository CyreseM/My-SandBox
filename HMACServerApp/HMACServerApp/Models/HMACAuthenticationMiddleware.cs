using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace HMACServerApp.Models
{
    public class HMACAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        // Nonce expiry time to prevent reuse of nonces
        private static readonly TimeSpan NonceExpiry = TimeSpan.FromMinutes(5);

        // Constructor to initialize the middleware with the next request delegate and memory cache
        public HMACAuthenticationMiddleware(RequestDelegate next, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _next = next;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        // This will handle each request and perform HMAC validation
        public async Task Invoke(HttpContext context)
        {
            // Check if HMAC is enabled
            var isHMACEnabled = _configuration.GetValue<bool>("HMACSettings:EnableHMAC");
            if (!isHMACEnabled)
            {
                // Skip HMAC validation and call the next middleware
                await _next(context);
                return;
            }

            // Proceed with HMAC validation

            // Check if the Authorization header is present
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authorization header missing");
                return;
            }

            // Check if the Authorization header starts with "HMAC "
            if (!authHeader.ToString().StartsWith("HMAC ", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid Authorization header");
                return;
            }

            // Extract token parts from the Authorization header
            var tokenParts = authHeader.ToString().Substring("HMAC ".Length).Trim().Split('|');
            if (tokenParts.Length != 4)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid HMAC format");
                return;
            }

            var clientId = tokenParts[0]; // Extract client ID
            var token = tokenParts[1];    // Extract HMAC token
            var nonce = tokenParts[2];    // Extract nonce
            var timestamp = tokenParts[3]; // Extract timestamp

            // Get the IClientSecretService Instance from DI
            var clientSecretService = context.RequestServices.GetRequiredService<ClientSecretService>();

            // Validate the client ID and get the secret key
            var secretKey = await clientSecretService.GetSecretKeyAsync(clientId);
            if (string.IsNullOrEmpty(secretKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid client ID");
                return;
            }

            // Validate the timestamp 
            if (!long.TryParse(timestamp, out var timestampSeconds))
            {
                // Invalid timestamp format
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid timestamp format");
                return;
            }

            //Convert the timestamp to Unix Time or EPOC Time
            var requestTime = DateTimeOffset.FromUnixTimeSeconds(timestampSeconds).UtcDateTime;
            var currentTime = DateTime.UtcNow;

            // Check if the timestamp is within the allowed timeframe (within 5 minutes)
            // This is to avoid Reply Attack
            if (Math.Abs((currentTime - requestTime).TotalMinutes) > 5)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Timestamp is outside the allowable range");
                return;
            }

            // Validate the nonce using a client-specific cache key
            var nonceKey = $"{clientId}:{nonce}";
            if (_memoryCache.TryGetValue(nonceKey, out _))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Nonce has already been used");
                return;
            }

            // Add the client specific nonce to the cache with an expiry time
            // This is to avoid Reply Attack
            _memoryCache.Set(nonceKey, true, NonceExpiry);

            // Read the request body for POST and PUT requests
            var requestBody = string.Empty;
            if (context.Request.Method == HttpMethod.Post.Method || context.Request.Method == HttpMethod.Put.Method)
            {
                //The context.Request.EnableBuffering(); method is used to allow the HTTP request body to be read multiple times.
                //In the context of HMAC authentication middleware, it is necessary because the request body needs to be read to compute the HMAC for validation,
                //but it must also be available to any downstream middleware or the actual API controller.
                context.Request.EnableBuffering();

                //Read the request body
                //Using a StreamReader, we can read the entire request body into a string.
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    //This reads the request body stream to the end.
                    requestBody = await reader.ReadToEndAsync();

                    //The statement context.Request.Body.Position = 0; is used to reset the position of the request body stream to the beginning.
                    //This is important because, by default, the request body stream in ASP.NET Core is a forward-only stream,
                    //meaning once you read it, it cannot be read again unless you explicitly reset its position.
                    context.Request.Body.Position = 0;
                }
            }

            // Validate the HMAC token
            var isValid = ValidateToken(token, nonce, timestamp, context.Request, requestBody, secretKey);

            if (!isValid)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid HMAC token");
                return;
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }

        // Method to validate the HMAC token
        private bool ValidateToken(string token, string nonce, string timestamp, HttpRequest request, string requestBody, string secretKey)
        {
            //Fetch the Request Path
            var path = Convert.ToString(request.Path);

            // Build the request content by concatenating method, path, nonce, and timestamp
            var requestContent = new StringBuilder()
                .Append(request.Method.ToUpper())
                .Append(path.ToUpper())
                .Append(nonce)
                .Append(timestamp);

            // Include the request body for POST and PUT methods
            if (request.Method == HttpMethod.Post.Method || request.Method == HttpMethod.Put.Method)
            {
                requestContent.Append(requestBody);
            }

            // Convert secret key and request content to bytes
            var secretBytes = Encoding.UTF8.GetBytes(secretKey);
            var requestBytes = Encoding.UTF8.GetBytes(requestContent.ToString());

            // Create HMACSHA256 instance with the secret key
            using var hmac = new HMACSHA256(secretBytes);

            // Compute the hash of the request content
            var computedHash = hmac.ComputeHash(requestBytes);

            // Convert the computed hash to base64 string (token)
            var computedToken = Convert.ToBase64String(computedHash);

            //compare the generated HMAC with the HMAC received from the request 
            return token == computedToken;
        }
    }
}
