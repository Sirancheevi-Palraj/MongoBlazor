using ClosedXML.Excel;
namespace MongoBlazor.Model
{
    public class ExcelExporter
    {
        public static void Export(List<TransactionData> data)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Transactions");

            sheet.Cell(1, 1).Value = "TrackingId";
            sheet.Cell(1, 2).Value = "Status";
            sheet.Cell(1, 3).Value = "Request Time";
            sheet.Cell(1, 4).Value = "Response Time";
            sheet.Cell(1, 5).Value = "Time Taken";

            for (int i = 0; i < data.Count; i++)
            {
                var r = i + 2;
                sheet.Cell(r, 1).Value = data[i].TrackingId;
                sheet.Cell(r, 2).Value = data[i].Status;
                sheet.Cell(r, 3).Value = data[i].RequestDateTime;
                sheet.Cell(r, 4).Value = data[i].ResponseDateTime;
                sheet.Cell(r, 5).Value = data[i].TimeTaken.ToString();
            }

            workbook.SaveAs($"Transactions_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }
}
