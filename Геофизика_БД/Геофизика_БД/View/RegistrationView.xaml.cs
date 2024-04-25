using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

namespace Геофизика_БД.View
{
    /// <summary>
    /// Логика взаимодействия для RegistrationView.xaml
    /// </summary>
    public partial class RegistrationView : Window
    {
        public RegistrationView()
        {
            InitializeComponent();
            PopulateRolesFromDatabase();
        }



        private void RegisterUser()
        {
            string name = NameTextBox.Text;
            string surname = SurnameTextBox.Text;
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;

            string role = null;

            // Получение роли пользователя из базы данных
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Роль FROM Пользователи WHERE Логин = @Login AND Пароль = @Password";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        role = reader["Роль"].ToString();
                    }
                    reader.Close();
                }
            }

            if (role != null)
            {
                // Показываем сообщение об успешной регистрации
                MessageBox.Show("Вход выполнен успешно! Ваша роль: " + role);

                
            }
            else
            {
                // Показываем сообщение о невозможности входа пользователя
                MessageBox.Show("Пользователь с таким логином и паролем не найден. Пожалуйста, проверьте правильность введенных данных.");
            }
        }
        private void PopulateRolesFromDatabase()
        {
            // Clear existing items in the ComboBox
            RoleComboBox.Items.Clear();

            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve roles from the database
                string query = "SELECT DISTINCT Роль FROM Пользователи"; // Adjust the table name as per your database schema
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string role = reader["Роль"].ToString();
                            RoleComboBox.Items.Add(role);
                        }
                    }
                }
            }
        }

        private void RegisterNewUser()
        {
            string name = RegNameTextBox.Text;
            string surname = RegSurnameTextBox.Text;
            string login = RegLoginTextBox.Text;
            string password = RegPasswordTextBox.Text;
            string selectedRole = RoleComboBox.SelectedItem?.ToString();

            // Check if any field is empty
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(selectedRole))
            {
                MessageBox.Show("Fill out all fields for registration.");
                return;
            }

            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if the user already exists
                string checkUserQuery = "SELECT COUNT(*) FROM Пользователи WHERE Логин = @Login";
                using (SqlCommand checkUserCommand = new SqlCommand(checkUserQuery, connection))
                {
                    checkUserCommand.Parameters.AddWithValue("@Login", login);
                    int count = (int)checkUserCommand.ExecuteScalar();
                    if (count > 0)
                    {
                       
                        return;
                    }
                }

                // Insert new user into the database
                string insertQuery = "INSERT INTO Пользователи (Имя, Фамилия, Логин, Пароль, Роль) VALUES (@Name, @Surname, @Login, @Password, @Role)";
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Surname", surname);
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@Role", selectedRole);
                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Пользователь успешно зарегистрирован!");

            // Clear the registration fields after successful registration
            RegNameTextBox.Text = "";
            RegSurnameTextBox.Text = "";
            RegLoginTextBox.Text = "";
            RegPasswordTextBox.Text = "";
            RoleComboBox.SelectedIndex = -1; // Reset ComboBox selection
        }

        

        // Call this method when the RegistrationView is initialized to populate roles
       
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterUser();
            MenuView1 menu = new MenuView1();
            menu.Show();

            // Close the Registration window
            this.Close();
        }

        private void RegisterButton1_Click(object sender, RoutedEventArgs e)
        {
            RegisterNewUser();
            MenuView1 menu = new MenuView1();
            menu.Show();

            // Close the Registration window
            this.Close();
        }

        
    }
}

