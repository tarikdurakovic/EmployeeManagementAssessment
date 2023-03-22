using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace EmployeeManagementAssessment
{
    public partial class MainWindow : Window
    {
        private string _apiUrl = "https://gorest.co.in/public/v2/users";
        private int pagesCount = 0;
        private const string _apiToken = "0bf7fb56e6a27cbcadc402fc2fce8e3aa9ac2b40d4190698eb4e8df9284e2023";
        string selectedContent = null;
        private HttpClient _client;

        public MainWindow()
        {
            InitializeComponent();
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiToken);
            InitializeDataAsync();

            Closing += MainWindow_Closing;
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                var response = await _client.GetAsync(_apiUrl);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    pagesCount = int.Parse(response.Headers.GetValues("x-pagination-pages").FirstOrDefault());
                    var data = JsonConvert.DeserializeObject<List<User>>(content);
                    var userList = data.Take(10).ToList(); // get the first 10 elements from the list

                    myDataGrid.ItemsSource = userList;
                }
                else
                {
                    MessageBox.Show($"Failed to load data: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if ((window is AddEmployee) || (window is EditEmployee))
                {
                    window.Close();
                }
            }
        }

        private void addEmployee(object sender, RoutedEventArgs e)
        {
            var addEmployeeWindow = new AddEmployee(_client);
            addEmployeeWindow.Show();
        }

        private void editEmployee(object sender, RoutedEventArgs e)
        {
            var editEmployeeWindow = new EditEmployee(_client);
            editEmployeeWindow.Show();
        }

        private void ExportToCSV(object sender, RoutedEventArgs e)
        {
            // Get the selected users
            var selectedUsers = myDataGrid.SelectedItems.Cast<User>().ToList();

            // Create a CSV string for each user
            var csvStrings = selectedUsers.Select(u => $"{u.id},{u.name},{u.email},{u.gender},{u.status}");

            // Add column names to the first row of the CSV file
            var header = "Id,Name,Email,Gender,Status";
            var csv = new List<string> { header };
            csv.AddRange(csvStrings);

            // Save the CSV strings to a file
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;
                using (var writer = new StreamWriter(filePath, true))
                {
                    foreach (var line in csv)
                    {
                        writer.WriteLine(line);
                    }
                }
                MessageBox.Show($"Selected employee(s) exported to CSV successfully");
            }
        }


        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Resize and reposition elements in the window here
            myDataGrid.Margin = new Thickness(20);
        }

        private void pageTextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Verify that the pressed key isn't any non-numeric digit
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(pageTextBox.Text, out int value))
                {
                    if (value > pagesCount)
                    {
                        MessageBox.Show($"Non existing page number entered. Showing the last page, page {pagesCount}");
                        _apiUrl = $"https://gorest.co.in/public/v2/users?page={pagesCount}";
                    }
                    else 
                    { 
                        _apiUrl = $"https://gorest.co.in/public/v2/users?page={value}";
                    }
                    foreach (RadioButton rb in SearchOptionsPanel.Children)
                    {
                        rb.IsChecked = false;
                    }
                    InitializeDataAsync();
                    SearchTextBox.Text = "";
                }
                else
                {
                    MessageBox.Show("Please enter a valid page number");
                }
            }
        }

        private void SubmitPage(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";

            if (int.TryParse(pageTextBox.Text, out int value))
            {
                if (value > pagesCount)
                {
                    MessageBox.Show($"Non existing page number entered. Showing the last page, page {pagesCount}");
                    _apiUrl = $"https://gorest.co.in/public/v2/users?page={pagesCount}";
                }
                else 
                { 
                    _apiUrl = $"https://gorest.co.in/public/v2/users?page={value}";
                }
                foreach (RadioButton rb in SearchOptionsPanel.Children)
                {
                    rb.IsChecked = false;
                }

                InitializeDataAsync();
            }
            else
            {
                MessageBox.Show("Please enter a valid page number");
            }

            
        }

        private async void DeleteEmployee(object sender, RoutedEventArgs e)
        {
            if (myDataGrid.SelectedItems.Count > 0)
            {
                try
                {
                    var selectedUser = (User)myDataGrid.SelectedItems[0];
                    var response = await _client.DeleteAsync($"https://gorest.co.in/public/v2/users/{selectedUser.id}");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Employee deleted successfully");
                        InitializeDataAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Failed to delete employee: {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }

        private async void SearchEmployee(object sender, RoutedEventArgs e)
        {
            var users = await SearchEmployeeAsync();
            myDataGrid.ItemsSource = users;
            pageTextBox.Text = "";
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton selectedRadioButton = SearchOptionsPanel.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true);
            if (selectedRadioButton != null)
            {
                selectedContent = selectedRadioButton.Content.ToString();
            }
        }

        private async Task<List<User>> SearchEmployeeAsync()
        {
            var baseUrl = "https://gorest.co.in/public/v2/users";
            var url = $"{baseUrl}?{selectedContent}={SearchTextBox.Text}";

            try
            {
                // Make the GET request
                var response = await _client.GetAsync(url);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize the response content into a list of User objects
                
                var data = JsonConvert.DeserializeObject<List<User>>(responseContent);

                // Return the list of users
                return data;
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("An error occurred: " + ex.Message);
                return new List<User>();
            }
        }

        private void SearchTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchEmployee(sender, e);
                pageTextBox.Text = "";
            }
        }
    }

    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string status { get; set; }
    }

}


