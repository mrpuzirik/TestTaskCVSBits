# Employee CSV Management

## Project Overview
This web application allows uploading and managing employee data via CSV files.  
Users can edit, sort, search, and delete records directly in the browser.  

> ⚠ Note: The application runs locally, while the database runs in a Docker container.

---

## Features
- Upload CSV files (`;` or `,` as separators)  
- Inline editing of employee fields: Name, Date of Birth, Married, Phone, Salary  
- Client-side and server-side validation  
- Search and filtering  
- Sorting by any column  
- Delete employee records  
- Validation rules:  
  - Phone in international format (`+` required, 7–15 digits, first digit 1–9)  
  - Age between 18–70  
  - Salary 0–1,000,000, max 2 decimals  

---

## Technologies
- ASP.NET Core MVC  
- Entity Framework Core  
- JavaScript (Fetch API)  
- Bootstrap 5  
- Docker (for database)

---

## Author

### Andrii Shevchuk

This project was created as a test assignment.
It demonstrates uploading CSVs, validating and editing employee data, sorting, searching, and deleting records in a web interface.
