using System.Net.Http;
using System.Net.Http.Json;

namespace POS.Desktop.Services.Api
{
    public class PosApiClient
    {
        private readonly HttpClient _http;

        public PosApiClient(HttpClient http)
        {
            _http = http;
        }

        // Auth
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var res = await _http.PostAsJsonAsync("api/auth/login", request);
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<AuthResponse>() : null;
        }

        // Products
        public async Task<List<ProductDto>> GetProductsAsync(string? search = null)
        {
            var url = string.IsNullOrWhiteSpace(search) ? "api/inventory/products" : $"api/inventory/products?searchTerm={search}";
            return await _http.GetFromJsonAsync<List<ProductDto>>(url) ?? new();
        }

        public async Task<ProductDto?> GetProductByBarcodeAsync(string barcode)
        {
            try
            {
                return await _http.GetFromJsonAsync<ProductDto>($"api/inventory/products/barcode/{barcode}");
            }
            catch
            {
                return null;
            }
        }

        // Categories & Units
        public async Task<List<CategoryDto>> GetCategoriesAsync() => await _http.GetFromJsonAsync<List<CategoryDto>>("api/inventory/categories") ?? new();
        public async Task<List<UnitDto>> GetUnitsAsync() => await _http.GetFromJsonAsync<List<UnitDto>>("api/inventory/units") ?? new();

        // Customers & Suppliers
        public async Task<List<CustomerDto>> GetCustomersAsync() => await _http.GetFromJsonAsync<List<CustomerDto>>("api/sales/customers") ?? new();
        public async Task<List<SupplierDto>> GetSuppliersAsync() => await _http.GetFromJsonAsync<List<SupplierDto>>("api/purchases/suppliers") ?? new();

        // Sales & Shifts
        public async Task<Guid?> CreateSaleAsync(CreateSaleCommand command)
        {
            var res = await _http.PostAsJsonAsync("api/sales", command);
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<Guid>() : null;
        }

        public async Task<Guid?> OpenShiftAsync(OpenShiftCommand command)
        {
            var res = await _http.PostAsJsonAsync("api/shifts/open", command);
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<Guid>() : null;
        }

        public async Task<bool> CloseShiftAsync(CloseShiftCommand command)
        {
            var res = await _http.PostAsJsonAsync($"api/shifts/{command.ShiftId}/close", command);
            return res.IsSuccessStatusCode;
        }

        // Dashboard
        public async Task<DashboardDataDto?> GetDashboardAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<DashboardDataDto>("api/dashboard");
            }
            catch
            {
                return null;
            }
        }
    }
}
