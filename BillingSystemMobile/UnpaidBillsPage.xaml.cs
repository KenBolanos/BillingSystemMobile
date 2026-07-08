using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
using System.Net.Http.Json;

namespace BillingSystemMobile
{
    public partial class UnpaidBillsPage : ContentPage
    {
        #if ANDROID
            private const string ApiBase = "http://10.0.2.2:6969";
        #else
            private const string ApiBase = "http://localhost:6969";
        #endif

        private readonly HttpClient _http = new HttpClient();

        public UnpaidBillsPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadUnpaidBillsAsync();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlertAsync("Log Out",
                "Are you sure you want to log out?",
                "Yes", "Cancel");

            if (!confirm)
                return;

            await Shell.Current.GoToAsync("//LoginPage");
        }

        private async Task LoadUnpaidBillsAsync()
        {
            int customerId = Preferences.Get("CustomerID", 0);
            string fullName = Preferences.Get("FullName", string.Empty);

            lblWelcome.Text = $"Welcome, {fullName}";
            stackBills.Children.Clear();
            stackBills.IsVisible = true;
            stackEmpty.Children.Clear();
            stackEmpty.IsVisible = false;

            try
            {
                var bills = await _http.GetFromJsonAsync<List<UnpaidBill>>(
                    $"{ApiBase}/api/billing/unpaid/{customerId}") ?? new();

                if (bills.Count == 0)
                {
                    stackBills.IsVisible = false;
                    stackEmpty.Children.Add(BuildEmptyState());
                    stackEmpty.IsVisible = true;
                    return;
                }

                foreach (var bill in bills)
                {
                    stackBills.Children.Add(BuildBillCard(bill));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Connection Error",
                    $"Could not load unpaid bills:\n{ex.Message}",
                    "OK");
            }
        }

        // Friendly "all caught up" card shown when there are no unpaid bills.
        private Border BuildEmptyState()
        {
            var lblIcon = new Label
            {
                Text = "✔️",
                FontSize = 42,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 10, 0, 6)
            };

            var lblTitle = new Label
            {
                Text = "You're all caught up!",
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1F4E79"),
                HorizontalOptions = LayoutOptions.Center
            };

            var lblSubtitle = new Label
            {
                Text = "You have no unpaid bills at the moment.",
                FontSize = 13,
                TextColor = Color.FromArgb("#777777"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(20, 4, 20, 10)
            };

            var content = new VerticalStackLayout
            {
                Spacing = 2,
                HorizontalOptions = LayoutOptions.Center,
                Children = { lblIcon, lblTitle, lblSubtitle }
            };

            return new Border
            {
                Padding = new Thickness(20, 24),
                Stroke = Color.FromArgb("#D9EAD3"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                BackgroundColor = Color.FromArgb("#F3FAF1"),
                HorizontalOptions = LayoutOptions.Fill,
                Content = content,
                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Opacity = 0.05f,
                    Radius = 8,
                    Offset = new Point(0, 2)
                }
            };
        }

        private Border BuildBillCard(UnpaidBill bill)
        {
            var lblMonth = new Label
            {
                Text = bill.BillingMonth,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                TextColor = Color.FromArgb("#1F4E79")
            };

            var lblStatus = new Label
            {
                Text = bill.Status,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#C0392B"),
                BackgroundColor = Color.FromArgb("#FCE4E4"),
                Padding = new Thickness(8, 3),
                HorizontalOptions = LayoutOptions.End
            };

            var headerRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            headerRow.Add(lblMonth, 0, 0);
            headerRow.Add(lblStatus, 1, 0);

            var lblConsumption = new Label
            {
                Text = $"Consumption: {bill.Consumption} cu.m.",
                FontSize = 13,
                TextColor = Color.FromArgb("#555555")
            };

            var lblAmount = new Label
            {
                Text = $"₱{bill.TotalAmount:N2}",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#2E75B6")
            };

            var lblDate = new Label
            {
                Text = bill.BillingDate,
                FontSize = 12,
                TextColor = Color.FromArgb("#999999"),
                HorizontalOptions = LayoutOptions.End
            };

            var footerRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            footerRow.Add(lblAmount, 0, 0);
            footerRow.Add(lblDate, 1, 0);

            var content = new VerticalStackLayout
            {
                Spacing = 6,
                Children = { headerRow, lblConsumption, footerRow }
            };

            return new Border
            {
                Padding = 15,
                Stroke = Color.FromArgb("#DDDDDD"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                BackgroundColor = Colors.White,
                Content = content,
                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Opacity = 0.08f,
                    Radius = 8,
                    Offset = new Point(0, 2)
                }
            };
        }
    }

    public class UnpaidBill
    {
        public int BillingID { get; set; }
        public string BillingMonth { get; set; } = string.Empty;
        public int Consumption { get; set; }
        public decimal RatePerCubic { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BillingDate { get; set; } = string.Empty;
    }
}