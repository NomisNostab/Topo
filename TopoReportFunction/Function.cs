using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using Topo.Models.MemberList;
using Topo.Models.ReportGeneration;
using Topo.Services;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TopoReportFunction;

public class Function
{
    private ReportService reportService = new ReportService();

    /// <summary>
    /// Returns a Member List Report in a base 64 string of the PDF or XLSX
    /// </summary>
    /// <param name="reportData">A ReportGenerationRequest object containing the report request</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public string FunctionHandler(string reportData, ILambdaContext context)
    {
        try
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VVhiQlFadVdJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRdkJiUX9YdHZRRGheVkQ=");
            var reportGenerationRequest = JsonConvert.DeserializeObject<ReportGenerationRequest>(reportData);
            if (reportGenerationRequest != null)
            {
                var workbook = reportService.CreateWorkbookWithSheets(1);
                switch (reportGenerationRequest.ReportType)
                {
                    case ReportType.MemberList:
                        workbook = GenerateMemberListWorkbook(reportGenerationRequest);
                        break;
                }
                
                if (workbook != null)
                {
                    MemoryStream strm = new MemoryStream();
                    workbook.Version = ExcelVersion.Excel2016;

                    if (reportGenerationRequest.OutputType == OutputType.PDF)
                    {
                        //Initialize XlsIO renderer.
                        XlsIORenderer renderer = new XlsIORenderer();

                        //Convert Excel document into PDF document 
                        PdfDocument pdfDocument = renderer.ConvertToPDF(workbook);
                        pdfDocument.Save(strm);
                    }

                    if (reportGenerationRequest.OutputType == OutputType.Excel)
                    {
                        //Stream as Excel file
                        workbook.SaveAs(strm);
                    }

                    // return stream in browser
                    return Convert.ToBase64String(strm.ToArray());

                }
            }
        }

        catch
        {

        }
        

        return "";
    }

    private IWorkbook GenerateMemberListWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var memberListReportData = JsonConvert.DeserializeObject<List<MemberListModel>>(reportGenerationRequest.ReportData);
        if (memberListReportData != null)
        {
            var workbook = reportService.GenerateMemberListWorkbook(memberListReportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section, reportGenerationRequest.UnitName);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }
}
