using System.Net.Http.Json;
using Microsoft.Maui.Controls;


namespace BillingSystemMobile
{
    // Matches the JSON returned by POST /api/auth/login on success
    public class LoginResponse
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int CustomerID { get; set; }
        public string Message { get; set; } = string.Empty;
    }


    public partial class LoginPage : ContentPage
    {
        // Must match the base URL used in MainPage.xaml.cs
        private const string ApiBase = "http://10.0.2.2:6969";


        private readonly HttpClient _http = new HttpClient();


        public LoginPage()
        {
            InitializeComponent();
        }


        // Pressing Return/Go on the password keyboard triggers login
        private void OnPasswordCompleted(object sender, EventArgs e)
        {
            OnLoginClicked(sender, e);
        }


        // Log In button clicked
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Clear any previous error message
            lblError.IsVisible = false;
            lblError.Text = string.Empty;


            // Step 1: Validate that both fields are filled
            if (string.IsNullOrWhiteSpace(entryUsername.Text))
            {
                ShowError("Please enter your username.");
                return;
            }


            if (string.IsNullOrWhiteSpace(entryPassword.Text))
            {
                ShowError("Please enter your password.");
                return;
            }


            // Step 2: Disable the button to prevent double-tap
            btnLogin.IsEnabled = false;
            btnLogin.Text = "Logging in...";


            try
            {
                // Step 3: Send credentials to the API
                var loginData = new
                {
                    username = entryUsername.Text.Trim(),
                    password = entryPassword.Text
                };


                var response = await _http.PostAsJsonAsync(
                    $"{ApiBase}/api/auth/login", loginData);


                if (response.IsSuccessStatusCode)
                {
                    // Step 4: Login succeeded — deserialize to get Role and CustomerID
                    var loginResult = await response.Content
                        .ReadFromJsonAsync<LoginResponse>();

                    if (loginResult == null)
                    {
                        ShowError("Invalid response from server.");
                        return;
                    }

                    // Step 5: Route based on Role.
                    // Double-slash '//' replaces the entire navigation stack
                    // so the user cannot press Back to return to Login.
                    if (loginResult.Role == "Biller")
                    {
                        await Shell.Current.GoToAsync("//MainPage");
                    }
                    else if (loginResult.Role == "Customer")
                    {
                        Preferences.Set("CustomerID", loginResult.CustomerID);
                        Preferences.Set("FullName", loginResult.FullName);

                        await Shell.Current.GoToAsync("//UnpaidBillsPage");
                    }
                    else
                    {
                        ShowError("Account role is not authorized for mobile access.");
                    }
                }
                else if (response.StatusCode ==
                         System.Net.HttpStatusCode.Unauthorized)
                {
                    // Wrong credentials or not a Biller/Customer
                    ShowError(
                        "Invalid username or password, or account is not authorized for mobile access.");
                }
                else
                {
                    ShowError($"Unexpected error: {(int)response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Connection error:\n{ex.Message}");
            }
            finally
            {
                // Always re-enable the button
                btnLogin.IsEnabled = true;
                btnLogin.Text = "Log In";
            }
        }


        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.IsVisible = true;
        }
    }
}