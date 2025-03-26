using AirQualityTracker;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Windows;

public class AzureAirQualityService
{
    #region Properties

    internal const string Endpoint = "YOUR_END_POINT_NAME";
    internal const string DeploymentName = "DEPLOYMENT_NAME";
    internal const string Key = "API_KEY";
    internal IChatClient? Client { get; set; }
    internal bool IsValid { get; set; }

    #endregion

    #region Constructor

    public AzureAirQualityService()
    {
        ValidateCredential();
    } 

    #endregion

    #region Methods

    internal async Task ValidateCredential()
    {
        GetAzureOpenAIClient();

        try
        {
            if (Client != null)
            {
                IsValid = true;
                await Client!.CompleteAsync("Hello, AI Validation");
            }
            else
            {
                IsValid = false;
                MessageBox.Show("Invalid Credential , The data has been retrieved from the previously loaded JSON file.");
            }
        }
        catch (Exception)
        {
            IsValid = false;
            MessageBox.Show("Invalid Credential , The data has been retrieved from the previously loaded JSON file.");
        }
    }

    internal async Task<List<AirQualityInfo>> PredictAirQualityTrends(string location)
    {
        try
        {
            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

            var systemMessage = "You are an AI model specialized in air pollution forecasting and environmental analysis. " +
                                "Your task is to generate a realistic dataset for the past 30 days (" + startDate + " to " + today + ") " +
                                "for the specified location. The data should include daily air quality trends.";

            var userMessage = $"Generate a JSON-formatted dataset for the past 30 days for {location}. " +
                              "Ensure that the output follows this structured format:\n\n" +
                              "[\n" +
                              "  {\n" +
                              "    \"Date\": \"YYYY-MM-DD\",\n" +
                              "    \"PollutionIndex\": \"Air Quality Index (0-500)\",\n" +
                              "    \"AirQualityStatus\": \"Good | Satisfactory | Moderate | Poor | Very Poor | Severe \",\n" +
                              "    \"Latitude\": \"decimal\",\n" +
                              "    \"Longitude\": \"decimal\"\n" +
                              "    \"AIPredictionAccuracy\": \"Confidence score (0-85)\"\n" +
                              "  }\n" +
                              "]\n\n" +
                              "The generated data should be realistic and reflect environmental patterns.";

            string response = await GetAnswerFromGPT(systemMessage + "\n\n" + userMessage);
            string extractedJson = JsonExtractor.ExtractJson(response);

            return !string.IsNullOrEmpty(extractedJson)
                ? JsonSerializer.Deserialize<List<AirQualityInfo>>(extractedJson) ?? new List<AirQualityInfo>()
                : new List<AirQualityInfo>();
        }
        catch (Exception)
        {
            return GetCurrentDataFromEmbeddedJson();
        }
    }

    internal async Task<List<AirQualityInfo>> PredictNextMonthForecast(List<AirQualityInfo> historicalData)
    {
        try
        {
            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string futureDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");

            var systemMessage = "You are an AI model specialized in air pollution forecasting. " +
                                "Based on the provided historical data, generate an accurate prediction " +
                                "for air quality trends over the next 30 days (" + today + " to " + futureDate + ").";

            var userMessage = $"Using the following historical dataset, predict the Pollution Index for the next 30 days:\n\n" +
                              $"{JsonSerializer.Serialize(historicalData)}\n\n" +
                              "Ensure the output follows this structured format:\n\n" +
                              "[\n" +
                              "  {\n" +
                              "    \"Date\": \"YYYY-MM-DD\",\n" +
                              "    \"PollutionIndex\": \"Air Quality Index (0-500)\"\n" +
                              "  }\n" +
                              "]\n\n" +
                              "Ensure that predictions are realistic and follow previous trends.";

            string response = await GetAnswerFromGPT(systemMessage + "\n\n" + userMessage);
            string extractedJson = JsonExtractor.ExtractJson(response);

            return !string.IsNullOrEmpty(extractedJson)
                ? JsonSerializer.Deserialize<List<AirQualityInfo>>(extractedJson) ?? new List<AirQualityInfo>()
                : new List<AirQualityInfo>();
        }
        catch (Exception)
        {
            return GetPredictionFromEmbeddedJson();
        }
    }

    private void GetAzureOpenAIClient()
    {
        try
        {
            var client = new AzureOpenAIClient(new Uri(Endpoint), new AzureKeyCredential(Key)).AsChatClient(modelId: DeploymentName);
            this.Client = client;
        }
        catch (Exception)
        {
        }
    }

    private async Task<string> GetAnswerFromGPT(string userPrompt)
    {
        try
        {
            if (Client != null)
            {
                var response = await Client.CompleteAsync(userPrompt);
                return response.ToString();
            }
        }
        catch
        {
            return "";
        }

        return "";
    }

    private List<AirQualityInfo> GetCurrentDataFromEmbeddedJson()
    {
        var executingAssembly = typeof(App).GetTypeInfo().Assembly;

        using (var stream = executingAssembly.GetManifestResourceStream("AirQualityTracker.Resources.current_data.json"))
        using (var textStream = new StreamReader(stream))
        {
            // Read the JSON content from the embedded resource
            string json = textStream.ReadToEnd();

            return JsonSerializer.Deserialize<List<AirQualityInfo>>(json) ?? new List<AirQualityInfo>();
        }
    }

    private List<AirQualityInfo> GetPredictionFromEmbeddedJson()
    {
        var executingAssembly = typeof(App).GetTypeInfo().Assembly;

        using (var stream = executingAssembly.GetManifestResourceStream("AirQualityTracker.Resources.prediction_data.json"))
        using (var textStream = new StreamReader(stream))
        {
            // Read the JSON content from the embedded resource
            string json = textStream.ReadToEnd();

            return JsonSerializer.Deserialize<List<AirQualityInfo>>(json) ?? new List<AirQualityInfo>();
        }
    } 

    #endregion
}

public class JsonExtractor
{
    public static string ExtractJson(string response)
    {
        try
        {
            Match match = Regex.Match(response, @"\[.*?\]", RegexOptions.Singleline);

            if (match.Success && !string.IsNullOrWhiteSpace(match.Value))
            {
                string json = match.Value.Trim();
                return json;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error extracting JSON: {ex.Message}");
        }

        return "Invalid or No JSON Found";
    }
}