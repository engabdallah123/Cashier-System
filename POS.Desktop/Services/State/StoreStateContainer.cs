using POS.Desktop.Services.Api;

namespace POS.Desktop.Services.State
{
    public class StoreStateContainer
    {
        public string StoreName { get; private set; } = "نظام الكاشير";
        public StoreSettingDto? Settings { get; private set; }
        public bool IsLoaded { get; private set; }

        public event Action? OnStateChanged;

        public void SetSettings(StoreSettingDto settings)
        {
            Settings = settings;
            if (!string.IsNullOrWhiteSpace(settings.StoreName))
            {
                StoreName = settings.StoreName;
            }
            IsLoaded = true;
            NotifyStateChanged();
        }

        public async Task LoadSettingsAsync(PosApiClient apiClient)
        {
            try
            {
                var settings = await apiClient.GetSettingsAsync();
                if (settings != null)
                {
                    SetSettings(settings);
                }
            }
            catch
            {
                // Fallback retains existing StoreName
            }
        }

        private void NotifyStateChanged() => OnStateChanged?.Invoke();
    }
}
