using System.Text;
using System.Text.Json;

namespace AESClientApp
{
    class Program
    {
        // Hardcoded Client Credentials
        private const string ClientId = "Client1";
        private const string Base64Key = "gi1D2eDd8Tg565ZbfRWc00j9xKtBka4ZHu0Sen+Drgc=";
        private const string Base64IV = "Qb4nTgWS7UBo2YU7G/gJCg==";

        // API Base URL
        private const string ApiBaseUrl = "https://localhost:7086/";

        static async Task Main(string[] args)
        {
            Console.WriteLine("AES Encrypted Employee Client");
            Console.WriteLine("------------------------------");

            // Initialize HttpClient
            using (var httpClient = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) })
            {
                // Add ClientId to headers
                httpClient.DefaultRequestHeaders.Add("ClientId", ClientId);

                // Initialize AES Encryption Service
                var encryptionService = new AesEncryptionService(Base64Key, Base64IV);

                try
                {
                    // 1. Get All Employees
                    Console.WriteLine("\n1. Fetching All Employees...");
                    var employees = await GetAllEmployeesAsync(httpClient, encryptionService);
                    DisplayEmployees(employees);

                    // 2. Create a New Employee
                    Console.WriteLine("\n2. Creating a New Employee...");
                    var newEmployee = new Employee
                    {
                        Name = "Pranaya",
                        Salary = 10000m
                    };
                    var createdEmployee = await CreateEmployeeAsync(httpClient, encryptionService, newEmployee);
                    Console.WriteLine($"Employee Created: ID = {createdEmployee.Id}, Name = {createdEmployee.Name}, Salary = {createdEmployee.Salary}");

                    // 3. Get Employee By ID
                    Console.WriteLine("\n3. Fetching Employee By ID...");
                    var fetchedEmployee = await GetEmployeeByIdAsync(httpClient, encryptionService, createdEmployee.Id);
                    Console.WriteLine($"Fetched Employee: ID = {fetchedEmployee.Id}, Name = {fetchedEmployee.Name}, Salary = {fetchedEmployee.Salary}");

                    // 4. Update Existing Employee
                    Console.WriteLine("\n4. Updating Existing Employee...");
                    fetchedEmployee.Name = "Pranaya Updated";
                    fetchedEmployee.Salary = 12000m;
                    bool updateSuccess = await UpdateEmployeeAsync(httpClient, encryptionService, fetchedEmployee.Id, fetchedEmployee);
                    Console.WriteLine(updateSuccess ? "Employee Updated Successfully." : "Failed to Update Employee.");

                    // 5. Get Updated Employee By ID
                    Console.WriteLine("\n5. Fetching Updated Employee By ID...");
                    var updatedEmployee = await GetEmployeeByIdAsync(httpClient, encryptionService, fetchedEmployee.Id);
                    Console.WriteLine($"Updated Employee: ID = {updatedEmployee.Id}, Name = {updatedEmployee.Name}, Salary = {updatedEmployee.Salary}");

                    // 6. Delete Employee
                    Console.WriteLine("\n6. Deleting Employee...");
                    bool deleteSuccess = await DeleteEmployeeAsync(httpClient, encryptionService, updatedEmployee.Id);
                    Console.WriteLine(deleteSuccess ? "Employee Deleted Successfully." : "Failed to Delete Employee.");

                    // 7. Get All Employees After Deletion
                    Console.WriteLine("\n7. Fetching All Employees After Deletion...");
                    var employeesAfterDeletion = await GetAllEmployeesAsync(httpClient, encryptionService);
                    DisplayEmployees(employeesAfterDeletion);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                }
            }

            Console.WriteLine("\nAll operations completed.");
            Console.ReadKey();
        }

        // Fetches all employees from the API.
        private static async Task<List<Employee>> GetAllEmployeesAsync(HttpClient httpClient, AesEncryptionService encryptionService)
        {
            var response = await httpClient.GetAsync("api/employees");
            response.EnsureSuccessStatusCode();

            string encryptedResponse = await response.Content.ReadAsStringAsync();
            string decryptedResponse = encryptionService.DecryptString(encryptedResponse);

            var employees = JsonSerializer.Deserialize<List<Employee>>(decryptedResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (employees == null)
                throw new JsonException("Deserialization returned null for GetAllEmployeesAsync.");

            return employees;
        }

        // Creates a new employee via the API.
        private static async Task<Employee> CreateEmployeeAsync(HttpClient httpClient, AesEncryptionService encryptionService, Employee employee)
        {
            string plainJson = JsonSerializer.Serialize(employee);
            string encryptedJson = encryptionService.EncryptString(plainJson);

            var content = new StringContent(encryptedJson, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/employees", content);
            response.EnsureSuccessStatusCode();

            string encryptedResponse = await response.Content.ReadAsStringAsync();
            string decryptedResponse = encryptionService.DecryptString(encryptedResponse);

            var createdEmployee = JsonSerializer.Deserialize<Employee>(decryptedResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (createdEmployee == null)
                throw new JsonException("Deserialization returned null for CreateEmployeeAsync.");

            return createdEmployee;
        }

        // Fetches an employee by ID from the API.
        private static async Task<Employee> GetEmployeeByIdAsync(HttpClient httpClient, AesEncryptionService encryptionService, int id)
        {
            var response = await httpClient.GetAsync($"api/employees/{id}");
            response.EnsureSuccessStatusCode();

            string encryptedResponse = await response.Content.ReadAsStringAsync();
            string decryptedResponse = encryptionService.DecryptString(encryptedResponse);

            var employee = JsonSerializer.Deserialize<Employee>(decryptedResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (employee == null)
                throw new JsonException("Deserialization returned null for GetEmployeeByIdAsync.");

            return employee;
        }

        // Updates an existing employee via the API.
        private static async Task<bool> UpdateEmployeeAsync(HttpClient httpClient, AesEncryptionService encryptionService, int id, Employee employee)
        {
            string plainJson = JsonSerializer.Serialize(employee);
            string encryptedJson = encryptionService.EncryptString(plainJson);

            var content = new StringContent(encryptedJson, Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"api/employees/{id}", content);

            return response.IsSuccessStatusCode;
        }

        // Deletes an employee via the API.
        private static async Task<bool> DeleteEmployeeAsync(HttpClient httpClient, AesEncryptionService encryptionService, int id)
        {
            var response = await httpClient.DeleteAsync($"api/employees/{id}");
            return response.IsSuccessStatusCode;
        }

        // Displays a list of employees to the console.
        private static void DisplayEmployees(List<Employee> employees)
        {
            if (employees == null || employees.Count == 0)
            {
                Console.WriteLine("No employees found.");
                return;
            }

            Console.WriteLine("Employees:");
            foreach (var emp in employees)
            {
                Console.WriteLine($"ID: {emp.Id}, Name: {emp.Name}, Salary: {emp.Salary}");
            }
        }
    }
}