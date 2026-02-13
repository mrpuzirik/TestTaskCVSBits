document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("searchInput").addEventListener("keyup", function () {
        let filter = this.value.toLowerCase();

        document.querySelectorAll("#employeesTable tbody tr").forEach(row => {
            const cells = row.querySelectorAll("td");
            let text = "";

            text += cells[0].querySelector("input").value + " ";
            const dobInput = cells[1].querySelector("input").value;
            let dobText = "";
            if (dobInput) {
                const date = new Date(dobInput);
                dobText = ("0" + date.getDate()).slice(-2) + "." +
                    ("0" + (date.getMonth() + 1)).slice(-2) + "." +
                    date.getFullYear();
            }
            text += dobText + " ";
            text += cells[2].querySelector("select").value + " ";
            text += cells[3].querySelector("input").value + " ";
            text += cells[4].querySelector("input").value;

            row.style.display = text.toLowerCase().includes(filter) ? "" : "none";
        });
    });

    window.deleteRow = function (id) {
        fetch('/Employees/Delete/' + id, { method: 'POST' })
            .then(() => location.reload());
    }

    document.querySelectorAll("#employeesTable .editable").forEach(input => {
        let previousValue;

        input.addEventListener("focus", function () {
            previousValue = input.value;
        });

        input.addEventListener("change", function () {
            const row = input.closest("tr");
            const id = row.dataset.id;

            const cells = row.querySelectorAll(".editable");
            const name = cells[0].value.trim();
            const dob = cells[1].value;
            const married = cells[2].value === "true";
            const phone = cells[3].value.trim();
            const salary = parseFloat(cells[4].value);


            if (!name) {
                alert("Name is required");
                cells[0].value = previousValue;
                return;
            }

            if (!dob) {
                alert("Date of birth is required");
                cells[1].value = previousValue;
                return;
            }


            if (!/^\+[1-9]\d{6,14}$/.test(phone)) {
                alert("Phone must be 7-15 digits, starting with +, first num 1-9");
                cells[3].value = previousValue;
                return;
            }

            const minSalary = 0;
            const maxSalary = 1000000;
            if (isNaN(salary) || salary < minSalary || salary > maxSalary) {
                alert(`Salary must be a number between ${minSalary} and ${maxSalary}`);
                cells[4].value = previousValue;
                return;
            }

            const decimalPart = cells[4].value.split('.')[1];
            if (decimalPart && decimalPart.length > 2) {
                alert("Salary can have at most 2 decimal places");
                cells[4].value = previousValue;
                return;
            }

            const data = { Id: id, Name: name, DateOfBirth: dob, Married: married, Phone: phone, Salary: salary };

            fetch('/Employees/Update', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            }).then(res => {
                if (!res.ok) {
                    res.json().then(errors => {
                        alert(errors.join("\n"));
                        cells.forEach((cell, idx) => {
                            cell.value = previousValue;
                        });
                    });
                }
            });
        });
    });

    const table = document.getElementById("employeesTable");
    const headers = table.querySelectorAll("th");
    let sortDirection = {};

    headers.forEach((header, index) => {
        header.style.cursor = "pointer";
        sortDirection[index] = true; 

        header.addEventListener("click", () => {
            const tbody = table.querySelector("tbody");
            const rows = Array.from(tbody.querySelectorAll("tr"));

            rows.sort((a, b) => {
                let aValue, bValue;

                if (index === 1) { 
                    aValue = new Date(a.cells[index].querySelector("input").value);
                    bValue = new Date(b.cells[index].querySelector("input").value);
                } else if (index === 2) { 
                    aValue = a.cells[index].querySelector("select").value === "true" ? 1 : 0;
                    bValue = b.cells[index].querySelector("select").value === "true" ? 1 : 0;
                } else if (index === 4) {
                    aValue = parseFloat(a.cells[index].querySelector("input").value);
                    bValue = parseFloat(b.cells[index].querySelector("input").value);
                } else {
                    aValue = a.cells[index].querySelector("input").value.toLowerCase();
                    bValue = b.cells[index].querySelector("input").value.toLowerCase();
                }

                if (aValue > bValue) return sortDirection[index] ? 1 : -1;
                if (aValue < bValue) return sortDirection[index] ? -1 : 1;
                return 0;
            });

            tbody.innerHTML = "";
            rows.forEach(row => tbody.appendChild(row));

            sortDirection[index] = !sortDirection[index]; 
        });
    });
});