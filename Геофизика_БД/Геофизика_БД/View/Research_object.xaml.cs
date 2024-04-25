using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;


namespace Геофизика_БД.View
{
    /// <summary>
    /// Логика взаимодействия для Research_object.xaml
    /// </summary>
    public partial class Research_object : Window
    {

        public Research_object()
        {
            InitializeComponent();
            Window_Loaded();
            objectsComboBox.SelectionChanged += objectsComboBox_SelectionChanged;
            placesComboBox.SelectionChanged += placesComboBox_SelectionChanged;
            LoadStructureData();
        }



        private void Window_Loaded()
        {
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT DISTINCT Место_исследования.Название_местности FROM ОбъектыИсследования INNER JOIN Место_исследования ON ОбъектыИсследования.ID_Места = Место_исследования.ID_Места";

                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        placesComboBox.Items.Add(reader["Название_местности"].ToString());
                    }
                }
            }
        }

        private void placesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedLocation = placesComboBox.SelectedItem as string; // Преобразуйте в строку

            if (!string.IsNullOrEmpty(selectedLocation))
            {
                string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT Название FROM ОбъектыИсследования WHERE ID_Места IN (SELECT ID_Места FROM Место_исследования WHERE Название_местности = @SelectedLocation)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SelectedLocation", selectedLocation);
                        SqlDataReader reader = command.ExecuteReader();

                        objectsComboBox.Items.Clear(); // Очистить предыдущие значения

                        while (reader.Read())
                        {
                            objectsComboBox.Items.Add(reader["Название"].ToString());
                        }
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = objectsComboBox.Text;

            if (string.IsNullOrEmpty(selectedObject))
            {
                // Если выбранного объекта нет, то пользователь хочет создать новую запись
                string newType = typeTextBox.Text;
                string newArea = areaTextBox.Text;
                string newStructure = structureTextBox.Text;
                string newCoordinatesObj = coordinatesObjTextBox.Text;
                string newCoordinatesLoc = coordinatesLocTextBox.Text;

                if ( !string.IsNullOrEmpty(newType) && !string.IsNullOrEmpty(newArea) && !string.IsNullOrEmpty(newStructure) && !string.IsNullOrEmpty(newCoordinatesObj) && !string.IsNullOrEmpty(newCoordinatesLoc))
                {
                    // Вызвать метод добавления нового объекта
                    AddNewObject(newType, newArea, newStructure, newCoordinatesObj, newCoordinatesLoc);
                }
                else
                {
                    MessageBox.Show("Пожалуйста, заполните все поля для добавления нового объекта.");
                }
            }
            else
            {
                // Если выбран существующий объект, то сохраняем обновления
                string updatedType = typeTextBox.Text;
                string updatedArea = areaTextBox.Text;
                string updatedStructure = structureTextBox.Text;
                string updatedCoordinatesObj = coordinatesObjTextBox.Text;
                string updatedCoordinatesLoc = coordinatesLocTextBox.Text;


                if (decimal.TryParse(updatedArea, out decimal areaValue))
                {
                    // Обновление информации об объекте
                    UpdateSelectedObject(selectedObject, updatedType, areaValue, updatedStructure, updatedCoordinatesObj, updatedCoordinatesLoc);
                }
                else
                {
                    MessageBox.Show("Площадь должна быть числом.");
                }
            }
        }

        private void AddNewObject( string objectType, string area, string structure, string coordinatesObj, string coordinatesLoc)
        {
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertLocationQuery = "INSERT INTO Место_исследования (Название_местности, Координаты) VALUES (@LocationName, @LocationCoordinates)";
                string insertObjectQuery = "INSERT INTO ОбъектыИсследования (Название, ТипОбъекта, Площадь, ID_Места, Координаты) VALUES (@ObjectName, @ObjectType, @Area, @LocationId, @ObjectCoordinates)";

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Добавление нового местоположения
                        using (SqlCommand locationCommand = new SqlCommand(insertLocationQuery, connection, transaction))
                        {
                            locationCommand.Parameters.AddWithValue("@LocationName", coordinatesLoc);
                            locationCommand.Parameters.AddWithValue("@LocationCoordinates", coordinatesLoc);
                            locationCommand.ExecuteNonQuery();
                        }

                        // Получение ID добавленного местоположения
                        string getLocationIdQuery = "SELECT MAX(ID_Места) FROM Место_исследования";
                        int locationId;
                        using (SqlCommand command = new SqlCommand(getLocationIdQuery, connection, transaction))
                        {
                            locationId = (int)command.ExecuteScalar();
                        }

                        // Добавление нового объекта исследования
                        using (SqlCommand objectCommand = new SqlCommand(insertObjectQuery, connection, transaction))
                        {

                            objectCommand.Parameters.AddWithValue("@ObjectType", objectType);
                            objectCommand.Parameters.AddWithValue("@Area", area);
                            objectCommand.Parameters.AddWithValue("@ObjectCoordinates", coordinatesObj);
                            objectCommand.Parameters.AddWithValue("@LocationId", locationId);
                            objectCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Новый объект и местоположение успешно добавлены в базу данных.");
                        LoadStructureData();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Произошла ошибка при добавлении нового объекта и местоположения: " + ex.Message);
                    }
                }
            }
        }
        private void UpdateSelectedObject(string selectedObject, string updatedType, decimal updatedArea, string updatedCoordinates, string updatedCoordinatesLoc, string updatedCoordinatesLoc1)
        {
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string updateObjectQuery = "UPDATE ОбъектыИсследования SET ТипОбъекта = @Type, Площадь = @Area, Координаты = @Coordinates WHERE Название = @SelectedObject";

                using (SqlCommand command = new SqlCommand(updateObjectQuery, connection))
                {
                    command.Parameters.AddWithValue("@Type", updatedType);
                    command.Parameters.AddWithValue("@Area", updatedArea);
                    command.Parameters.AddWithValue("@Coordinates", updatedCoordinates);
                    command.Parameters.AddWithValue("@SelectedObject", selectedObject);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Информация об объекте успешно обновлена!");
                        LoadStructureData();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении информации об объекте.");
                    }
                }
            }
        }



        private void objectsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedObject = objectsComboBox.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedObject))
            {
                string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT oi.ТипОбъекта, oi.Площадь, oi.Координаты AS Объект_Координаты, mi.Координаты AS Место_Координаты, gs.Описание AS Структура " +
                                   "FROM ОбъектыИсследования AS oi " +
                                   "JOIN Место_исследования AS mi ON oi.ID_Места = mi.ID_Места " +
                                   "JOIN ГеологическиеСтруктуры AS gs ON oi.ID_Объекта = gs.ID_Исследования " +
                                   "WHERE oi.Название = @SelectedObject";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SelectedObject", selectedObject);
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            typeTextBox.Text = reader["ТипОбъекта"].ToString();
                            areaTextBox.Text = reader["Площадь"].ToString();
                            coordinatesObjTextBox.Text = reader["Объект_Координаты"].ToString(); // Координаты объекта
                            coordinatesLocTextBox.Text = reader["Место_Координаты"].ToString(); // Координаты места
                            structureTextBox.Text = reader["Структура"].ToString(); // Данные о структуре
                        }
                    }
                }
            }
        }

        private void AddLocationAndObject(string newLocationName, string newObjectName)
        {
            using (SqlConnection connection = new SqlConnection("Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;"))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Добавление нового места
                    string insertLocationQuery = "INSERT INTO Место_исследования (Название_местности) VALUES (@NewLocationName)";
                    using (SqlCommand command = new SqlCommand(insertLocationQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@NewLocationName", newLocationName);
                        command.ExecuteNonQuery();
                    }

                    // Добавление нового объекта и его привязка к добавленному месту
                    string insertObjectQuery = "INSERT INTO ОбъектыИсследования (Название, ID_Места) VALUES (@NewObjectName, (SELECT ID_Места FROM Место_исследования WHERE Название_местности = @NewLocationName))";
                    using (SqlCommand command = new SqlCommand(insertObjectQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@NewObjectName", newObjectName);
                        command.Parameters.AddWithValue("@NewLocationName", newLocationName);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Новое место и объект успешно добавлены в базу данных.");

                    // Загрузка данных о новом объекте после успешного добавления
                    LoadStructureData();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Произошла ошибка при добавлении места и объекта: " + ex.Message);
                }
            }
        }

        private void AddLocationAndObjectButton_Click(object sender, RoutedEventArgs e)
        {
            string newLocationName = newLocationTextBox.Text;
            string newObjectName = newObjectTextBox.Text;

            if (!string.IsNullOrEmpty(newLocationName) && !string.IsNullOrEmpty(newObjectName))
            {
                AddLocationAndObject(newLocationName, newObjectName);
                placesComboBox.Items.Clear();
                objectsComboBox.Items.Clear();
                Window_Loaded();
                // Обновить Combobox'ы с данными, если необходимо
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите название нового места и объекта.");
            }
        }
        private void LoadStructureData()
        {
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Получение информации о последнем добавленном объекте
                string query = "SELECT TOP 1 oi.Название, oi.ТипОбъекта, oi.Площадь, oi.Координаты AS Объект_Координаты, mi.Координаты AS Место_Координаты, gs.Описание AS Структура " +
                               "FROM ОбъектыИсследования AS oi " +
                               "JOIN Место_исследования AS mi ON oi.ID_Места = mi.ID_Места " +
                               "LEFT JOIN ГеологическиеСтруктуры AS gs ON oi.ID_Объекта = gs.ID_Исследования " +
                               "ORDER BY oi.ID_Объекта DESC"; // Сортировка по убыванию ID для получения последнего добавленного объекта

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        typeTextBox.Text = reader["ТипОбъекта"].ToString();
                        areaTextBox.Text = reader["Площадь"].ToString();
                        coordinatesObjTextBox.Text = reader["Объект_Координаты"].ToString(); // Координаты объекта
                        coordinatesLocTextBox.Text = reader["Место_Координаты"].ToString(); // Координаты места
                        structureTextBox.Text = reader["Структура"].ToString(); // Данные о структуре
                    }
                }
            }
        }


    }

    
}
