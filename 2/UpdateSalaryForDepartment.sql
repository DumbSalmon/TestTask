-- FUNCTION: public.updatesalaryfordepartment(integer, integer)

-- DROP FUNCTION IF EXISTS public.updatesalaryfordepartment(integer, integer);

CREATE OR REPLACE FUNCTION public.updatesalaryfordepartment(
	p_department_id integer,
	percent integer)
    RETURNS TABLE(id integer, "ID Отдела" integer, "ID Руководителя" integer, "Сотрудник" character varying, "Старая ЗП" integer, "Обновленная ЗП" integer) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
DECLARE
	val_chief_id INT;
	max_salary INT;
BEGIN
	IF percent <= 0
	THEN RAISE EXCEPTION 'Процент для повышения не может быть отрицательным или нулевым';
	END IF;

	IF NOT EXISTS (SELECT d.id
				   FROM department AS d
				   WHERE d.id = p_department_id)
	THEN RAISE EXCEPTION 'Выбранного отдела % не существует', p_department_id;
	END IF;

	SELECT e.id
	INTO val_chief_id
	FROM employee AS e
	WHERE p_department_id = e.department_id AND e.chief_id IS NULL;

	CREATE TEMP TABLE old_salaries AS
	SELECT e.id, e.salary AS old_salary
	FROM employee AS e
	WHERE p_department_id = e.department_id;

	UPDATE employee AS e
	SET salary = ROUND(salary * (1 + percent/100.0))::INT
	WHERE p_department_id = e.department_id AND e.id <> val_chief_id;

	SELECT MAX(e.salary)
	INTO max_salary
	FROM employee AS e
	WHERE p_department_id = e.department_id;

	IF (SELECT e.salary
		FROM employee AS e
		WHERE e.id = val_chief_id) < max_salary
	THEN UPDATE employee AS e
		 SET salary = max_salary
		 WHERE e.id = val_chief_id;
	END IF;

	RETURN QUERY 
	SELECT 
	e.id AS ID,
	e.department_id AS "ID Отдела",
	e.chief_id AS "ID Руководителя",
	e.name AS "Сотрудник", 
	os.old_salary AS "Старая ЗП",
	e.salary AS "Обновленная ЗП"
	FROM employee AS e
	INNER JOIN old_salaries AS os
	ON e.id = os.id
	WHERE p_department_id = e.department_id
	ORDER BY e.id;

	DROP TABLE old_salaries;

END;
$BODY$;

ALTER FUNCTION public.updatesalaryfordepartment(integer, integer)
    OWNER TO postgres;

