USE [dumbsalmon]
GO

/****** Объект:  StoredProcedure [dbo].[UpdateSalaryForDepartment]    Дата создания скрипта: 31.07.2026 4:31:04 ******/ 
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[UpdateSalaryForDepartment]
    @p_department_id INT,
    @percent INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @val_chief_id INT;
    DECLARE @max_salary INT;

    IF @percent <= 0
    BEGIN
        THROW 50001,
              N'Процент для повышения не может быть отрицательным или нулевым',
              1;
    END;

    IF NOT EXISTS (
        SELECT d.id
        FROM department AS d
        WHERE d.id = @p_department_id
    )
    BEGIN
        THROW 50002, N'Выбранного отдела не существует', 1;
    END;

    SELECT @val_chief_id = e.id
    FROM employee AS e
    WHERE @p_department_id = e.department_id
      AND e.chief_id IS NULL;

    SELECT e.id, e.salary AS old_salary
    INTO #old_salaries
    FROM employee AS e
    WHERE @p_department_id = e.department_id;

    UPDATE e
    SET salary = CAST(ROUND(salary * (1 + @percent / 100.0), 0) AS INT)
    FROM employee AS e
    WHERE @p_department_id = e.department_id AND e.id <> @val_chief_id;

    SELECT @max_salary = MAX(e.salary)
    FROM employee AS e
    WHERE @p_department_id = e.department_id;

    IF (
        SELECT e.salary
        FROM employee AS e
        WHERE e.id = @val_chief_id) < @max_salary
    BEGIN
        UPDATE e
        SET salary = @max_salary
        FROM employee AS e
        WHERE e.id = @val_chief_id;
    END;

    SELECT
        e.id AS [ID],
        e.department_id AS [ID Отдела],
        e.chief_id AS [ID Руководителя],
        e.name AS [Сотрудник],
        os.old_salary AS [Старая ЗП],
        e.salary AS [Обновленная ЗП]
    FROM employee AS e
    INNER JOIN #old_salaries AS os
        ON e.id = os.id
    WHERE @p_department_id = e.department_id
    ORDER BY e.id;

    DROP TABLE #old_salaries;
END;
GO

