using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.SQLQuery;
using ERP.Web.API.Domain.Interfaces.Accounting;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ERP.Web.API.Domain.Services.Accounting
{
    public class JournalReportService : IJournalReportService
    {
        private readonly TenantContext _db; 
        public JournalReportService(TenantContext db)
        {
            _db = db;
        }
        public IEnumerable<ReportJournalResult> GetLists(string rptBy, string dateFrom, string dateTo,
            string vouFrom, string rptDet, string src, string coaCode, string sort)
        {
            string cteName = "cte_jur_src_final", detFilter = "";

            if (rptBy.ToUpper() == "N" || rptBy.ToUpper() == "DT")
            {
                string wh = "";

                if (rptBy.ToUpper() == "N")
                    dateFrom = dateTo = null;

                if (rptBy.ToUpper() == "DT")
                    vouFrom = null;

                if (!string.IsNullOrEmpty(vouFrom))
                {
                    wh += (rptBy.ToUpper() == "N")
                    ? $"WHERE Code LIKE '{vouFrom.Replace("'", "''")}%'"
                    : $"WHERE Code = '{vouFrom.Replace("'", "''")}'";
                }

                cteName = "cte_det_filter_c";
                detFilter = $@"
                    ,cte_det_filter_c AS (
                        SELECT *
                        FROM cte_jur_src_final {wh}
                    )";

                if (rptDet.ToUpper() == "CR")
                {
                    cteName = "cte_det_filter_cr";
                    detFilter += $@"
                        ,cte_det_filter_cr AS (
                            SELECT *
                            FROM cte_det_filter_c
                            UNION
                            SELECT *
                            FROM cte_jur_src_final TBA
                            WHERE EXISTS (
                                SELECT *
                                FROM (
                                    SELECT *
                                    FROM cte_jur_src_final WHERE RefCode1 LIKE '{vouFrom.Replace("'", "''")}%'
                                    UNION
                                    SELECT *
                                    FROM cte_jur_src_final WHERE RefCode2 LIKE '{vouFrom.Replace("'", "''")}%'
                                    UNION
                                    SELECT *
                                    FROM cte_jur_src_final WHERE RefCode3 LIKE '{vouFrom.Replace("'", "''")}%'
                                    UNION
                                    SELECT *
                                    FROM cte_jur_src_final WHERE RefCode4 LIKE '{vouFrom.Replace("'", "''")}%'
                                ) TBB
                                WHERE TBB.Code = TBA.Code
                            )
                        )";
                }
            }

            string sortBy = sort?.ToUpper() == "N" ? "Code" : "[Date],Code";

            string sql = $@"{SourceJournalQuery.BuildQuery(dateFrom, dateTo, coaCode, null, null, null, src, null)} {detFilter}
                SELECT *
                FROM {cteName}
                ORDER BY {sortBy},[Group],sort_dc,CoaCode";

            return _db.ReportJournalResults.FromSqlRaw(sql).ToList();
        }
    }
}
