using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using System.Globalization;
using Topo.Models.Events;
using Topo.Models.MemberList;
using Topo.Models.Milestone;
using Topo.Models.OAS;
using Topo.Models.ReportGeneration;
using Topo.Models.SIA;
using Topo.Models.Logbook;
using Topo.Models.Wallchart;
using Topo.Models.AditionalAwards;
using Topo.Services;
using Topo.Models.Approvals;

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
    public string FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("en-AU");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VVhiQlFadVdJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRdkJiUX9YdHZRRGheVkQ=");

            var requestBody = request.Body;
            Console.WriteLine(requestBody);
            var reportGenerationRequest = JsonConvert.DeserializeObject<ReportGenerationRequest>(requestBody);
            if (reportGenerationRequest != null)
            {
                var workbook = reportService.CreateWorkbookWithSheets(1);
                switch (reportGenerationRequest.ReportType)
                {
                    case ReportType.MemberList:
                        workbook = GenerateMemberListWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.PatrolList:
                        workbook = GeneratePatrolListWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.PatrolSheets:
                        workbook = GeneratePatrolSheetsWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.SignInSheet:
                        workbook = GenerateSignInSheetWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.EventAttendance:
                        workbook = GenerateEventAttendanceWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.Attendance:
                        workbook = GenerateAttendanceReportWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.OASWorksheet:
                        workbook = GenerateOASWorksheetWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.SIA:
                        workbook = GenerateSIAWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.Milestone:
                        workbook = GenerateMilestoneWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.Logbook:
                        workbook = GenerateLogbookWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.Wallchart:
                        workbook = GenerateWallchartWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.AdditionalAwards:
                        workbook = GenerateAdditionalAwardsWorkbook(reportGenerationRequest);
                        break;
                    case ReportType.Aprovals:
                        workbook = GenerateApprovalsWorkbook(reportGenerationRequest);
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

        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        

        return "";
    }

    private IWorkbook GenerateMemberListWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<List<MemberListModel>>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateMemberListWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GeneratePatrolListWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<List<MemberListModel>>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GeneratePatrolListWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName, reportGenerationRequest.IncludeLeaders);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GeneratePatrolSheetsWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<List<MemberListModel>>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GeneratePatrolSheetsWorkbook(reportData, reportGenerationRequest.Section);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GenerateSignInSheetWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<List<MemberListModel>>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateSignInSheetWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName, reportGenerationRequest.EventName);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GenerateEventAttendanceWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<EventListModel>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateEventAttendanceWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GenerateAttendanceReportWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<AttendanceReportModel>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateAttendanceReportWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName, reportGenerationRequest.FromDate, reportGenerationRequest.ToDate
                , reportGenerationRequest.OutputType == OutputType.PDF);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GenerateOASWorksheetWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<List<OASWorksheetAnswers>>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateOASWorksheetWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName, reportGenerationRequest.OutputType == OutputType.PDF, reportGenerationRequest.BreakByPatrol);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GenerateSIAWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<List<SIAProjectListModel>>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateSIAWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName, reportGenerationRequest.OutputType == OutputType.PDF);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GenerateMilestoneWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<List<MilestoneSummaryListModel>>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateMilestoneWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName, reportGenerationRequest.OutputType == OutputType.PDF);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GenerateLogbookWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<List<MemberLogbookReportViewModel>>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateLogbookWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName, reportGenerationRequest.OutputType == OutputType.PDF);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GenerateWallchartWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<List<WallchartItemModel>>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateWallchartWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName, reportGenerationRequest.OutputType == OutputType.PDF);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GenerateAdditionalAwardsWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<AdditionalAwardsReportDataModel>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateAdditionalAwardsWorkbook(reportData.AwardSpecificationsList, reportData.SortedAdditionalAwardsList, reportData.DistinctAwards, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }

    private IWorkbook GenerateApprovalsWorkbook(ReportGenerationRequest reportGenerationRequest)
    {
        var reportData = JsonConvert.DeserializeObject<List<ApprovalsListModel>>(reportGenerationRequest.ReportData);
        if (reportData != null)
        {
            var workbook = reportService.GenerateApprovalsWorkbook(reportData, reportGenerationRequest.GroupName, reportGenerationRequest.Section
                , reportGenerationRequest.UnitName, reportGenerationRequest.FromDate, reportGenerationRequest.ToDate, reportGenerationRequest.GroupByMember
                , reportGenerationRequest.OutputType == OutputType.PDF);
            return workbook;
        }
        return reportService.CreateWorkbookWithSheets(1);
    }
}
