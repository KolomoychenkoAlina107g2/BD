using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Геофизика_БД.View
{
    public partial class MeasurementsView : Window
    {
        private bool isGeoElectricEditWindowOpened = false;
        private GeoElectricEditWindow geoElectricEditWindow; // Поле для хранения экземпляра окна GeoElectricEditWindow
        public MeasurementsView()
        {
            InitializeComponent();
            Window_Loaded();
            

            locationsComboBox.SelectionChanged += LocationsComboBox_SelectionChanged;
            objectsComboBox.SelectionChanged += ObjectsComboBox_SelectionChanged;
            OpenGeoElectricEditWindowButton.Click += OpenGeoElectricEditWindow;
        }

        private void LocationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (locationsComboBox.SelectedItem != null)
            {
                dynamic selectedLocation = locationsComboBox.SelectedItem;
                int locationId = selectedLocation.ID;

                LoadObjectsForSelectedLocation(locationId); // Load objects for the selected location
            }
        }

        private void LoadObjectsForSelectedLocation(int locationId)
        {
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";
            string objectsQuery = $"SELECT ID_Объекта, Название FROM ОбъектыИсследования WHERE ID_Места = {locationId}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(objectsQuery, connection);

                try
                {
                    connection.Open();
                    SqlDataReader objectsReader = command.ExecuteReader();

                    // Clear the items before adding new ones
                    objectsComboBox.Items.Clear();

                    while (objectsReader.Read())
                    {
                        int objectId = Convert.ToInt32(objectsReader["ID_Объекта"]);
                        string objectName = objectsReader["Название"].ToString();
                        objectsComboBox.Items.Add(new { ID = objectId, Name = objectName });
                    }

                    objectsReader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading objects: " + ex.Message);
                }
            }
        }

        private void Window_Loaded()
        {
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT ID_Места, Название_местности FROM Место_исследования";

                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        locationsComboBox.Items.Add(new { ID = reader["ID_Места"], Название = reader["Название_местности"].ToString() });
                    }
                }
            }
        }

        private void ObjectsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (objectsComboBox.SelectedItem != null)
            {
                dynamic selectedObject = objectsComboBox.SelectedItem;
                int objectId = selectedObject.ID;
                string objectName = selectedObject.Name;

                string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";
                string query = $"SELECT TOP (1) Напряжение, Дата, Время FROM [BD].[dbo].[Измерения] WHERE ID_Объекта = {objectId}";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            voltageTextBox.Text = reader["Напряжение"].ToString();

                            // Convert date to DateTime and display date only
                            DateTime dateValue = Convert.ToDateTime(reader["Дата"]);
                            dateTextBox.Text = dateValue.ToShortDateString();

                            timeTextBox.Text = reader["Время"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("No measurement data found for the selected object.");
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading measurement data: " + ex.Message);
                    }
                }

                
            }
        }

        private void AddMeasurementButton_Click(object sender, RoutedEventArgs e)
        {
           

            int objectId = ((dynamic)objectsComboBox.SelectedItem).ID;
            double voltage = Convert.ToDouble(voltageTextBox.Text);
            DateTime dateTime = DateTime.Parse($"{dateTextBox.Text} {timeTextBox.Text}");

            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";
            string insertQuery = $"INSERT INTO [BD].[dbo].[Измерения] (ID_Объекта, Напряжение, Дата, Время) " +
                                 $"VALUES ({objectId}, {voltage}, '{dateTime.Date}', '{dateTime.TimeOfDay}')";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(insertQuery, connection);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Новые данные измерений успешно добавлены!");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить новые данные измерений.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding measurement data: " + ex.Message);
                }
            }
        }

        // Добавление нового объекта в таблицу разрезы
        private void AddObjectToRazrezTable(int objectId, string depth, string resistance, string coordinates)
        {
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";
            string insertQuery = $"INSERT INTO [BD].[dbo].[ГеоэлектрическиеРазрезы] (ID_Объекта, Глубина, ЭлектрическоеСопротивление, Координата_X, Координата_Y) " +
                                 $"VALUES ({objectId}, '{depth}', '{resistance}', '{coordinates}');";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(insertQuery, connection);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Новый объект успешно добавлен в таблицу разрезы!");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить новый объект в таблицу разрезы.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении нового объекта в таблицу разрезы: " + ex.Message);
                }
            }
        }

        // Логика добавления нового объекта в таблицу разрезы
        private void AddNewObjectToRazrezTable(int objectId, string depth, string resistance, string coordinates)
        {
            // Проверка наличия обязательных данных перед добавлением
            if (objectId > 0 && !string.IsNullOrWhiteSpace(depth) && !string.IsNullOrWhiteSpace(resistance) && !string.IsNullOrWhiteSpace(coordinates))
            {
                AddObjectToRazrezTable(objectId, depth, resistance, coordinates);
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните все необходимые данные перед добавлением в таблицу разрезы.");
            }
        }
        private void SaveMeasurementButton_Click(object sender, RoutedEventArgs e)
        {
            if (objectsComboBox.SelectedItem != null)
            {
                dynamic selectedObject = objectsComboBox.SelectedItem;
                int objectId = selectedObject.ID;
                string updatedVoltage = voltageTextBox.Text;
                string updatedDate = dateTextBox.Text;
                string updatedTimeOfDay = timeTextBox.Text;

                if (decimal.TryParse(updatedVoltage, out decimal parsedVoltage))
                {
                    string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";
                    string updateQuery = "UPDATE Измерения SET Напряжение = @Voltage, Дата = @Date, Время = @TimeOfDay WHERE ID_Объекта = @ObjectId";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                        updateCommand.Parameters.AddWithValue("@Voltage", parsedVoltage);
                        updateCommand.Parameters.AddWithValue("@Date", updatedDate);
                        updateCommand.Parameters.AddWithValue("@TimeOfDay", updatedTimeOfDay);
                        updateCommand.Parameters.AddWithValue("@ObjectId", objectId);

                        try
                        {
                            connection.Open();
                            int rowsAffected = updateCommand.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Информация об объекте успешно обновлена!");
                            }
                            else
                            {
                                MessageBox.Show("Не удалось обновить информацию об объекте.");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при обновлении данных: " + ex.Message);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Некорректное значение напряжения.");
                }
            }
            else
            {
                MessageBox.Show("Выберите объект для сохранения изменений.");
            }
        }



        private void OpenGeoElectricEditWindow(object sender, RoutedEventArgs e)
        {
            if (!isGeoElectricEditWindowOpened)
            {
                dynamic selectedObject = objectsComboBox.SelectedItem as dynamic;

                if (selectedObject != null)
                {
                    int objectId = selectedObject.ID;

                    string depth = "";
                    string resistance = "";
                    string coordinates = "";
                    string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";
                    string query = $"SELECT Глубина, ЭлектрическоеСопротивление, Координата_X, Координата_Y " +
                                   $"FROM [BD].[dbo].[ГеоэлектрическиеРазрезы] " +
                                   $"WHERE ID_Объекта = {objectId}";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand(query, connection);
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            depth = reader["Глубина"].ToString();
                            resistance = reader["ЭлектрическоеСопротивление"].ToString();
                            coordinates = $"X: {reader["Координата_X"]}, Y: {reader["Координата_Y"]}";
                        }
                        reader.Close();

                        if (geoElectricEditWindow == null || !geoElectricEditWindow.IsVisible)
                        {
                            geoElectricEditWindow = new GeoElectricEditWindow(objectId, depth, resistance, coordinates);
                            geoElectricEditWindow.Closed += (s, args) => isGeoElectricEditWindowOpened = false;
                            isGeoElectricEditWindowOpened = true;
                            geoElectricEditWindow.Show();
                        }
                    }
                }
            }
            else
            {
               
            }
        }
        private void EquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (locationsComboBox.SelectedItem != null && objectsComboBox.SelectedItem != null)
            {
                dynamic selectedLocation = locationsComboBox.SelectedItem;
                int locationId = selectedLocation.ID;

                dynamic selectedObject = objectsComboBox.SelectedItem;
                int objectId = selectedObject.ID;

                EquipmentMiniView equipmentMiniView = new EquipmentMiniView(locationId, objectId);
                equipmentMiniView.Show();
            }
            else
            {
                MessageBox.Show("Please select both a location and an object before opening the Equipment view.");
            }
        }
    }
}