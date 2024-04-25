using System;
using System.Data.SqlClient;
using System.Windows;

namespace Геофизика_БД.View
{
    public partial class EquipmentMiniView : Window
    {
        private int objectId;
        public EquipmentMiniView(int locationId, int objectId)
        {
            InitializeComponent();
             this.objectId = objectId;
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            string selectQuery = $"SELECT Наименование, Тип, Серийный_номер FROM [BD].[dbo].[Оборудование] WHERE ID_Объекта = {objectId}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            NameEquiment.Text = reader["Наименование"].ToString();
                            TypeEquiment.Text = reader["Тип"].ToString();
                            NumderEquiment.Text = reader["Серийный_номер"].ToString();
                        }
                    }
                }
            }

            SaveButton.Click += SaveButton_Click;
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string updateQuery = "UPDATE [BD].[dbo].[Оборудование] SET Наименование = @Наименование, Тип = @Тип, Серийный_номер = @Серийный_номер WHERE ID_Объекта = @ID_Объекта";
            string connectionString = "Server=HOME-PC\\SQLEXPRESS01;Database=BD;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@Наименование", NameEquiment.Text);
                    command.Parameters.AddWithValue("@Тип", TypeEquiment.Text);
                    command.Parameters.AddWithValue("@Серийный_номер", NumderEquiment.Text);
                    command.Parameters.AddWithValue("@ID_Объекта", objectId); // Assuming objectId is accessible in this context

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Данные успешно сохранены!");
                    }
                    else
                    {
                        MessageBox.Show("Failed to save data. Please try again.");
                    }
                }
            }

            this.Close(); // Close the EquipmentMiniView window
        }

    }
}