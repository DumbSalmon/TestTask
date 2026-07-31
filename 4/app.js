const apiBaseUrl = "https://localhost:7137";

const state = {
    employees: [],
    selectedEmployeeId: null,
    employeeFormMode: "create"
};

const elements = {
    loadButton: document.getElementById("load-button"),
    createButton: document.getElementById("create-button"),
    editButton: document.getElementById("edit-button"),
    deleteButton: document.getElementById("delete-button"),
    salaryUpdateButton: document.getElementById("salary-update-button"),
    maxSalaryCheckbox: document.getElementById("max-salary-checkbox"),
    message: document.getElementById("message"),
    employeesBody: document.getElementById("employees-body"),

    employeeModal: document.getElementById("employee-modal"),
    employeeModalTitle: document.getElementById("employee-modal-title"),
    employeeForm: document.getElementById("employee-form"),
    employeeFormMessage: document.getElementById("employee-form-message"),
    employeeCloseButton: document.getElementById("employee-close-button"),
    employeeId: document.getElementById("employee-id"),
    departmentId: document.getElementById("department-id"),
    chiefId: document.getElementById("chief-id"),
    employeeName: document.getElementById("employee-name"),
    salary: document.getElementById("salary"),

    salaryModal: document.getElementById("salary-modal"),
    salaryForm: document.getElementById("salary-form"),
    salaryFormMessage: document.getElementById("salary-form-message"),
    salaryCloseButton: document.getElementById("salary-close-button"),
    salaryDepartmentId: document.getElementById("salary-department-id"),
    salaryPercent: document.getElementById("salary-percent")
};

function showMessage(text, type = "") {
    elements.message.textContent = text;
    elements.message.className = "message";

    if (type) {
        elements.message.classList.add(type);
    }
}

function clearMessage() {
    showMessage("");
}

function showFormMessage(messageElement, text) {
    messageElement.textContent = text;
}

function clearFormMessage(messageElement) {
    messageElement.textContent = "";
}

function openModal(modal) {
    modal.classList.remove("hidden");
}

function closeModal(modal) {
    modal.classList.add("hidden");
}

function openCreateModal() {
    clearFormMessage(elements.employeeFormMessage);
    state.employeeFormMode = "create";
    elements.employeeForm.reset();
    elements.employeeId.value = "";
    elements.employeeModalTitle.textContent = "Добавить сотрудника";
    openModal(elements.employeeModal);
}

function openEditModal() {
    const employee = getSelectedEmployee();

    if (!employee) {
        showMessage("Выберитесотрудника для редактирования.", "error");
        return;
    }

    clearFormMessage(elements.employeeFormMessage);
    state.employeeFormMode = "edit";

    elements.employeeId.value = employee.id;
    elements.departmentId.value = employee.departmentId;
    elements.chiefId.value = employee.chiefId ?? "";
    elements.employeeName.value = employee.name;
    elements.salary.value = employee.salary;

    elements.employeeModalTitle.textContent = "Редактировать сотрудника";
    openModal(elements.employeeModal);
}

async function deleteSelectedEmployee() {
    const employee = getSelectedEmployee();

    if (!employee) {
        showMessage("Сначала выберите сотрудника для удаления.", "error");
        return;
    }

    const shouldDelete = confirm(`Удалить запись с ID = ${employee.id}?`);

    if (!shouldDelete) {
        return;
    }

    clearMessage();
    elements.deleteButton.disabled = true;

    try {
        const response = await fetch(`${apiBaseUrl}/api/employees/${employee.id}`, {
            method: "DELETE"
        });

        if (!response.ok) {
            throw new Error(await getErrorMessage(response));
        }

        await loadEmployees();
        showMessage("Сотрудник успешно удалён.", "success");
    } catch (error) {
        showMessage(error.message || "Не удалось удалить сотрудника.", "error");
    } finally {
        elements.deleteButton.disabled = false;
    }
}

function openSalaryModal() {
    clearFormMessage(elements.salaryFormMessage);
    elements.salaryForm.reset();
    elements.salaryPercent.value = "5";
    openModal(elements.salaryModal);
}

function getSelectedEmployee() {
    return state.employees.find(employee => employee.id === state.selectedEmployeeId);
}

async function getErrorMessage(response) {
    const responseText = await response.text();

    if (!responseText) {
        return `Ошибка API: ${response.status} ${response.statusText}`;
    }

    try {
        const errorData = JSON.parse(responseText);

        if (errorData.error) {
            return errorData.error;
        }

        if (errorData.errors) {
            return Object.values(errorData.errors)
                .flat()
                .join(" ");
        }
    } catch {
        return responseText;
    }

    return responseText;
}

function getEmployeesForDisplay() {
    if (!elements.maxSalaryCheckbox.checked || state.employees.length === 0) {
        return state.employees;
    }

    const maxSalary = Math.max(...state.employees.map(employee => employee.salary));

    return state.employees.filter(employee => employee.salary === maxSalary);
}

function renderEmployees() {
    elements.employeesBody.innerHTML = "";

    const employeesForDisplay = getEmployeesForDisplay();

    for (const employee of employeesForDisplay) {
        const row = document.createElement("tr");

        if (employee.id === state.selectedEmployeeId) {
            row.classList.add("selected");
        }

        const values = [
            employee.id,
            employee.departmentId,
            employee.chiefId ?? "—",
            employee.name,
            employee.salary
        ];

        for (const value of values) {
            const cell = document.createElement("td");
            cell.textContent = value;
            row.append(cell);
        }

        row.addEventListener("click", () => {
            state.selectedEmployeeId = employee.id;
            renderEmployees();
        });

        elements.employeesBody.append(row);
    }
}

async function loadEmployees() {
    clearMessage();
    elements.loadButton.disabled = true;

    try {
        const response = await fetch(`${apiBaseUrl}/api/employees`);

        if (!response.ok) {
            throw new Error(await getErrorMessage(response));
        }

        state.employees = await response.json();
        state.selectedEmployeeId = null;

        renderEmployees();
        showMessage(`Загружено сотрудников: ${state.employees.length}`, "success");
    } catch (error) {
        console.error(error);
        showMessage(error.message || "Не удалось получить данные от API.", "error");
    } finally {
        elements.loadButton.disabled = false;
    }
}

function getPositiveInteger(value, fieldName) {
    const number = Number(value);

    if (!Number.isInteger(number) || number <= 0) {
        throw new Error(`Поле ${fieldName} должно быть целым числом больше нуля.`);
    }

    return number;
}

function getPercent(value) {
    const percent = Number(value);

    if (!Number.isInteger(percent) || percent <= 0 || percent > 100) {
        throw new Error("Процент должен быть целым числом от 1 до 100.");
    }

    return percent;
}

function getEmployeeRequestData() {
    const name = elements.employeeName.value.trim();
    const chiefIdText = elements.chiefId.value.trim();

    if (!name) {
        throw new Error("Поле NAME обзательно для заполнения.");
    }

    if (name.length > 100) {
        throw new Error("Поле NAME не должно быть длиннее 100 символов.");
    }

    const requestData = {
        departmentId: getPositiveInteger(elements.departmentId.value, "DEPARTMENT_ID"),
        chiefId: chiefIdText === ""
            ? null
            : getPositiveInteger(chiefIdText, "CHIEF_ID"),
        name,
        salary: getPositiveInteger(elements.salary.value, "SALARY")
    };

    if (state.employeeFormMode === "edit") {
        requestData.id = getPositiveInteger(elements.employeeId.value, "ID");
    }

    return requestData;
}

async function saveEmployee(event) {
    event.preventDefault();
    clearMessage();
    clearFormMessage(elements.employeeFormMessage);

    try {
        const requestData = getEmployeeRequestData();

        const response = await fetch(`${apiBaseUrl}/api/employees`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(requestData)
        });

        if (!response.ok) {
            throw new Error(await getErrorMessage(response));
        }

        const actionName = state.employeeFormMode === "create"
            ? "добавлен"
            : "обновлён";

        closeModal(elements.employeeModal);
        await loadEmployees();
        showMessage(`Сотрудник ${actionName}.`, "success");
    } catch (error) {
        console.error(error);
        showFormMessage(
            elements.employeeFormMessage,
            error.message || "Не удалось сохранить сотрудника."
        );
    }
}

async function updateSalaryForDepartment(event) {
    event.preventDefault();
    clearMessage();
    clearFormMessage(elements.salaryFormMessage);

    try {
        const requestData = {
            departmentId: getPositiveInteger(
                elements.salaryDepartmentId.value,
                "DEPARTMENT_ID"
            ),
            percent: getPercent(elements.salaryPercent.value)
        };

        const response = await fetch(`${apiBaseUrl}/api/employees/salary-update`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(requestData)
        });

        if (!response.ok) {
            throw new Error(await getErrorMessage(response));
        }

        closeModal(elements.salaryModal);
        await loadEmployees();
        showMessage("Зарплаты сотрудников отдела успешно обновлены.", "success");
    } catch (error) {
        console.error(error);
        showFormMessage(
            elements.salaryFormMessage,
            error.message || "Не удалось обновить зарплаты."
        );
    }
}

elements.createButton.addEventListener("click", openCreateModal);
elements.editButton.addEventListener("click", openEditModal);
elements.deleteButton.addEventListener("click", deleteSelectedEmployee);
elements.salaryUpdateButton.addEventListener("click", openSalaryModal);
elements.loadButton.addEventListener("click", loadEmployees);

elements.maxSalaryCheckbox.addEventListener("change", () => {
    renderEmployees();
});

elements.employeeCloseButton.addEventListener("click", () => {
    closeModal(elements.employeeModal);
});

elements.salaryCloseButton.addEventListener("click", () => {
    closeModal(elements.salaryModal);
});

elements.employeeForm.addEventListener("submit", saveEmployee);
elements.salaryForm.addEventListener("submit", updateSalaryForDepartment);