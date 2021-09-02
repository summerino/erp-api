using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateFunctionNumToWord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create function dbo.udf_num_to_words_id
            var sql = @"CREATE FUNCTION [dbo].[udf_num_to_words_id] (

	@value Numeric (38, 2), -- Input number with as many as 18 digits
	@centToWord bit = 0

) RETURNS VARCHAR(8000) 
/*
* Converts a integer number as large as 34 digits into the 
* equivalent words.  The first letter is capitalized.
*
* Attribution: Based on NumberToWords by Srinivas Sampath
*        as revised by Nick Barclay
*
* Example:
select dbo.udf_num_to_words_id (1234567890) + CHAR(10)
      +  dbo.udf_num_to_words_id (0) + CHAR(10)
      +  dbo.udf_num_to_words_id (123) + CHAR(10)
select dbo.udf_num_to_words_id(76543210987654321098765432109876543210)
 
DECLARE @i numeric (38,0)
SET @i = 0
WHILE @I <= 1000 BEGIN 
    PRINT convert (char(5), @i)  
            + convert(varchar(255), dbo.udf_num_to_words_id(@i)) 
    SET @I  = @i + 1 
END
*
* Published as the T-SQL UDF of the Week Vol 2 #9 2/17/03
****************************************************************/
AS BEGIN

Declare @Number Numeric(38,0)
DECLARE @Cents as int
DECLARE @inputNumber VARCHAR(38)
DECLARE @NumbersTable TABLE (number CHAR(2), word VARCHAR(15))
DECLARE @outputString VARCHAR(8000)
DECLARE @length INT
DECLARE @counter INT
DECLARE @loops INT
DECLARE @position INT
DECLARE @chunk CHAR(3) -- for chunks of 3 numbers
DECLARE @tensones CHAR(2)
DECLARE @hundreds CHAR(1)
DECLARE @tens CHAR(1)
DECLARE @ones CHAR(1)

SET @Number = FLOOR(@value)
SET @Cents = 100 * (@value - CONVERT(decimal, @Number))

IF @Number = 0 Return '-'

-- initialize the variables
SELECT @inputNumber = CONVERT(varchar(38), @Number)
     , @outputString = ''
     , @counter = 1
SELECT @length   = LEN(@inputNumber)
     , @position = LEN(@inputNumber) - 2
     , @loops    = LEN(@inputNumber)/3

-- make sure there is an extra loop added for the remaining numbers
IF LEN(@inputNumber) % 3 <> 0 SET @loops = @loops + 1

-- insert data for the numbers and words
INSERT INTO @NumbersTable   SELECT '00', ''
    UNION ALL SELECT '01', 'satu'      UNION ALL SELECT '02', 'dua'
    UNION ALL SELECT '03', 'tiga'    UNION ALL SELECT '04', 'empat'
    UNION ALL SELECT '05', 'lima'     UNION ALL SELECT '06', 'enam'
    UNION ALL SELECT '07', 'tujuh'    UNION ALL SELECT '08', 'delapan'
    UNION ALL SELECT '09', 'sembilan'     UNION ALL SELECT '10', 'sepuluh'
    UNION ALL SELECT '11', 'sebelas'   UNION ALL SELECT '12', 'dua belas'
    UNION ALL SELECT '13', 'tiga belas' UNION ALL SELECT '14', 'empat belas'
    UNION ALL SELECT '15', 'lima belas'  UNION ALL SELECT '16', 'enam belas'
    UNION ALL SELECT '17', 'tujuh belas' UNION ALL SELECT '18', 'delapan belas'
    UNION ALL SELECT '19', 'sembilan belas' UNION ALL SELECT '20', 'dua puluh'
    UNION ALL SELECT '30', 'tiga puluh'   UNION ALL SELECT '40', 'empat puluh'
    UNION ALL SELECT '50', 'lima puluh'    UNION ALL SELECT '60', 'enam puluh'
    UNION ALL SELECT '70', 'tujuh puluh'  UNION ALL SELECT '80', 'delapan puluh'
    UNION ALL SELECT '90', 'sembilan puluh'

WHILE @counter <= @loops BEGIN

	-- get chunks of 3 numbers at a time, padded with leading zeros
	SET @chunk = RIGHT('000' + SUBSTRING(@inputNumber, @position, 3), 3)

	IF @chunk <> '000' BEGIN
		SELECT @tensones = SUBSTRING(@chunk, 2, 2)
		     , @hundreds = SUBSTRING(@chunk, 1, 1)
		     , @tens = SUBSTRING(@chunk, 2, 1)
		     , @ones = SUBSTRING(@chunk, 3, 1)

		-- If twenty or less, use the word directly from @NumbersTable
		IF CONVERT(INT, @tensones) <= 20 OR @Ones='0' BEGIN
			SET @outputString = CASE WHEN  @counter = 2 AND @tensones = '01' AND LEN(@outputString) > 0 THEN ''
								ELSE (SELECT word 
                                    FROM @NumbersTable 
                                    WHERE @tensones = number)
								END
                   + CASE @counter WHEN 1 THEN '' -- No name
					   WHEN 2 THEN CASE WHEN @tensones = '01' AND LEN(@outputString) > 0 THEN 'seribu ' ELSE ' ribu ' END
					   WHEN 3 THEN ' juta '
                       WHEN 4 THEN ' milyar '  WHEN 5 THEN ' triliun '
                       WHEN 6 THEN ' kuadriliun ' WHEN 7 THEN ' kuantiliun '
                       WHEN 8 THEN ' sekstiliun '  WHEN 9 THEN ' septiliun '
                       WHEN 10 THEN ' oktiliun '  WHEN 11 THEN ' noniliun '
                       WHEN 12 THEN ' desiliun '   WHEN 13 THEN ' undesiliun '
                       ELSE '' END
                               + @outputString
		    END
		 ELSE BEGIN -- break down the ones and the tens separately

             SET @outputString = ' ' 
                            + (SELECT word 
                                    FROM @NumbersTable 
                                    WHERE @tens + '0' = number)
					         + ' '
                             + (SELECT word 
                                    FROM @NumbersTable 
                                    WHERE '0'+ @ones = number)
                   + CASE @counter WHEN 1 THEN '' -- No name
                       WHEN 2 THEN ' ribu ' WHEN 3 THEN ' juta '
                       WHEN 4 THEN ' milyar '  WHEN 5 THEN ' triliun '
                       WHEN 6 THEN ' kuadriliun ' WHEN 7 THEN ' kuantiliun '
                       WHEN 8 THEN ' sekstiliun '  WHEN 9 THEN ' septiliun '
                       WHEN 10 THEN ' oktiliun '  WHEN 11 THEN ' noniliun '
                       WHEN 12 THEN ' desiliun '   WHEN 13 THEN ' undesiliun '
                       ELSE '' END
                            + @outputString
		END

		-- now get the hundreds
		IF @hundreds <> '0' BEGIN
			SET @outputString  = CASE WHEN @hundreds = '1' THEN ' seratus ' 
								 ELSE (SELECT word 
                                      FROM @NumbersTable 
                                      WHERE '0' + @hundreds = number)
					            + ' ratus ' END
                                + @outputString
		END
	END

	SELECT @counter = @counter + 1
	     , @position = @position - 3

END

-- Remove any double spaces
SET @outputString = LTRIM(RTRIM(REPLACE(@outputString, '  ', ' ')))
SET @outputstring = UPPER(LEFT(@outputstring, 1)) + SUBSTRING(@outputstring, 2, 8000)

IF (@outputString = 'Satu ribu') SET @outputString = 'Seribu'

IF @Cents > 0 
	IF @centToWord = 0 OR @centToWord IS NULL
		SET @outputstring = @outputstring + ' dan ' + convert(varchar, @Cents) + '/100'
	ELSE
		SET @outputstring = @outputstring + ' dan ' + LOWER(dbo.udf_num_to_words_id(@Cents, default)) + ' sen'

RETURN @outputString -- return the result
END";
            migrationBuilder.Sql(sql);

			// Create procedure dbo.sp_get_si_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_si_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT si_h.*,
			dbo.udf_num_to_words_id(si_h.Total, @centToWord) AS TotalInWord,
			e.Initial AS SalesInitial,
			c.Initial AS CustInitial,
			c.[Name] AS CustName,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.ContactPerson
				ELSE ca_b.ContactPerson END AS CustContactPerson
		FROM (
			SELECT *
			FROM Sales.SalesInvoiceHeader 
			WHERE Code = @code
		) si_h
		LEFT JOIN Sales.SalesOrderHeader so_h
			ON so_h.Code = si_h.SOCode
		LEFT JOIN General.Employee e
			ON e.Id = so_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = si_h.CustCode
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = c.Code
			AND ca_d.IsDefault = 1
			AND c.BillingAddressId IS NULL
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = c.Code
			AND ca_b.Id = c.BillingAddressId
			AND c.BillingAddressId IS NOT NULL
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT si_d.Id,si_d.Code,si_d.DOCode,
			dlv_d.Id AS DlvDetailId, dlv_d.[LineNo] AS DlvDetailLineNo,
			dlv_d.Qty, dlv_d.UnitPrice, dlv_d.Disc, dlv_d.TaxAmount, dlv_d.Total,
			i.Initial AS ItemInitial,
			i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id,Code,DOCode
			FROM Sales.SalesInvoiceDetail
			WHERE Code = @code
		) si_d
		LEFT JOIN Sales.SalesDeliveryHeader dlv_h
			ON dlv_h.Code = si_d.DOCode
		LEFT JOIN Sales.SalesDeliveryDetail dlv_d
			ON dlv_d.Code = dlv_h.Code
		LEFT JOIN Inventory.Item i
			ON i.Id = dlv_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = dlv_d.UnitId
		UNION ALL
		SELECT 0, '', dlv_d_fg.Code,
			dlv_d_fg.DlvOrderDetailId, dlv_d_fg.[LineNo],
			dlv_d_fg.Qty, dlv_d_fg.UnitPrice, dlv_d_fg.UnitPrice, 0, 0,
			i.Initial AS ItemInitial,
			i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT *
			FROM Sales.SalesDeliveryDetailFreeGood
			WHERE EXISTS (
				SELECT Id,Code,DOCode
				FROM Sales.SalesInvoiceDetail
				WHERE Code = @code
				AND DOCode = Code
			)
		) dlv_d_fg
		LEFT JOIN Inventory.Item i
			ON i.Id = dlv_d_fg.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = dlv_d_fg.UnitId
		ORDER BY Sort, Id, DOCode, DlvDetailId, DlvDetailLineNo
	END

END";

		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop function dbo.udf_num_to_words_id
            var sql = "DROP VIEW [dbo].[udf_num_to_words_id]";
            migrationBuilder.Sql(sql);

			// Drop procedure dbo.sp_get_si_print_data
			sql = "DROP PROCEDURE [dbo].[sp_get_si_print_data]";
            migrationBuilder.Sql(sql);
        }
    }
}
