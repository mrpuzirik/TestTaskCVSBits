using Microsoft.AspNetCore.Mvc;
using TestTaskCVS.Models;
using TestTaskCVS.Services;

namespace TestTaskCVS.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllAsync();
            return View(employees);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var result = await _employeeService.ImportCsvAsync(file);
            if (!result.IsSuccess)
                TempData["Error"] = result.ErrorMessage;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _employeeService.DeleteAsync(id);
            return result.IsSuccess ? Ok() : NotFound(result.ErrorMessage);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] Employee employee)
        {
            var (isSuccess, errors) = await _employeeService.UpdateAsync(employee);
            return isSuccess ? Ok() : BadRequest(errors);
        }

        public IActionResult About()
        {
            return View(About);
        }
    }
}

