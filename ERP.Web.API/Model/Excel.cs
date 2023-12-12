using ERP.Common.Models;
using Syncfusion.XlsIO;

namespace ERP.Web.API.Model
{
    public class ColumnExcel
    {
        public string text { get; set; }
        public string value { get; set; }
        public int width { get; set; }
        public bool isNumber { get; set; }
        public bool isDecimal { get; set; }
        public bool isDateTime { get; set; }
        public bool isDate { get; set; }
        public bool autofit { get; set; }
    }

    public class RequestExcel
    {
        public List<ColumnExcel> column;
        public List<dynamic> data;
    }

    public static class ExportExcel
    {
        public static IWorksheet ExportExcelGL(IWorksheet worksheet, string moduleName, string title,
            string search,
            RequestExcel data,
            IEnumerable<Filter> filters,
            IEnumerable<Sort> sorts)
        {
            var filterString = "";
            if (search != null)
                // filterString += $"Pencarian dengan kata kunci {search}; ";
                filterString += $"Search by keyword {search}; ";

            foreach (var filter in filters)
            {
                filter.Operator = filter.Operator
                    .Replace("eq", "Equals")
                    .Replace("neq", "Not Equals")
                    .Replace("startswith", "Starts With")
                    .Replace("endswith", "Ends With")
                    .Replace("doesnotcontain", "Not Contains");
                if (filter.Keyword is DateTime)
                {
                    filter.Field = filter.Field
                        .Replace(".Value.Date", "")
                        .Replace(".Value", "")
                        .Replace(".Date", "");
                }


                filterString += $"{filter.Field} {filter.Operator} {filter.Keyword}; ";
            }

            // foreach (var sort in sorts)
            // {
            //     filterString += $"{sort.Field} {sort.Direction}; ";
            // }

            worksheet.Range[$"A1"].Text = title;
            worksheet.Range[$"A1"].CellStyle.Font.Size = 18;
            worksheet.Range[$"A1"].CellStyle.Font.Bold = true;

            worksheet.Range[$"A2"].Text = moduleName;
            worksheet.Range[$"A1"].CellStyle.Font.Size = 13;
            worksheet.Range[$"A1"].CellStyle.Font.Bold = true;
            worksheet.Range[$"A3"].Text = filterString;
            worksheet.Range[$"A5"].Text = $"{data.data.Count} data";
            worksheet.Name = moduleName;

            for (int i = 1; i <= data.column.Count; i++)
            {
                worksheet.Range[6, i].Value = data.column[i - 1].text;
                worksheet.Range[6, i].CellStyle.Color = Syncfusion.Drawing.Color.Gray;
                worksheet.Range[6, i].CellStyle.Font.Bold = true;
                worksheet.Range[6, i].CellStyle.Font.Color = ExcelKnownColors.White;
                worksheet.Range[6, i].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                worksheet.Range[6, i].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                worksheet.Range[6, i].RowHeight = 20;

                if (data.column[i - 1].width > 0)
                {
                    worksheet.Range[6, i].ColumnWidth = data.column[i - 1].width;
                }
            }

            worksheet.Range[7, 1].FreezePanes();

            for (int i = 0; i < data.data.Count; i++)
            {
                worksheet.Range[i + 7, 1].Value = (i + 1).ToString();
                for (int j = 0; j < data.column.Count; j++)
                {
                    var columnName = data.column[j].value;
                    var propertyInfo = data.data[i].GetType().GetProperty(columnName);
                    if (propertyInfo != null)
                    {
                        var value = propertyInfo.GetValue(data.data[i]);

                        if (IsNumber(propertyInfo.PropertyType) || data.column[j].isNumber)
                        {
                            worksheet.Range[i + 7, j + 1].Value = value == null ? "" : value.ToString();
                            worksheet.Range[i + 7, j + 1].NumberFormat = "#,##0";
                        }
                        else if (IsDecimal(propertyInfo.PropertyType) || data.column[j].isDecimal)
                        {
                            worksheet.Range[i + 7, j + 1].Value = value == null ? "" : value.ToString();
                            worksheet.Range[i + 7, j + 1].NumberFormat = "#,##0.00";
                        }
                        else if (propertyInfo.PropertyType == typeof(DateTime) ||
                                 propertyInfo.PropertyType == typeof(DateTime?) || data.column[j].isDate ||
                                 data.column[j].isDateTime)
                            if (data.column[j].isDate)
                                worksheet.Range[i + 7, j + 1].Text =
                                    value == null ? "" : value.ToString("dd-MMM-yyyy");
                            else
                                worksheet.Range[i + 7, j + 1].Text =
                                    value == null ? "" : value.ToString("dd-MMM-yyyy HH:mm:ss");
                        else
                        {
                            worksheet.Range[i + 7, j + 1].Text =
                                string.IsNullOrWhiteSpace(value) ? "" : value.ToString();
                            worksheet.Range[i + 7, j + 1].NumberFormat = "@";
                        }
                    }
                }
            }

            worksheet.UsedRange.WrapText = false;

            // worksheet.UsedRange.AutofitColumns();

            return worksheet;
        }

        private static bool IsNumber(dynamic value)
        {
            var typeNumber = new List<Type>()
        {
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(byte?),
            typeof(short?),
            typeof(int?),
            typeof(long?),
        };
            return typeNumber.Exists(x => x == value);
        }

        private static bool IsDecimal(dynamic value)
        {
            var typeNumber = new List<Type>()
        {
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(float?),
            typeof(double?),
            typeof(decimal?),
        };
            return typeNumber.Exists(x => x == value);
        }
    }
}
