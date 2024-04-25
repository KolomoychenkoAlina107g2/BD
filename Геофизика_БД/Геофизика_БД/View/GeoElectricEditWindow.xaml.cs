using System;
using System.Data.SqlClient;
using System.Windows;

namespace Геофизика_БД.View
{
    public partial class GeoElectricEditWindow : Window
    {

        private int objectId;

        public GeoElectricEditWindow(int objectId, string depth, string resistance, string coordinates)
        {
            InitializeComponent();
            this.objectId = objectId; // Сохраняем ID разреза
            DepthTextBox.Text = depth;
            ResistanceTextBox.Text = resistance;
            CoordinatesTextBox.Text = coordinates;
            SaveButton.Click += SaveButton_Click;
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            string selectQuery = $"SELECT TOP 1 Глубина, ЭлектрическоеСопротивление, Координата_X, Координата_Y " +
                                 $"FROM [BD].[dbo].[ГеоэлектрическиеРазрезы] " +
                                 $"WHERE ID_Объекта = {objectId}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DepthTextBox.Text = reader["Глубина"].ToString();
                            ResistanceTextBox.Text = reader["ЭлектрическоеСопротивление"].ToString();
                            CoordinatesTextBox.Text = $"X: {reader["Координата_X"]}, Y: {reader["Координата_Y"]}";
                        }
                    }
                }
            }

            
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string updateQuery = "UPDATE [BD].[dbo].[ГеоэлектрическиеРазрезы] SET Глубина = @Depth, ЭлектрическоеСопротивление = @Resistance WHERE ID_Объекта = @ObjectId";
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@Depth", DepthTextBox.Text);
                    command.Parameters.AddWithValue("@Resistance", float.Parse(ResistanceTextBox.Text));
                    command.Parameters.AddWithValue("@ObjectId", objectId); // Правильное использование ID объекта

                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Данные успешно сохранены!");
            this.Close(); // Закрыть окно GeoElectricEditWindow
        }
    }
}