
namespace ExpenseReimbursementUI.Models;

public class ProcessedReceipt
{
    public string FileName { get; set; } = "";
    public string? BillNumber { get; set; }
    public string? BillDate { get; set; }
    public decimal? Amount { get; set; }
    public string? ClaimStatus { get; set; }
    public string? ClaimReason { get; set; }
    public string? ExtractedText { get; set; }
    public bool IsSubmitted { get; set; } = false;
    public string? SubmissionId { get; set; }

}

