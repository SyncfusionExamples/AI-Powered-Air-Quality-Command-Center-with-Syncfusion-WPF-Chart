using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AirQualityTracker
{
    public class AirQualityViewModel : INotifyPropertyChanged
    {
        #region Fields

        private AIAirQualityService? airQualityService;
        private string countryName = "New York";
        private bool isEnabled;
        private bool isBusy;
        private ObservableCollection<AirQualityInfo>? data;
        private ObservableCollection<AirQualityInfo>? foreCastData;
        private ObservableCollection<AirQualityInfo>? mapMarkers;
        private string currentPollutionIndex = "Loading...";
        private string avgPollution7Days = "Loading...";
        private string aiPredictionAccuracy = "Loading...";
        private string latestAirQualityStatus = "Loading...";

        #endregion

        #region Properties

        public string CountryName
        {
            get => countryName;
            set
            {
                countryName = value;
                OnPropertyChanged(nameof(CountryName));
            }
        }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }

            set
            {
                isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }

            set
            {
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public ObservableCollection<AirQualityInfo>? Data
        {
            get => data;
            set
            {
                data = value;
                OnPropertyChanged(nameof(Data));
            }
        }

        public ObservableCollection<AirQualityInfo>? ForeCastData
        {
            get => foreCastData;
            set
            {
                foreCastData = value;
                OnPropertyChanged(nameof(ForeCastData));
            }
        }

        public ObservableCollection<AirQualityInfo>? MapMarkers
        {
            get => mapMarkers;
            set
            {
                mapMarkers = value;
                OnPropertyChanged(nameof(MapMarkers));
            }
        }

        public string CurrentPollutionIndex
        {
            get => currentPollutionIndex;
            set
            {
                if (currentPollutionIndex != value)
                {
                    currentPollutionIndex = value;
                    OnPropertyChanged(nameof(CurrentPollutionIndex));
                }
            }
        }

        public string AvgPollution7Days
        {
            get => avgPollution7Days;
            set
            {
                if (avgPollution7Days != value)
                {
                    avgPollution7Days = value;
                    OnPropertyChanged(nameof(AvgPollution7Days));
                }
            }
        }

        public string AIPredictionAccuracy
        {
            get => aiPredictionAccuracy;
            set
            {
                if (aiPredictionAccuracy != value)
                {
                    aiPredictionAccuracy = value;
                    OnPropertyChanged(nameof(AIPredictionAccuracy));
                }
            }
        }

        public string LatestAirQualityStatus
        {
            get => latestAirQualityStatus;
            set
            {
                if (latestAirQualityStatus != value)
                {
                    latestAirQualityStatus = value;
                    OnPropertyChanged(nameof(LatestAirQualityStatus));
                }
            }
        }


        #endregion

        #region Constructor

        public AirQualityViewModel()
        {
            IsBusy = true;
            IsEnabled = true;
        }

        #endregion

        #region Methods

        internal async Task FetchAirQualityData(string countryName)
        {
            airQualityService = new AIAirQualityService();

            IsBusy = true;

            var newData = await airQualityService.PredictAirQualityTrends(countryName);
            Data = new ObservableCollection<AirQualityInfo>(newData);

            var singleMarker = Data.Select(d => new AirQualityInfo
            {
                Latitude = d.Latitude,
                Longitude = d.Longitude
            }).FirstOrDefault();

            if (singleMarker != null)
            {
                MapMarkers = new ObservableCollection<AirQualityInfo> { singleMarker };
            }

            CountryName = countryName;

            UpdateCalculatedProperties();

            IsBusy = false;
        }

        internal async Task PredictForecastData()
        {
            IsBusy = true;

            var historicalData = Data?.OrderByDescending(d => d.Date).Take(40)
                .Select(d => new AirQualityInfo
                {
                    Date = d.Date,
                    PollutionIndex = d.PollutionIndex
                })
                .ToList();

            if (airQualityService != null && historicalData != null)
            {
                var forecastedData = await airQualityService.PredictNextMonthForecast(historicalData);

                ForeCastData = new ObservableCollection<AirQualityInfo>(forecastedData);
            }

            IsBusy = false;
        }

        internal async Task ValidateCredential()
        {
            if(airQualityService != null)
            {
                await airQualityService.ValidateCredential();

                if (!airQualityService.IsValid)
                {
                    IsEnabled = false;
                    CountryName = "New York";
                }
                else
                {
                    IsEnabled = true;
                }
            }
        }

        private void UpdateCalculatedProperties()
        {
            if (!IsBusy)
                return;

            var latestData = Data?.OrderByDescending(d => d.Date).FirstOrDefault();
            CurrentPollutionIndex = latestData != null ? latestData.PollutionIndex.ToString("F0") : "0";

            var last7Days = Data?.OrderByDescending(d => d.Date).Take(7).ToList();
            AvgPollution7Days = (last7Days != null && last7Days.Any())
                ? last7Days.Average(d => d.PollutionIndex).ToString("F2")
                : "0.00";

            AIPredictionAccuracy = (Data != null && Data.Any())
                ? Data.Average(d => d.AIPredictionAccuracy).ToString("F2")
                : "0.00";

            LatestAirQualityStatus = latestData?.AirQualityStatus ?? "Unknown";
        }

        #endregion

        #region Property Changed Event

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}