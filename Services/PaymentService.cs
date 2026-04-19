using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayroTech.Services;

public interface IPaymentService
{
    Task<PaymentLinkResult?> CreatePaymentLinkAsync(string description, decimal amount, string remarks);
    Task<PaymentResult?> CreatePaymentIntentAsync(decimal amount, string description, string currency = "PHP");
    Task<PaymentResult?> GetPaymentStatusAsync(string paymentId);
    Task<bool> ProcessRefundAsync(string paymentId, decimal amount, string reason);
}

public class PaymentLinkResult
{
    public string? Id { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Status { get; set; }
}

public class PaymentResult
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Description { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<PaymentService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("PayMongo");
        _logger = logger;

        var secretKey = _configuration["PayMongo:SecretKey"];
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{secretKey}:"));
        
        _httpClient.BaseAddress = new Uri("https://api.paymongo.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Creates a payment link for subscription payments
    /// </summary>
    public async Task<PaymentLinkResult?> CreatePaymentLinkAsync(string description, decimal amount, string remarks)
    {
        try
        {
            // PayMongo amounts are in centavos (PHP * 100)
            var amountInCentavos = (int)(amount * 100);

            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        amount = amountInCentavos,
                        description = description,
                        remarks = remarks,
                        currency = "PHP"
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("links", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<PayMongoLinkResponse>(responseContent);
                _logger.LogInformation("Payment link created: {LinkId}", result?.Data?.Id);
                
                return new PaymentLinkResult
                {
                    Id = result?.Data?.Id,
                    CheckoutUrl = result?.Data?.Attributes?.CheckoutUrl,
                    ReferenceNumber = result?.Data?.Attributes?.ReferenceNumber,
                    Status = result?.Data?.Attributes?.Status
                };
            }

            _logger.LogError("Failed to create payment link. Status: {Status}, Error: {Error}", 
                response.StatusCode, responseContent);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating payment link");
            return null;
        }
    }

    /// <summary>
    /// Creates a payment intent for direct card payments
    /// </summary>
    public async Task<PaymentResult?> CreatePaymentIntentAsync(decimal amount, string description, string currency = "PHP")
    {
        try
        {
            var amountInCentavos = (int)(amount * 100);

            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        amount = amountInCentavos,
                        payment_method_allowed = new[] { "card", "gcash", "grab_pay", "paymaya" },
                        payment_method_options = new { card = new { request_three_d_secure = "any" } },
                        currency = currency,
                        description = description,
                        statement_descriptor = "PAYROTECH"
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("payment_intents", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<PayMongoPaymentIntentResponse>(responseContent);
                _logger.LogInformation("Payment intent created: {PaymentId}", result?.Data?.Id);
                
                return new PaymentResult
                {
                    Id = result?.Data?.Id,
                    Status = result?.Data?.Attributes?.Status,
                    Amount = amount,
                    Currency = currency,
                    Description = description
                };
            }

            _logger.LogError("Failed to create payment intent. Status: {Status}, Error: {Error}",
                response.StatusCode, responseContent);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating payment intent");
            return null;
        }
    }

    /// <summary>
    /// Gets the status of a payment
    /// </summary>
    public async Task<PaymentResult?> GetPaymentStatusAsync(string paymentId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"payments/{paymentId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<PayMongoPaymentResponse>(responseContent);
                
                return new PaymentResult
                {
                    Id = result?.Data?.Id,
                    Status = result?.Data?.Attributes?.Status,
                    Amount = (result?.Data?.Attributes?.Amount ?? 0) / 100m,
                    Currency = result?.Data?.Attributes?.Currency,
                    Description = result?.Data?.Attributes?.Description,
                    PaidAt = result?.Data?.Attributes?.PaidAt
                };
            }

            _logger.LogWarning("Payment not found: {PaymentId}", paymentId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting payment status");
            return null;
        }
    }

    /// <summary>
    /// Process a refund for a payment
    /// </summary>
    public async Task<bool> ProcessRefundAsync(string paymentId, decimal amount, string reason)
    {
        try
        {
            var amountInCentavos = (int)(amount * 100);

            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        amount = amountInCentavos,
                        payment_id = paymentId,
                        reason = reason
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("refunds", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Refund processed for payment {PaymentId}", paymentId);
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to process refund. Status: {Status}, Error: {Error}",
                response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception processing refund");
            return false;
        }
    }
}

#region PayMongo Response Models

internal class PayMongoLinkResponse
{
    [JsonPropertyName("data")]
    public PayMongoLinkData? Data { get; set; }
}

internal class PayMongoLinkData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("attributes")]
    public PayMongoLinkAttributes? Attributes { get; set; }
}

internal class PayMongoLinkAttributes
{
    [JsonPropertyName("checkout_url")]
    public string? CheckoutUrl { get; set; }
    
    [JsonPropertyName("reference_number")]
    public string? ReferenceNumber { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

internal class PayMongoPaymentIntentResponse
{
    [JsonPropertyName("data")]
    public PayMongoPaymentIntentData? Data { get; set; }
}

internal class PayMongoPaymentIntentData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("attributes")]
    public PayMongoPaymentIntentAttributes? Attributes { get; set; }
}

internal class PayMongoPaymentIntentAttributes
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("client_key")]
    public string? ClientKey { get; set; }
}

internal class PayMongoPaymentResponse
{
    [JsonPropertyName("data")]
    public PayMongoPaymentData? Data { get; set; }
}

internal class PayMongoPaymentData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("attributes")]
    public PayMongoPaymentAttributes? Attributes { get; set; }
}

internal class PayMongoPaymentAttributes
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("paid_at")]
    public DateTime? PaidAt { get; set; }
}

#endregion
