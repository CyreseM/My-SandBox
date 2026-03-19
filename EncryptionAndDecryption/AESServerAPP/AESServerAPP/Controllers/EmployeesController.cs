using AESServerAPP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AESServerAPP.Controllers
{
    // API controller for managing employees.
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Initializes a new instance of EmployeesController with the specified DbContext.
        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieves all employees.
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            // Retrieve all employees from the database.
            var employees = await _context.Employees.ToListAsync();

            // Return the employee list as JSON. Middleware will handle encryption.
            return Ok(employees);
        }

        // Retrieves a specific employee by ID.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            // Find the employee by ID in the database.
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                // Return 404 Not Found if the employee does not exist.
                return NotFound();
            }

            // Return the employee data as JSON. Middleware will handle encryption.
            return Ok(employee);
        }

        // Adds a new employee.
        [HttpPost]
        public async Task<IActionResult> PostEmployee([FromBody] Employee employee)
        {
            if (employee == null)
            {
                // Return 400 Bad Request if the request body is null.
                return BadRequest("Employee data is required.");
            }

            // Add the new employee to the database context.
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Return the newly created employee. Middleware will handle encryption.
            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }

        // Updates an existing employee.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, [FromBody] Employee employee)
        {
            if (employee == null)
            {
                // Return 400 Bad Request if the request body is null.
                return BadRequest("Employee data is required.");
            }

            // Find the existing employee in the database.
            var existingEmployee = await _context.Employees.FindAsync(id);
            if (existingEmployee == null)
            {
                // Return 404 Not Found if the employee does not exist.
                return NotFound();
            }

            // Update the existing employee's properties.
            existingEmployee.Name = employee.Name;
            existingEmployee.Salary = employee.Salary;

            // Mark the entity as modified.
            _context.Entry(existingEmployee).State = EntityState.Modified;

            try
            {
                // Save changes to the database.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Return 404 Not Found if the employee no longer exists.
                return NotFound();
            }

            // Return 204 No Content to indicate successful update.
            return NoContent();
        }

        // Deletes an employee by ID.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            // Find the employee by ID in the database.
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                // Return 404 Not Found if the employee does not exist.
                return NotFound();
            }

            // Remove the employee from the database context.
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            // Return 204 No Content to indicate successful deletion.
            return NoContent();
        }

        // Checks if an employee exists in the database.
        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}