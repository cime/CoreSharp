SET ANSI_WARNINGS OFF

BEGIN TRANSACTION
DECLARE @@Rename nvarchar(MAX)
DECLARE RenameCursor CURSOR FOR

	WITH Partitioned AS
	(
		SELECT 
			Constraint_Type,
			Table_Name,
			Constraint_Name,
			Column_Name,
			ROW_NUMBER() OVER (PARTITION BY Constraint_Name ORDER BY Column_Name) AS NameNumber,
			COUNT(*) OVER (PARTITION BY Constraint_Name) AS NameCount
		FROM (

			SELECT TC.Table_Name, TC.Constraint_Name, CC.Column_Name, 
			(CASE 
				WHEN TC.constraint_type = 'UNIQUE' THEN 'UQ' 
				WHEN TC.constraint_type = 'PRIMARY KEY' THEN 'PK' 
			END) AS Constraint_Type
			FROM information_schema.table_constraints TC
			inner join information_schema.constraint_column_usage CC on TC.Constraint_Name = CC.Constraint_Name
			where TC.constraint_type = 'UNIQUE' OR TC.constraint_type = 'PRIMARY KEY'

		) A
	),
	Concatenated AS
	(
		SELECT Constraint_Type, Table_Name, Constraint_Name, CAST(Column_Name AS NVARCHAR(1024)) AS FullName, Column_Name, NameNumber, NameCount FROM Partitioned WHERE NameNumber = 1

		UNION ALL

		SELECT 
			P.Constraint_Type, P.Table_Name, P.Constraint_Name, CAST(C.FullName + '_' + P.Column_Name AS NVARCHAR(1024)), P.Column_Name, P.NameNumber, P.NameCount
		FROM Partitioned AS P
			INNER JOIN Concatenated AS C ON P.Constraint_Name = C.Constraint_Name AND P.NameNumber = C.NameNumber + 1
	)
	SELECT
		'EXEC sp_rename ''[' + Constraint_Name + ']'', ''' + Constraint_Type + '_' + Table_Name + '_' + FullName + ''', ''OBJECT'''
	FROM Concatenated
	WHERE NameNumber = NameCount


UNION

    SELECT
            'EXEC sp_rename ''[' + c.CONSTRAINT_SCHEMA + '].[' + c.CONSTRAINT_NAME + ']'', ''PK_' + c.TABLE_NAME + ''', ''OBJECT'''
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS c
        WHERE
            c.CONSTRAINT_TYPE = 'PRIMARY KEY'
            AND
            c.TABLE_NAME IS NOT NULL
UNION
	SELECT
		'EXEC sp_rename ''[' + sys.default_constraints.name + ']'', ''DF_' + sys.tables.name + '_' + sys.all_columns.name + ''', ''OBJECT'''
	FROM sys.all_columns
	INNER JOIN sys.tables ON all_columns.object_id = tables.object_id
	INNER JOIN sys.default_constraints ON all_columns.default_object_id = default_constraints.object_id

OPEN RenameCursor
FETCH NEXT
    FROM RenameCursor
    INTO @@Rename
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC sp_executesql @@Rename
    FETCH NEXT
        FROM RenameCursor
        INTO @@Rename
END
CLOSE RenameCursor
DEALLOCATE RenameCursor
COMMIT TRANSACTION
