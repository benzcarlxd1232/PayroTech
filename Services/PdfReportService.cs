using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Borders;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace PayroTech.Services;

public interface IPdfReportService
{
    /// <summary>Generate a password-protected PDF report.</summary>
    byte[] GeneratePayrollSummary(string companyName, List<PayrollReportRow> rows, string period, string password);
    byte[] GenerateEmployeeMasterlist(string companyName, List<EmployeeReportRow> rows, string password);
    byte[] GenerateAttendanceReport(string companyName, List<AttendanceReportRow> rows, string period, string password);
    byte[] GenerateGovernmentReport(string companyName, List<GovernmentReportRow> rows, string period, string password);
    byte[] GenerateAuditLogReport(string companyName, List<AuditLogReportRow> rows, string period, string password);
    byte[] GenerateSuperAdminCompanyReport(List<CompanyReportRow> rows, string password);
    byte[] GenerateSuperAdminRevenueReport(List<RevenueReportRow> rows, string period, string password);
    byte[] GenerateSuperAdminActivityReport(List<AuditLogReportRow> rows, string period, string password);
}

public class PdfReportService : IPdfReportService
{
    private static readonly DeviceRgb HeaderBg = new(15, 23, 42);    // slate-900
    private static readonly DeviceRgb AccentColor = new(8, 145, 178); // cyan-600
    private static readonly DeviceRgb LightBg = new(248, 250, 252);   // slate-50
    private static readonly DeviceRgb BorderColor = new(226, 232, 240);

    private byte[] EncryptPdf(byte[] pdfBytes, string password)
    {
        using var input = new MemoryStream(pdfBytes);
        using var output = new MemoryStream();

        var readerProps = new ReaderProperties();
        using var reader = new PdfReader(input, readerProps);

        var writerProps = new WriterProperties()
            .SetStandardEncryption(
                System.Text.Encoding.UTF8.GetBytes(password),
                System.Text.Encoding.UTF8.GetBytes(password),
                EncryptionConstants.ALLOW_PRINTING | EncryptionConstants.ALLOW_COPY,
                EncryptionConstants.ENCRYPTION_AES_256);

        using var writer = new PdfWriter(output, writerProps);
        using var pdfDoc = new PdfDocument(reader, writer);
        pdfDoc.Close();

        return output.ToArray();
    }

    private byte[] BuildPdf(Action<Document, PdfFont, PdfFont> buildContent, string password)
    {
        using var ms = new MemoryStream();
        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        using (var doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4))
        {
            doc.SetMargins(36, 36, 36, 36);
            var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            buildContent(doc, fontNormal, fontBold);
        }
        return EncryptPdf(ms.ToArray(), password);
    }

    private void AddHeader(Document doc, PdfFont fontBold, string title, string subtitle, string? period = null)
    {
        var header = new Div()
            .SetBackgroundColor(HeaderBg)
            .SetPadding(20)
            .SetMarginBottom(20);

        header.Add(new Paragraph("PayroTech")
            .SetFont(fontBold).SetFontSize(22).SetFontColor(ColorConstants.WHITE));
        header.Add(new Paragraph(title)
            .SetFont(fontBold).SetFontSize(14).SetFontColor(new DeviceRgb(148, 163, 184)));
        header.Add(new Paragraph(subtitle + (period != null ? $" — {period}" : ""))
            .SetFontSize(10).SetFontColor(new DeviceRgb(100, 116, 139)));
        header.Add(new Paragraph($"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
            .SetFontSize(8).SetFontColor(new DeviceRgb(100, 116, 139)).SetMarginTop(4));

        doc.Add(header);
    }

    private void AddFooter(Document doc, PdfFont font)
    {
        doc.Add(new Paragraph("\n"));
        doc.Add(new Paragraph("This document is password-protected and confidential. Unauthorized distribution is prohibited.")
            .SetFont(font).SetFontSize(7).SetFontColor(new DeviceRgb(148, 163, 184))
            .SetTextAlignment(TextAlignment.CENTER));
        doc.Add(new Paragraph($"© {DateTime.Now.Year} PayroTech — Payroll & HR Management System")
            .SetFont(font).SetFontSize(7).SetFontColor(new DeviceRgb(148, 163, 184))
            .SetTextAlignment(TextAlignment.CENTER));
    }

    private Table CreateTable(float[] colWidths)
    {
        var table = new Table(UnitValue.CreatePercentArray(colWidths))
            .UseAllAvailableWidth()
            .SetMarginBottom(16);
        return table;
    }

    private Cell HeaderCell(string text, PdfFont fontBold)
    {
        return new Cell().Add(new Paragraph(text).SetFont(fontBold).SetFontSize(8).SetFontColor(ColorConstants.WHITE))
            .SetBackgroundColor(AccentColor).SetPadding(6).SetBorder(Border.NO_BORDER);
    }

    private Cell DataCell(string text, PdfFont font, bool alt = false)
    {
        var cell = new Cell().Add(new Paragraph(text).SetFont(font).SetFontSize(8))
            .SetPadding(5).SetBorder(new SolidBorder(BorderColor, 0.5f));
        if (alt) cell.SetBackgroundColor(LightBg);
        return cell;
    }

    // ═══════════════════════════════════════════════════════════════════
    // MANAGER REPORTS
    // ═══════════════════════════════════════════════════════════════════

    public byte[] GeneratePayrollSummary(string companyName, List<PayrollReportRow> rows, string period, string password)
    {
        return BuildPdf((doc, font, fontBold) =>
        {
            AddHeader(doc, fontBold, companyName, "Payroll Summary Report", period);

            // Summary stats
            var totalGross = rows.Sum(r => r.GrossPay);
            var totalNet = rows.Sum(r => r.NetPay);
            var totalDeductions = rows.Sum(r => r.TotalDeductions);

            doc.Add(new Paragraph($"Total Employees: {rows.Count}  |  Gross Pay: ₱{totalGross:N2}  |  Deductions: ₱{totalDeductions:N2}  |  Net Pay: ₱{totalNet:N2}")
                .SetFont(fontBold).SetFontSize(9).SetMarginBottom(12));

            var table = CreateTable(new float[] { 3, 2, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f });
            table.AddHeaderCell(HeaderCell("Employee", fontBold));
            table.AddHeaderCell(HeaderCell("Department", fontBold));
            table.AddHeaderCell(HeaderCell("Days", fontBold));
            table.AddHeaderCell(HeaderCell("Gross Pay", fontBold));
            table.AddHeaderCell(HeaderCell("Deductions", fontBold));
            table.AddHeaderCell(HeaderCell("Gov't", fontBold));
            table.AddHeaderCell(HeaderCell("Net Pay", fontBold));

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i]; bool alt = i % 2 == 1;
                table.AddCell(DataCell(r.EmployeeName, font, alt));
                table.AddCell(DataCell(r.Department, font, alt));
                table.AddCell(DataCell(r.DaysWorked.ToString("F1"), font, alt));
                table.AddCell(DataCell($"₱{r.GrossPay:N2}", font, alt));
                table.AddCell(DataCell($"₱{r.TotalDeductions:N2}", font, alt));
                table.AddCell(DataCell($"₱{r.GovContributions:N2}", font, alt));
                table.AddCell(DataCell($"₱{r.NetPay:N2}", font, alt));
            }
            doc.Add(table);
            AddFooter(doc, font);
        }, password);
    }

    public byte[] GenerateEmployeeMasterlist(string companyName, List<EmployeeReportRow> rows, string password)
    {
        return BuildPdf((doc, font, fontBold) =>
        {
            AddHeader(doc, fontBold, companyName, "Employee Masterlist");

            doc.Add(new Paragraph($"Total Active Employees: {rows.Count}")
                .SetFont(fontBold).SetFontSize(9).SetMarginBottom(12));

            var table = CreateTable(new float[] { 1.5f, 3, 3, 2, 2, 2 });
            table.AddHeaderCell(HeaderCell("Emp #", fontBold));
            table.AddHeaderCell(HeaderCell("Full Name", fontBold));
            table.AddHeaderCell(HeaderCell("Email", fontBold));
            table.AddHeaderCell(HeaderCell("Department", fontBold));
            table.AddHeaderCell(HeaderCell("Position", fontBold));
            table.AddHeaderCell(HeaderCell("Hire Date", fontBold));

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i]; bool alt = i % 2 == 1;
                table.AddCell(DataCell(r.EmployeeNumber, font, alt));
                table.AddCell(DataCell(r.FullName, font, alt));
                table.AddCell(DataCell(r.Email, font, alt));
                table.AddCell(DataCell(r.Department, font, alt));
                table.AddCell(DataCell(r.Role, font, alt));
                table.AddCell(DataCell(r.HireDate, font, alt));
            }
            doc.Add(table);
            AddFooter(doc, font);
        }, password);
    }

    public byte[] GenerateAttendanceReport(string companyName, List<AttendanceReportRow> rows, string period, string password)
    {
        return BuildPdf((doc, font, fontBold) =>
        {
            AddHeader(doc, fontBold, companyName, "Attendance Report", period);

            var table = CreateTable(new float[] { 3, 2, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f });
            table.AddHeaderCell(HeaderCell("Employee", fontBold));
            table.AddHeaderCell(HeaderCell("Department", fontBold));
            table.AddHeaderCell(HeaderCell("Present", fontBold));
            table.AddHeaderCell(HeaderCell("Late", fontBold));
            table.AddHeaderCell(HeaderCell("Absent", fontBold));
            table.AddHeaderCell(HeaderCell("OT Hrs", fontBold));
            table.AddHeaderCell(HeaderCell("Late Mins", fontBold));

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i]; bool alt = i % 2 == 1;
                table.AddCell(DataCell(r.EmployeeName, font, alt));
                table.AddCell(DataCell(r.Department, font, alt));
                table.AddCell(DataCell(r.PresentDays.ToString(), font, alt));
                table.AddCell(DataCell(r.LateDays.ToString(), font, alt));
                table.AddCell(DataCell(r.AbsentDays.ToString(), font, alt));
                table.AddCell(DataCell(r.OtHours.ToString("F1"), font, alt));
                table.AddCell(DataCell(r.LateMinutes.ToString(), font, alt));
            }
            doc.Add(table);
            AddFooter(doc, font);
        }, password);
    }

    public byte[] GenerateGovernmentReport(string companyName, List<GovernmentReportRow> rows, string period, string password)
    {
        return BuildPdf((doc, font, fontBold) =>
        {
            AddHeader(doc, fontBold, companyName, "Government Compliance Report", period);

            var totalSSS = rows.Sum(r => r.SSS);
            var totalPhil = rows.Sum(r => r.PhilHealth);
            var totalPag = rows.Sum(r => r.PagIbig);
            var totalTax = rows.Sum(r => r.Tax);

            doc.Add(new Paragraph($"SSS: ₱{totalSSS:N2}  |  PhilHealth: ₱{totalPhil:N2}  |  Pag-IBIG: ₱{totalPag:N2}  |  Tax: ₱{totalTax:N2}")
                .SetFont(fontBold).SetFontSize(9).SetMarginBottom(12));

            var table = CreateTable(new float[] { 3, 2, 1.5f, 1.5f, 1.5f, 1.5f });
            table.AddHeaderCell(HeaderCell("Employee", fontBold));
            table.AddHeaderCell(HeaderCell("Department", fontBold));
            table.AddHeaderCell(HeaderCell("SSS", fontBold));
            table.AddHeaderCell(HeaderCell("PhilHealth", fontBold));
            table.AddHeaderCell(HeaderCell("Pag-IBIG", fontBold));
            table.AddHeaderCell(HeaderCell("W/Tax", fontBold));

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i]; bool alt = i % 2 == 1;
                table.AddCell(DataCell(r.EmployeeName, font, alt));
                table.AddCell(DataCell(r.Department, font, alt));
                table.AddCell(DataCell($"₱{r.SSS:N2}", font, alt));
                table.AddCell(DataCell($"₱{r.PhilHealth:N2}", font, alt));
                table.AddCell(DataCell($"₱{r.PagIbig:N2}", font, alt));
                table.AddCell(DataCell($"₱{r.Tax:N2}", font, alt));
            }
            doc.Add(table);
            AddFooter(doc, font);
        }, password);
    }

    public byte[] GenerateAuditLogReport(string companyName, List<AuditLogReportRow> rows, string period, string password)
    {
        return BuildPdf((doc, font, fontBold) =>
        {
            AddHeader(doc, fontBold, companyName, "Activity & Audit Log Report", period);

            var table = CreateTable(new float[] { 2.5f, 2.5f, 3, 2, 2 });
            table.AddHeaderCell(HeaderCell("Date/Time", fontBold));
            table.AddHeaderCell(HeaderCell("User", fontBold));
            table.AddHeaderCell(HeaderCell("Action", fontBold));
            table.AddHeaderCell(HeaderCell("Category", fontBold));
            table.AddHeaderCell(HeaderCell("IP Address", fontBold));

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i]; bool alt = i % 2 == 1;
                table.AddCell(DataCell(r.DateTime, font, alt));
                table.AddCell(DataCell(r.UserName, font, alt));
                table.AddCell(DataCell(r.Action, font, alt));
                table.AddCell(DataCell(r.Category, font, alt));
                table.AddCell(DataCell(r.IpAddress, font, alt));
            }
            doc.Add(table);
            AddFooter(doc, font);
        }, password);
    }

    // ═══════════════════════════════════════════════════════════════════
    // SUPERADMIN REPORTS
    // ═══════════════════════════════════════════════════════════════════

    public byte[] GenerateSuperAdminCompanyReport(List<CompanyReportRow> rows, string password)
    {
        return BuildPdf((doc, font, fontBold) =>
        {
            AddHeader(doc, fontBold, "PayroTech Platform", "Company Directory Report");

            doc.Add(new Paragraph($"Total Registered Companies: {rows.Count}")
                .SetFont(fontBold).SetFontSize(9).SetMarginBottom(12));

            var table = CreateTable(new float[] { 3, 2, 1.5f, 2, 2, 1.5f });
            table.AddHeaderCell(HeaderCell("Company", fontBold));
            table.AddHeaderCell(HeaderCell("Industry", fontBold));
            table.AddHeaderCell(HeaderCell("Employees", fontBold));
            table.AddHeaderCell(HeaderCell("Subscription", fontBold));
            table.AddHeaderCell(HeaderCell("Registered", fontBold));
            table.AddHeaderCell(HeaderCell("Status", fontBold));

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i]; bool alt = i % 2 == 1;
                table.AddCell(DataCell(r.CompanyName, font, alt));
                table.AddCell(DataCell(r.Industry, font, alt));
                table.AddCell(DataCell(r.EmployeeCount.ToString(), font, alt));
                table.AddCell(DataCell(r.Subscription, font, alt));
                table.AddCell(DataCell(r.RegisteredDate, font, alt));
                table.AddCell(DataCell(r.Status, font, alt));
            }
            doc.Add(table);
            AddFooter(doc, font);
        }, password);
    }

    public byte[] GenerateSuperAdminRevenueReport(List<RevenueReportRow> rows, string period, string password)
    {
        return BuildPdf((doc, font, fontBold) =>
        {
            AddHeader(doc, fontBold, "PayroTech Platform", "Revenue Report", period);

            var totalRevenue = rows.Sum(r => r.Amount);
            doc.Add(new Paragraph($"Total Revenue: ₱{totalRevenue:N2}  |  Transactions: {rows.Count}")
                .SetFont(fontBold).SetFontSize(9).SetMarginBottom(12));

            var table = CreateTable(new float[] { 2.5f, 3, 2, 2, 2 });
            table.AddHeaderCell(HeaderCell("Date", fontBold));
            table.AddHeaderCell(HeaderCell("Company", fontBold));
            table.AddHeaderCell(HeaderCell("Plan", fontBold));
            table.AddHeaderCell(HeaderCell("Amount", fontBold));
            table.AddHeaderCell(HeaderCell("Status", fontBold));

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i]; bool alt = i % 2 == 1;
                table.AddCell(DataCell(r.Date, font, alt));
                table.AddCell(DataCell(r.CompanyName, font, alt));
                table.AddCell(DataCell(r.Plan, font, alt));
                table.AddCell(DataCell($"₱{r.Amount:N2}", font, alt));
                table.AddCell(DataCell(r.Status, font, alt));
            }
            doc.Add(table);
            AddFooter(doc, font);
        }, password);
    }

    public byte[] GenerateSuperAdminActivityReport(List<AuditLogReportRow> rows, string period, string password)
    {
        return BuildPdf((doc, font, fontBold) =>
        {
            AddHeader(doc, fontBold, "PayroTech Platform", "System Activity Report", period);

            var table = CreateTable(new float[] { 2.5f, 2.5f, 3, 2, 2 });
            table.AddHeaderCell(HeaderCell("Date/Time", fontBold));
            table.AddHeaderCell(HeaderCell("User", fontBold));
            table.AddHeaderCell(HeaderCell("Action", fontBold));
            table.AddHeaderCell(HeaderCell("Category", fontBold));
            table.AddHeaderCell(HeaderCell("IP Address", fontBold));

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i]; bool alt = i % 2 == 1;
                table.AddCell(DataCell(r.DateTime, font, alt));
                table.AddCell(DataCell(r.UserName, font, alt));
                table.AddCell(DataCell(r.Action, font, alt));
                table.AddCell(DataCell(r.Category, font, alt));
                table.AddCell(DataCell(r.IpAddress, font, alt));
            }
            doc.Add(table);
            AddFooter(doc, font);
        }, password);
    }
}

// ═══════════════════════════════════════════════════════════════════
// REPORT DTOs
// ═══════════════════════════════════════════════════════════════════

public class PayrollReportRow
{
    public string EmployeeName { get; set; } = "";
    public string Department { get; set; } = "";
    public decimal DaysWorked { get; set; }
    public decimal GrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal GovContributions { get; set; }
    public decimal NetPay { get; set; }
}

public class EmployeeReportRow
{
    public string EmployeeNumber { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Department { get; set; } = "";
    public string Role { get; set; } = "";
    public string HireDate { get; set; } = "";
}

public class AttendanceReportRow
{
    public string EmployeeName { get; set; } = "";
    public string Department { get; set; } = "";
    public int PresentDays { get; set; }
    public int LateDays { get; set; }
    public int AbsentDays { get; set; }
    public decimal OtHours { get; set; }
    public int LateMinutes { get; set; }
}

public class GovernmentReportRow
{
    public string EmployeeName { get; set; } = "";
    public string Department { get; set; } = "";
    public decimal SSS { get; set; }
    public decimal PhilHealth { get; set; }
    public decimal PagIbig { get; set; }
    public decimal Tax { get; set; }
}

public class AuditLogReportRow
{
    public string DateTime { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Action { get; set; } = "";
    public string Category { get; set; } = "";
    public string IpAddress { get; set; } = "";
}

public class CompanyReportRow
{
    public string CompanyName { get; set; } = "";
    public string Industry { get; set; } = "";
    public int EmployeeCount { get; set; }
    public string Subscription { get; set; } = "";
    public string RegisteredDate { get; set; } = "";
    public string Status { get; set; } = "";
}

public class RevenueReportRow
{
    public string Date { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string Plan { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
}
