using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.SQLQuery;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Model;
using Microsoft.EntityFrameworkCore;
using Syncfusion.XlsIO;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Domain.Services.Accounting;

public class GeneralLedgerReportService : IGeneralLedgerReportService
{
    private readonly TenantContext _db;
    private readonly IWebHostEnvironment _env;

    public GeneralLedgerReportService(TenantContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<Stream> GetDataExcel(IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search, IEnumerable<JournalReportWrapper> wrappedData, bool isMain, string title)
    {
        List<ColumnExcel> Columns = new();

        if (!isMain)
        {
            Columns = new() {
                new() { text= "Kode Akun", value= "AccCode", width = 30},
                new() { text= "Nama Akun", value= "AccName", width = 30},
                new() { text= "Catatan", value= "Notes", width = 30},
                new() { text= "Kode Ref 1", value= "RefCode1", width = 30},
                new(){ text= "Debit", value= "DebetOc", width = 30, isDecimal= true},
                new(){ text= "Kredit", value= "CreditOc", width = 30, isDecimal= true},
                new(){ text= "Kode Ref 2", value= "RefCode2", width = 30},
                new(){ text= "Kode Ref 3", value= "RefCode3", width = 30},
                new(){ text= "Kode Ref 4", value= "RefCode4", width = 30 }
            };
        }
        else
        {
            Columns = new() {
                new() { text= "Tanggal", value= "AccCode", width = 30},
                new() { text= "Kode", value= "AccName", width = 30},
                new() { text= "Catatan", value= "Notes", width = 30},
                new() { text= "Kode Ref 1", value= "RefCode1", width = 30},
                new(){ text= "Debit", value= "DebetOc", width = 30, isDecimal= true},
                new(){ text= "Kredit", value= "CreditOc", width = 30, isDecimal= true},
                new(){ text= "Saldo Akhir", value= "EndBalOc", width = 30, isDecimal= true},
                new(){ text= "Kode Ref 2", value= "RefCode2", width = 30},
                new(){ text= "Kode Ref 3", value= "RefCode3", width = 30},
                new(){ text= "Kode Ref 4", value= "RefCode4", width = 30 }
            };
        }

        var memoryStream = new MemoryStream();

        var company = await _db.Companies.FirstOrDefaultAsync();
        var data = new RequestExcel()
        {
            column = Columns,
            data = wrappedData.ToDynamicList()
        };

        using ExcelEngine excelEngine = new();
        IApplication application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;
        IWorkbook workbook = application.Workbooks.Create(1);
        IWorksheet worksheet = workbook.Worksheets[0];

        worksheet = ExportExcel.ExportExcelGL(worksheet, title, company.Name, search, data, filters, sorts);
        workbook.SaveAs(memoryStream);
        
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    public IEnumerable<GeneralLedgerResult> GetGeneralLedgerLists(string dateFrom, string dateTo, string coaFrom, string coaTo, string currCode, string sort, int? caller)
    {
        string sqlCoa = "";

        if (!(string.IsNullOrEmpty(coaFrom) && string.IsNullOrEmpty(coaTo)))
        {
            coaFrom = string.IsNullOrEmpty(coaFrom) ? coaTo : coaFrom;
            coaTo = string.IsNullOrEmpty(coaTo) ? coaFrom : coaTo;

            int acc1 = int.TryParse(coaFrom, out int tmp) ? tmp : 0;
            int acc2 = int.TryParse(coaTo, out tmp) ? tmp : 0;

            if (!(acc1 == 0 && acc2 == 0))
            {
                if (acc1 > acc2)
                {
                    tmp = acc1;
                    acc1 = acc2;
                    acc2 = tmp;
                }
                coaFrom = acc1.ToString();
                coaTo = acc2.ToString();
            }

            sqlCoa = $@"
                        SELECT DISTINCT Code
                        FROM Accounting.COA
                        WHERE Code BETWEEN '{coaFrom.Replace("'", "''")}' AND '{coaTo.Replace("'", "''")}'";
        }

        string whEndYear = "";

        if (DateTime.TryParse(dateTo, out var dateTemp))
        {
            if (dateTemp.Month == 12 && dateTemp.Day == 31)
                whEndYear = $"AND Code <> 'ENDYEAR-{dateTemp.Year}'";
        }

        if (!string.IsNullOrEmpty(dateFrom))
            dateFrom = dateFrom.Replace("'", "''");

        string cteSource = "cte_jur_src_final";
        string orderBy = (sort == "2" ? "Code,[Date],RefCode1" : "[Date],Code,RefCode1");

        string sql = $@"
                {SourceJournalQuery.BuildQuery(null, dateTo, null, sqlCoa, currCode, null, null, null)}
	            ,cte_begin_src AS (
		            SELECT '' AS Code,null AS [Date],'Saldo Awal' AS Notes
                    ,CoaCode,coaName,'' AS RefCode1,'' AS RefCode2,'' AS RefCode3,'' AS RefCode4
                    ,'' AS CurrCode,null AS rate
                    ,null AS debetOc,null AS creditOc,SUM(debetOc-creditOc) AS endBalOc
                    ,'0' AS sort,'1' AS isBold
                    FROM (
                        SELECT DISTINCT CoaCode,coaName,null AS debetOc,null AS creditOc
						FROM {cteSource}
						UNION ALL
						SELECT CoaCode,coaName,debetOc,creditOc
		                FROM {cteSource}
                        WHERE DATEDIFF(d,[Date],'{dateFrom}') > 0
                    ) TBA
                    GROUP BY CoaCode,coaName
                )
                ,cte_current_src AS (
                    SELECT Code,[Date],Notes
                    ,CoaCode,coaName
                    ,RefCode1,RefCode2,RefCode3,RefCode4
                    ,CurrCode,rate
                    ,debetOc,creditOc
                    ,null AS endBalOc
                    ,CASE WHEN LEFT(Code,8) <> 'ENDYEAR-' THEN '1' ELSE '2' END AS sort
                    ,'0' AS isBold
		            FROM {cteSource}
                    WHERE DATEDIFF(d,[Date],'{dateFrom}') <= 0
                    {whEndYear}
                )
                ,cte_end_src AS (
                    SELECT '' AS Code,null AS [Date],'Sub Total' AS Notes
                    ,CoaCode,coaName
                    ,'' AS RefCode1,'' AS RefCode2,'' AS RefCode3,'' AS RefCode4
                    ,'' AS CurrCode,null AS rate
                    ,SUM(ISNULL(debetOc,0)) AS debetOc
                    ,SUM(ISNULL(creditOc,0)) AS creditOc
                    ,SUM(ISNULL(endBalOc,0) + ISNULL(debetOc,0) - ISNULL(creditOc,0)) AS endBalOc
                    ,'3' AS sort,'1' AS isBold
		            FROM (
                        SELECT * FROM cte_begin_src
                        UNION ALL
                        SELECT * FROM cte_current_src
                    ) TBA
                    GROUP BY CoaCode,coaName
                )
                ,cte_union_src AS (
                    SELECT * FROM cte_begin_src
                    UNION ALL
                    SELECT * FROM cte_current_src
                    UNION ALL
                    SELECT * FROM cte_end_src
                )
                SELECT *
                FROM cte_union_src
                ORDER BY CoaCode,sort,{orderBy}";

        return _db.GeneralLedgerResults.FromSqlRaw(sql).ToList();
    }
}