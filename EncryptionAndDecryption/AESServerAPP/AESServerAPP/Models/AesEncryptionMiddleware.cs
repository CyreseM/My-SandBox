using System.Text;

namespace AESServerAPP.Models
{
    // Middleware to handle AES encryption and decryption for HTTP request and response bodies.
    public class AesEncryptionMiddleware
    {
        // Delegate representing the next middleware in the pipeline.
        private readonly RequestDelegate _next;

        private readonly IConfiguration _configuration;

        // Constructor to initialize the middleware with the next delegate in the pipeline.
        public AesEncryptionMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next; // Assign the next middleware delegate.
            _configuration = configuration;
        }

        // Main middleware logic that intercepts requests and responses.
        public async Task InvokeAsync(HttpContext context)
        {
            // Check if HMAC is enabled
            var isEncryptionEnabled = _configuration.GetValue<bool>("EncryptionSettings:EnableEncryption");
            if (!isEncryptionEnabled)
            {
                // Skip Encryption and call the next middleware
                await _next(context);
                return;
            }

            // Proceed with Encryption

            // Retrieve 'ClientId' from request headers or use 'DefaultClient' if not provided.
            string clientId = context.Request.Headers["ClientId"].FirstOrDefault() ?? "DefaultClient";

            // Resolve the AES encryption service from the dependency injection container.
            var _encryptionService = context.RequestServices.GetRequiredService<AesEncryptionService>();

            // Check if the HTTP method is POST or PUT, indicating a request with a body that may need decryption.
            if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put)
            {
                // Enable request body buffering to allow multiple reads of the request body.
                context.Request.EnableBuffering();

                // Create a StreamReader to read the encrypted request body.
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    // Read the entire encrypted request body as a string.
                    string encryptedBody = await reader.ReadToEndAsync();

                    // Reset the request body stream position to the beginning for further use.
                    context.Request.Body.Position = 0;

                    // If the encrypted body is not empty or null, attempt decryption.
                    if (!string.IsNullOrWhiteSpace(encryptedBody))
                    {
                        try
                        {
                            // Decrypt the encrypted request body using the AES service.
                            string decryptedBody = await _encryptionService.DecryptStringAsync(clientId, encryptedBody);

                            // Convert the decrypted string into a byte array.
                            byte[] decryptedBytes = Encoding.UTF8.GetBytes(decryptedBody);

                            // Replace the original request body with the decrypted data as a MemoryStream.
                            context.Request.Body = new MemoryStream(decryptedBytes);
                        }
                        catch
                        {
                            // If decryption fails, respond with a 400 Bad Request status code and an error message.
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsync("Invalid encrypted request data.");
                            return; // Terminate the middleware pipeline execution.
                        }
                    }
                }
            }

            // Store the original response body stream to restore it later.
            // This variable stores a reference to the original response body stream before the middleware changes it
            var originalResponseBodyStream = context.Response.Body;

            // Create a new MemoryStream to capture the response body.
            // This is a MemoryStream that temporarily replaces the original response body.
            using (var responseBody = new MemoryStream())
            {
                // Set the response body to the MemoryStream for temporary capture.
                context.Response.Body = responseBody;

                try
                {
                    // Proceed to the next middleware in the pipeline and execute the action method.
                    await _next(context);

                    // Reset the response body stream position to the beginning for reading.
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    // Read the response body content as a string.
                    string responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();

                    // Encrypt the response if the HTTP status code indicates success and the body is not empty.
                    if (!string.IsNullOrWhiteSpace(responseText) &&
                        context.Response.StatusCode >= 200 &&
                        context.Response.StatusCode < 300)
                    {
                        // Encrypt the response text using the AES service.
                        string encryptedResponse = await _encryptionService.EncryptStringAsync(clientId, responseText);

                        // Convert the encrypted string into a byte array.
                        byte[] encryptedBytes = Encoding.UTF8.GetBytes(encryptedResponse);

                        // Update the Content-Length header with the new encrypted data size.
                        context.Response.ContentLength = encryptedBytes.Length;

                        // Clear the current response body content.
                        context.Response.Body.SetLength(0);

                        // Write the encrypted response data back to the response body stream.
                        await context.Response.Body.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);
                    }
                }
                catch
                {
                    // If an error occurs during processing, set the response status to 500 Internal Server Error.
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    // Write an error message to the response body.
                    await context.Response.WriteAsync("Error processing response.");
                }
                //The finally block ensures that no matter what happens (whether the middleware runs successfully or encounters an error),
                //the original response stream is restored.
                finally
                {
                    // Reset the response body stream position.
                    // This ensures that the subsequent read operation (which will copy the stream’s data) begins from the very first byte.
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    // After resetting the position, this line copies the entire content of responseBody(the temporary stream holding the processed response) into originalResponseBodyStream(the original response stream provided by ASP.NET Core).
                    await context.Response.Body.CopyToAsync(originalResponseBodyStream);

                    // This restores the HttpContext’s response body to its original stream, making sure subsequent middleware or the server itself continues working with the correct response stream.
                    context.Response.Body = originalResponseBodyStream;
                }
            }
        }
    }
}