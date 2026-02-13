using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TestTaskCVS.data;
using TestTaskCVS.Models;

namespace TestTaskCVS.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
        }

         public async Task<(bool IsSuccess, string ErrorMessage)> ImportCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "File is empty");

            using var reader = new StreamReader(file.OpenReadStream());
            bool isFirstLine = true;
            int lineNumber = 0;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                lineNumber++;

                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                char separator = line.Count(c => c == ';') > line.Count(c => c == ',')
                                ? ';'
                                : ',';

                var values = line.Split(separator);
                if (values.Length != 5)
                    return (false, $"Line {lineNumber}: Invalid number of fields");

                string name = values[0];
                string dobStr = values[1];
                string marriedStr = values[2];
                string phone = values[3];
                string salaryStr = values[4];
  
                if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
                    return (false, $"Line {lineNumber}: Name is required and max 100 characters");

                if (!DateTime.TryParse(dobStr, out DateTime dob))
                    return (false, $"Line {lineNumber}: DateOfBirth is invalid");

                var today = DateTime.Today;
                var age = today.Year - dob.Year;
                if (dob.Date > today.AddYears(-age)) age--;
                if (age < 18 || age > 70)
                    return (false, $"Line {lineNumber}: Employee age must be between 18 and 70");

                if (!bool.TryParse(marriedStr, out bool married))
                    return (false, $"Line {lineNumber}: Married must be true or false");

                if (!Regex.IsMatch(phone, @"^\+[1-9]\d{6,14}"))
                    return (false, $"Line {lineNumber}: \"Phone must be 7-15 digits, starting with +, first num 1-9\"");

                if (!decimal.TryParse(salaryStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal salary))
                    return (false, $"Line {lineNumber}: Salary is invalid");

                if (salary < 0 || salary > 1000000)
                    return (false, $"Line {lineNumber}: Salary must be between 0 and 1,000,000");

                var decimalPart = salaryStr.Contains('.') ? salaryStr.Split('.')[1] : "";
                if (decimalPart.Length > 2)
                    return (false, $"Line {lineNumber}: Salary can have at most 2 decimal places");

                var employee = new Employee
                {
                    Name = name,
                    DateOfBirth = dob,
                    Married = married,
                    Phone = phone,
                    Salary = salary
                };

                _context.Employees.Add(employee);
            }

            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> DeleteAsync(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return (false, "Employee not found");

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool IsSuccess, IEnumerable<string> Errors)> UpdateAsync(Employee employee)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(employee.Name) || employee.Name.Length > 100)
                errors.Add("Name is required and max 100 characters");

            var today = DateTime.Today;
            var age = today.Year - employee.DateOfBirth.Year;
            if (employee.DateOfBirth.Date > today.AddYears(-age)) age--;
            if (age < 18 || age > 70)
                errors.Add("Employee age must be between 18 and 70");

            if (!Regex.IsMatch(employee.Phone, @"^\+[1-9]\d{6,14}$"))
                errors.Add("Phone must be 7-15 digits, starting with +, first num 1-9");

            if (employee.Salary < 0 || employee.Salary > 1000000)
                errors.Add("Salary must be between 0 and 1,000,000");

            var decimalPart = employee.Salary.ToString(CultureInfo.InvariantCulture).Contains('.') ?
                              employee.Salary.ToString(CultureInfo.InvariantCulture).Split('.')[1] : "";
            if (decimalPart.Length > 2)
                errors.Add("Salary can have at most 2 decimal places");

            if (errors.Any()) return (false, errors);

            var existing = await _context.Employees.FindAsync(employee.Id);
            if (existing == null)
                return (false, new[] { "Employee not found" });

            existing.Name = employee.Name;
            existing.DateOfBirth = employee.DateOfBirth;
            existing.Married = employee.Married;
            existing.Phone = employee.Phone;
            existing.Salary = employee.Salary;

            await _context.SaveChangesAsync();
            return (true, Enumerable.Empty<string>());
        }
    }
}
