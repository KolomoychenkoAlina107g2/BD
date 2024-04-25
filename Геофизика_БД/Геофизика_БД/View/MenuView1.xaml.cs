using System.Windows;

namespace Геофизика_БД.View
{
    /// <summary>
    /// Логика взаимодействия для MenuView1.xaml
    /// </summary>
    public partial class MenuView1 : Window
    {
        public MenuView1()
        {
            InitializeComponent();
        }

        private void ElementButton_Click(object sender, RoutedEventArgs e)
        {
            Research_object research_object = new Research_object();
            research_object.Show();
        }

        private void ElementButton1_Click(object sender, RoutedEventArgs e)
        {
            // Создание экземпляра MeasurementsView
            MeasurementsView measurementsView = new MeasurementsView();

            // Отображение вкладки MeasurementsView
            measurementsView.Show();
        }

        private void ElementButton2_Click(object sender, RoutedEventArgs e)
        {
            ReportView reportView = new ReportView();
            reportView.Show();
        }
    }
}
