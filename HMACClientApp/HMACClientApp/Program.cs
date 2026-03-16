using HMACClientApp;

public class Program
{
    static async Task Main(string[] args)
    {
        // Proper Client ID, Secret, and Base URL of the API
        var clientId = "DesktopClient";
        var secretKey = "m1n2b3v4c5x6z7l8k9j0";
        var baseUrl = "https://localhost:7035";
        var client = new HttpClient
        {
            // Default timeout for HttpClient in .NET is 100 seconds;
            // override it here to 5 Minutes
            Timeout = TimeSpan.FromMinutes(5)
        };
        try
        {
            // Create a New Employee (POST Request)
            var employee = new
            {
                Name = "Pranaya Rout",
                Position = "Developer",
                Salary = 60000
            };
            var response = await HMACHelper.SendRequestAsync(client, HttpMethod.Post, baseUrl, "/api/employees", clientId, secretKey, employee);
            if (response.IsSuccessStatusCode)
            {
                // Log success for POST request
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("POST Response: Employee Created Successfully");
                Console.WriteLine($"Response Content: {responseContent}");
            }
            else
            {
                // Log error details for POST request
                Console.WriteLine($"POST Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
            // Get All Employees (GET Request)
            response = await HMACHelper.SendRequestAsync(client, HttpMethod.Get, baseUrl, "/api/employees", clientId, secretKey);
            if (response.IsSuccessStatusCode)
            {
                // Log success for GET all employees request
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("\nGET Response: Employees Retrieved Successfully");
                Console.WriteLine($"Response Content: {responseContent}");
            }
            else
            {
                // Log error details for GET all employees request
                Console.WriteLine($"GET Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
            // Get Employee by ID (GET Request)
            var employeeId = 1;
            response = await HMACHelper.SendRequestAsync(client, HttpMethod.Get, baseUrl, $"/api/employees/{employeeId}", clientId, secretKey);
            if (response.IsSuccessStatusCode)
            {
                // Log success for GET by ID request
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("\nGET by ID Response: Employee Retrieved Successfully");
                Console.WriteLine($"Response Content: {responseContent}");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Log not found error for GET by ID request
                Console.WriteLine($"GET by ID Error: Employee with ID {employeeId} not found.");
            }
            else
            {
                // Log other error details for GET by ID request
                Console.WriteLine($"GET by ID Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
            // Update Employee (PUT Request)
            var updatedEmployee = new
            {
                Id = employeeId,
                Name = "Rakesh Sharma",
                Position = "Senior Developer",
                Salary = 80000
            };
            response = await HMACHelper.SendRequestAsync(client, HttpMethod.Put, baseUrl, $"/api/employees/{employeeId}", clientId, secretKey, updatedEmployee);
            if (response.IsSuccessStatusCode)
            {
                // Log success for PUT request
                Console.WriteLine($"PUT Response: Employee Updated Successfully. Status: {response.StatusCode}");
            }
            else
            {
                // Log error details for PUT request
                Console.WriteLine($"PUT Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
            // Delete Employee(DELETE Request)
            response = await HMACHelper.SendRequestAsync(client, HttpMethod.Delete, baseUrl, $"/api/employees/{employeeId}", clientId, secretKey);
            if (response.IsSuccessStatusCode)
            {
                // Log success for DELETE request
                Console.WriteLine($"DELETE Response: Employee Deleted Successfully. Status: {response.StatusCode}");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Log not found error for DELETE request
                Console.WriteLine($"DELETE Error: Employee with ID {employeeId} not found.");
            }
            else
            {
                // Log other error details for DELETE request
                Console.WriteLine($"DELETE Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            // Log any unexpected exceptions
            Console.WriteLine($"Unexpected Error: {ex.Message}");
        }
        Console.ReadKey();
    }
}