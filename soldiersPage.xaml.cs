using System;
using System.Collections.Generic;
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

namespace WPF_Военный_округ_Батталов
{
    /// <summary>
    /// Логика взаимодействия для soldiersPage.xaml
    /// </summary>
    public partial class soldiersPage : Page
    {
        static private List<Personnel> data = new List<Personnel>();
        public soldiersPage()
        {
            InitializeComponent();
            bool isloaded = true;
            MainListView.ItemsSource = MilitaryDistrictEntities.GetContext().Personnel.ToList();
            PageCountAllBlock.Text = MilitaryDistrictEntities.GetContext().Personnel.Count().ToString();
            PageCountCurrent.Text = MilitaryDistrictEntities.GetContext().Personnel.Count().ToString();

        }

        void update()
        {
            if (IsLoaded)
            {
                data = MilitaryDistrictEntities.GetContext().Personnel.ToList();

                switch (ComboBoxSort.SelectedIndex)
                {
                    case 0:

                        break;

                    case 1:
                        data = data.OrderBy(p => p.Last_name).ToList();
                        break;

                    case 2:
                        data = data.OrderByDescending(p => p.Last_name).ToList();
                        break;

                    case 3:
                        data = data.OrderBy(p => p.Service_experience_years).ToList();
                        break;

                    case 4:
                        data = data.OrderByDescending(p => p.Service_experience_years).ToList();
                        break;
                }

                switch (ComboBoxFiltr.SelectedIndex)
                {
                    case 0:

                        break;

                    case 1:
                        data = data.Where(Ismag => Ismag.Service_experience_years > 3).ToList();
                        break;

                    case 2:
                        data = data.Where(Ismag => Ismag.Service_experience_years > 5).ToList();
                        break;

                    case 3:
                        data = data.Where(Ismag => Ismag.Service_experience_years > 7).ToList();
                        break;

                    case 4:
                        data = data.Where(Ismag => Ismag.Service_experience_years > 9).ToList();
                        break;

                    case 5:
                        data = data.Where(Ismag => Ismag.Service_experience_years > 11).ToList();
                        break;
                }


                data = data.Where(p => p.Last_name.ToLower().Contains(TextBoxSerch.Text.ToLower().ToString()) || p.First_name.ToLower().Contains(TextBoxSerch.Text.ToLower().ToString()) || (p.Last_name + " " + p.First_name).ToLower().Contains(TextBoxSerch.Text.ToLower().ToString())).ToList();

                MainListView.ItemsSource = data;

                PageCountCurrent.Text = data.Count.ToString();
            }

        }
        private void ComboBoxSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            update();
        }

        private void ComboBoxFiltr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            update();
        }

        private void TextBoxSerch_TextChanged(object sender, TextChangedEventArgs e)
        {
            update();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage((sender as Button).DataContext as Personnel));
            update();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var currentSoldier = (sender as Button).DataContext as Personnel;
            var currentSoldiersEvent = MilitaryDistrictEntities.GetContext().Personnel_Event
                .Where(p => p.Id_personnel == currentSoldier.Id_personnel)
                .ToList();

            if (currentSoldiersEvent.Count != 0)
            {
                MessageBox.Show("Невозможно выполнить удаление, так как солдат записан на мероприятие");
            }
            else
            {
                if (MessageBox.Show("Вы точно хотите выполнить удаление?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        MilitaryDistrictEntities.GetContext().Personnel.Remove(currentSoldier);
                        MilitaryDistrictEntities.GetContext().SaveChanges();
                        update();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                MilitaryDistrictEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                PageCountAllBlock.Text = MilitaryDistrictEntities.GetContext().Personnel.Count().ToString();
                update();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }
    }
}
