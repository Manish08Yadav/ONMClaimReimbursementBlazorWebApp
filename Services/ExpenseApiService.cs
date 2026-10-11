using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
namespace ExpenseReimbursementUI.Services;

public sealed class ExpenseApiService
{
    private readonly HttpClient _httpClient;

    public ExpenseApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://127.0.0.1:8000/");
    }

    public async Task<string> UploadReceiptAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        using var multipartContent = new MultipartFormDataContent();

        using var fileStream = file.OpenReadStream(
            maxAllowedSize: 10 * 1024 * 1024);

        using var streamContent = new StreamContent(fileStream);

        streamContent.Headers.ContentType =
            new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType);

        multipartContent.Add(streamContent, "file", file.Name);

        using var response = await _httpClient.PostAsync(
            "onmreimbursement/upload",
            multipartContent,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"API returned {(int)response.StatusCode}: {responseBody}");
        }

        return responseBody;
    }

    

public async Task<AssistantResponse> AskReceiptAssistantAsync(
    string question,
    string extractedText,
    string? billNumber,
    string? billDate,
    decimal? amount,
    string? status,
    string? reason)
{
    var request = new
    {
        question,
        extracted_text = extractedText,
        bill_details = new
        {
            bill_number = billNumber,
            bill_date = billDate,
            total_amount = amount
        },
        claim_result = new { status, reason }
    };

    using var response = await _httpClient.PostAsJsonAsync(
        "onmreimbursement/assistant/ask", request);

    var responseBody = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
        throw new HttpRequestException(
            $"API returned {(int)response.StatusCode}: {responseBody}");

    var result = System.Text.Json.JsonSerializer.Deserialize<AssistantResponse>(
        responseBody,
        new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

    return result ?? new AssistantResponse
    {
        Answer = "The assistant returned no answer."
    };
}






public async Task<string> ReevaluateClaimAsync(
    string? billNumber,
    string? billDate,
    decimal? amount,
    string? correctedBillNumber,
    string? correctedBillDate,
    decimal? correctedAmount)
{
    var billDetails = new
    {
        bill_number = billNumber,
        bill_date = billDate,
        total_amount = amount
    };

    var corrections = new Dictionary<string, object>();

    if (correctedBillNumber != null)
        corrections["bill_number"] = correctedBillNumber;

    if (correctedBillDate != null)
        corrections["bill_date"] = correctedBillDate;

    if (correctedAmount.HasValue)
        corrections["total_amount"] = correctedAmount.Value;

    var request = new
    {
        bill_details = billDetails,
        corrections,
        employee_confirmed = true
    };

    using var response = await _httpClient.PostAsJsonAsync(
        "onmreimbursement/claim/reevaluate", request);

    var responseBody = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException(
            $"API returned {(int)response.StatusCode}: {responseBody}");
    }

    return responseBody;
}


public async Task<string> SubmitClaimAsync(
    string? billNumber,
    string? billDate,
    decimal? amount)
{
    var request = new
    {
        bill_details = new
        {
            bill_number = billNumber,
            bill_date = billDate,
            total_amount = amount
        }
    };

    using var response = await _httpClient.PostAsJsonAsync(
        "onmreimbursement/claim/submit",
        request);

    var responseBody = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException(
            $"API returned {(int)response.StatusCode}: {responseBody}");
    }

    return responseBody;
}

}

public sealed class AssistantResponse
{
    public string Answer { get; set; } = "";
    public Dictionary<string, ProposedCorrection> ProposedCorrections { get; set; } = new();
}
public sealed class ProposedCorrection
{
    public string Value { get; set; } = "";
    public string Evidence { get; set; } = "";
}

