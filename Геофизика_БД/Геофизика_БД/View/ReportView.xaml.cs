using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Геофизика_БД.View
{
    public partial class ReportView : Window
    {
        public ReportView()
        {
            InitializeComponent();
            LoadObjectNames();
            reportContentTextBox.TextChanged += ReportContentTextBox_TextChanged;
        }

        private void LoadObjectNames()
        {
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";
            string query = "SELECT DISTINCT Название FROM ОбъектыИсследования;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string objectName = reader["Название"].ToString();
                        objectNameComboBox.Items.Add(objectName);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных из базы данных: " + ex.Message);
                }
            }
        }
        private void UpdateReportButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedObjectName = objectNameComboBox.SelectedItem?.ToString();
            string newReportPurpose = researchPurposeTextBox.Text;
            string newReportContent = reportContentTextBox.Text;
            string newReportResult = researchResultTextBox.Text; // Добавлен аргумент для результата исследования

            if (!string.IsNullOrEmpty(selectedObjectName))
            {
                SaveReportToDatabase(selectedObjectName, newReportPurpose, newReportContent, newReportResult);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите корректное название объекта исследования.");
            }
        }
        private void LoadDataFromDatabase(string objectName)
        {
            string savedPurpose = researchPurposeTextBox.Text;
            string savedContent = reportContentTextBox.Text;
            string savedResult = researchResultTextBox.Text;

            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";
            string query = $@"
        SELECT I.Цель, I.Результат, O.Содержание
        FROM Исследования I
        INNER JOIN Отчеты O ON I.ID_Исследования = O.ID_Исследования
        WHERE I.ID_Объекта = (SELECT ID_Объекта FROM ОбъектыИсследования WHERE Название = '{objectName}');";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        researchPurposeTextBox.Text = reader["Цель"].ToString();
                        reportContentTextBox.Text = reader["Содержание"].ToString();
                        researchResultTextBox.Text = reader["Результат"].ToString();
                    }
                    else
                    {
                        // Если данные отсутствуют, можно сбросить текстовые поля
                        researchPurposeTextBox.Text = "";
                        reportContentTextBox.Text = "";
                        researchResultTextBox.Text = "";
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных из базы данных: " + ex.Message);
                }

                // Восстанавливаем сохраненные значения, если данные не удалось загрузить
                if (string.IsNullOrEmpty(researchPurposeTextBox.Text))
                {
                    researchPurposeTextBox.Text = savedPurpose;
                    reportContentTextBox.Text = savedContent;
                    researchResultTextBox.Text = savedResult;
                }
            }
        }

        private void LoadReportButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedObjectName = objectNameComboBox.SelectedItem.ToString();

            if (!string.IsNullOrEmpty(selectedObjectName))
            {
                LoadDataFromDatabase(selectedObjectName);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите корректное название объекта исследования.");
            }
        }

        private void ReportContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string selectedObjectName = objectNameComboBox.SelectedItem?.ToString();
            string newReportPurpose = researchPurposeTextBox.Text;
            string newReportContent = reportContentTextBox.Text;
            string newReportResult = researchResultTextBox.Text; // Добавлен четвертый аргумент

            if (!string.IsNullOrEmpty(selectedObjectName))
            {
                SaveReportToDatabase(selectedObjectName, newReportPurpose, newReportContent, newReportResult); // Добавлен четвертый аргумент
            }
        }

        private void SaveReportToDatabase(string objectName, string reportPurpose, string reportContent, string reportResult)
        {
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            int objectId = GetObjectIdByName(connectionString, objectName);

            if (objectId != -1)
            {
                string updateQuery = @"
                UPDATE I
                SET I.Цель = @ReportPurpose, I.Результат = @ReportResult
                FROM Исследования I
                WHERE I.ID_Объекта = @ObjectId;

                UPDATE O
                SET O.Содержание = @ReportContent
                FROM Отчеты O
                JOIN Исследования I ON O.ID_Исследования = I.ID_Исследования
                WHERE I.ID_Объекта = @ObjectId;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(updateQuery, connection);
                    command.Parameters.AddWithValue("@ReportPurpose", reportPurpose ?? "");
                    command.Parameters.AddWithValue("@ReportContent", reportContent ?? "");
                    command.Parameters.AddWithValue("@ReportResult", reportResult ?? "");
                    command.Parameters.AddWithValue("@ObjectId", objectId);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                           
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении отчета в базе данных: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Не удалось найти ID объекта исследования для указанного названия.");
            }
        }


        private int GetObjectIdByName(string connectionString, string objectName)
        {
            int objectId = -1;

            string query = "SELECT TOP 1 ID_Объекта FROM ОбъектыИсследования WHERE Название = @ObjectName;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ObjectName", objectName);

                try
                {
                    connection.Open();
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        objectId = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении ID объекта исследования: " + ex.Message);
                }
            }

            return objectId;
        }
        private void DeleteReportFromDatabase(string objectName)
        {
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            int objectId = GetObjectIdByName(connectionString, objectName);

            if (objectId != -1)
            {
                string deleteQuery = @"
        DELETE FROM Отчеты
        WHERE ID_Исследования IN (
            SELECT ID_Исследования
            FROM Исследования
            WHERE ID_Объекта = @ObjectId
        );
        ";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(deleteQuery, connection);
                    command.Parameters.AddWithValue("@ObjectId", objectId);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Отчет успешно удален из базы данных.");
                            // Очистка текстовых полей после удаления отчета
                            researchPurposeTextBox.Text = "";
                            reportContentTextBox.Text = "";
                            researchResultTextBox.Text = "";
                        }
                        else
                        {
                            MessageBox.Show("Отчет для данного объекта исследования не найден.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении отчета из базы данных: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Не удалось найти ID объекта исследования для указанного названия.");
            }
        }
        private void DeleteReportButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedObjectName = objectNameComboBox.SelectedItem.ToString();

            if (!string.IsNullOrEmpty(selectedObjectName))
            {
                DeleteReportFromDatabase(selectedObjectName);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите корректное название объекта исследования для удаления отчета.");
            }

        }
    }
}