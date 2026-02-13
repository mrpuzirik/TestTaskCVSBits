using TestTaskCVS.Models;

namespace TestTaskCVS.Services
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();
        Task<(bool IsSuccess, string ErrorMessage)> ImportCsvAsync(IFormFile file);
        Task<(bool IsSuccess, string ErrorMessage)> DeleteAsync(int id);
        Task<(bool IsSuccess, IEnumerable<string> Errors)> UpdateAsync(Employee employee);
    }
}
