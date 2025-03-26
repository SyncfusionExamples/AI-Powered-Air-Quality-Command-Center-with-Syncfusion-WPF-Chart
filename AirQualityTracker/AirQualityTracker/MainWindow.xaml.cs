using Syncfusion.UI.Xaml.Charts;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AirQualityTracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            viewModel.ValidateCredential();

            predictionButton.Background = Brushes.Transparent;
            busyIndicator.AnimationType = Syncfusion.Windows.Controls.Notification.AnimationTypes.Flower;

            string countryName = countryTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(countryName))
            {
                viewModel.IsBusy = true; // Set busy state before clearing data

                viewModel.Data?.Clear();
                viewModel.ForeCastData?.Clear();
                viewModel.MapMarkers?.Clear();

                viewModel.CurrentPollutionIndex = viewModel.AIPredictionAccuracy = viewModel.AvgPollution7Days = viewModel.LatestAirQualityStatus = "Loading...";

                await viewModel.FetchAirQualityData(countryName);

                viewModel.IsBusy = false; // Reset after fetching
            }
        }
    }

    private void ForecastButton_Click(object sender, RoutedEventArgs e)
    {
        ApplyAIButtonStyle();
        busyIndicator.AnimationType = Syncfusion.Windows.Controls.Notification.AnimationTypes.Rectangle;
        viewModel?.PredictForecastData();
    }

    private void ApplyAIButtonStyle()
    {
        LinearGradientBrush gradient = new LinearGradientBrush();
        gradient.StartPoint = new Point(0, 0);
        gradient.EndPoint = new Point(1, 1);
        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#99D0ED"), 0));
        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#C2E4F6"), 1));
        predictionButton.Background = gradient;
    }

    private void DateTimeAxis_LabelCreated(object sender, Syncfusion.UI.Xaml.Charts.LabelCreatedEventArgs e)
    {
        DateTimeAxisLabel? dateTimeLabel = e.AxisLabel as DateTimeAxisLabel;
        bool isTransition = dateTimeLabel.IsTransition;

        if (isTransition)
            e.AxisLabel.LabelContent = dateTimeLabel.Position.FromOADate().ToString("MMM-dd");
        else
            e.AxisLabel.LabelContent = dateTimeLabel.Position.FromOADate().ToString("dd");
    }
}