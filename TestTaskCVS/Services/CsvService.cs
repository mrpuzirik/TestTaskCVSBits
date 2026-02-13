using System.Globalization;
using TestTaskCVS.data;
using TestTaskCVS.Models;

namespace TestTaskCVS.Services
{
    public class CsvService : ICsvService
    {


        private readonly ApplicationDbContext _context;

        public CsvService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ProcessCsvAsync(IFormFile file)
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

                var values = line.Split(';');
                if (values.Length != 5)
                    return (false, $"Line {lineNumber}: Invalid number of fields");

                string name = values[0];
                string dobStr = values[1];
                string marriedStr = values[2];
                string phone = values[3];
                string salaryStr = values[4];

                // ===============================
                // Валідація полів
                // ===============================
                if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
                    return (false, $"Line {lineNumber}: Name is required and max 100 characters");

                if (!DateTime.TryParse(dobStr, out DateTime dob))
                    return (false, $"Line {lineNumber}: DateOfBirth is invalid");

                // Валідація віку
                var today = DateTime.Today;
                var age = today.Year - dob.Year;
                if (dob.Date > today.AddYears(-age)) age--;

                if (age < 18 || age > 70)
                    return (false, $"Line {lineNumber}: Employee age must be between 18 and 70");

                if (!bool.TryParse(marriedStr, out bool married))
                    return (false, $"Line {lineNumber}: Married must be true or false");

                if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+?\d{7,15}$"))
                    return (false, $"Line {lineNumber}: Phone must be 7-15 digits");

                if (!decimal.TryParse(salaryStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal salary))
                    return (false, $"Line {lineNumber}: Salary is invalid");

                if (salary < 0 || salary > 1000000)
                    return (false, $"Line {lineNumber}: Salary must be between 0 and 1,000,000");

                // Перевірка максимум 2 знаків після коми
                var decimalPart = salaryStr.Split('.')[1];
                if (decimalPart.Length > 2)
                    return (false, $"Line {lineNumber}: Salary can have at most 2 decimal places");

                // ===============================
                // Все ок — додаємо у контекст
                // ===============================
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
    }
}
