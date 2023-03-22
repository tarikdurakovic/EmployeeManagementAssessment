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
    public partial class EditEmployee : Window
    {
        private HttpClient _client;

        public EditEmployee(HttpClient client)
        {
            this._client = client;
            InitializeComponent();
        }

        private async void SubmitPatchedEmployee(object sender, RoutedEventArgs e)
        {
            await PutEmployee();
        }

        public async Task PutEmployee()
        {
            var userId = emplID.Text; 
            var url = "https://gorest.co.in/public/v2/users/" + userId;

            // Create a user object with the updated values
            var user = new User
            {
                name = emplName.Text,
                email = emplEmail.Text,
                gender = emplGender.Text,
                status = emplStatus.Text
            };
            var content = JsonConvert.SerializeObject(user);

            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                // Make the PUT request
                var response = await _client.PutAsync(url, httpContent);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize the response content into a User object
                var data = JsonConvert.DeserializeObject<User>(responseContent);

                // Show the response content in a message box
                MessageBox.Show("Successfully updated employee: " + user.name);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        
    }
}