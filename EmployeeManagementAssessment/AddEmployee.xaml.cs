using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static EmployeeManagementAssessment.MainWindow;

namespace EmployeeManagementAssessment
{
    public partial class AddEmployee : Window
    {
        private HttpClient _client;

        public AddEmployee(HttpClient client)
        {
            this._client = client;
            InitializeComponent();
        }

        private async void SubmitNewEmployee(object sender, RoutedEventArgs e)
        {
            await PostNewEmployee();
        }

        public async Task PostNewEmployee()
        {
            var url = "https://gorest.co.in/public/v2/users";

            // Create an user object
            var user = new User
            {
                name = emplName.Text,
                email = emplEmail.Text,
                gender = emplGender.Text,
                status = emplStatus.Text
            };

            var content = JsonConvert.SerializeObject(user);

            // Create a HttpContent object
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                // Make the POST request
                var response = await _client.PostAsync(url, httpContent);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize the response content into a User object
                var data = JsonConvert.DeserializeObject<User>(responseContent);

                MessageBox.Show("Successfully added employee: " + user.name);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
    }
}