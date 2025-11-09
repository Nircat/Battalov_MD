using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace WPF_Военный_округ_Батталов
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private Personnel currentSoldier = new Personnel();
        public List<Platoon> Platoons { get; }
        public List<Branch> Branches { get; }

        List<string> positions = new List<string>() { "Ефрейтор", "Младший сержант", "Младший лейтенант", "Старшина", "Майор","Капитан", "Лейтенант", "Сержант"};

        private const string DefaultImagePath = "images/icon_MD.png";
        public AddEditPage(Personnel currentsoldier)
        {

            var ctx = MilitaryDistrictEntities.GetContext();

            Platoons = ctx.Platoon.ToList();
            Branches = ctx.Branch.ToList();


            InitializeComponent();

            currentSoldier = currentsoldier ?? new Personnel();

            if (string.IsNullOrWhiteSpace(currentSoldier.Images_personnel))
                currentSoldier.Images_personnel = DefaultImagePath;

            if (currentSoldier.Id_platoon == 0 && Platoons.Any())
                currentSoldier.Id_platoon = Platoons.First().Id_platoon;


            DataContext = currentSoldier;

            PositionComboBox.ItemsSource = positions;
            if (!string.IsNullOrWhiteSpace(currentSoldier.Position) && positions.Contains(currentSoldier.Position))
            {
                PositionComboBox.SelectedItem = currentSoldier.Position;
            }

            BranchComboBox.ItemsSource = Branches;

            PlatoonComboBox.ItemsSource = Platoons;

            var currentPlatoon = Platoons.FirstOrDefault(p => p.Id_platoon == currentSoldier.Id_platoon);
            if (currentPlatoon != null)
            {
                BranchComboBox.SelectedItem = currentPlatoon.Unit?.Branch;
                PlatoonComboBox.SelectedItem = currentPlatoon;
                UpdateLocationText(currentPlatoon);
            }


        }

        private void ChangePicture_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Выберите фото",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp",
                InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images")
            };

            if (dlg.ShowDialog() == true)
            {
                // Скопируем файл в локальную папку приложения
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var targetDir = Path.Combine(baseDir, "images", "users");
                Directory.CreateDirectory(targetDir);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dlg.FileName)}";
                var destPath = Path.Combine(targetDir, fileName);

                File.Copy(dlg.FileName, destPath, overwrite: true);

                // Сохраняем путь в модель
                currentSoldier.Images_personnel = destPath;

                // Так как EF POCO обычно не реализует INotifyPropertyChanged,
                // обновим превью "в лоб":
                PhotoImage.Source = new BitmapImage(new Uri(destPath));
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            var ctx = MilitaryDistrictEntities.GetContext();

            if (currentSoldier != null)
            {
                if (Surname.Text.Trim().Length > 0 && Surname.Text.Trim().All(char.IsLetter))
                {
                    currentSoldier.Last_name = Surname.Text.Trim();
                }
                else
                {
                    errors.AppendLine("Фамилия введена не корректно!");
                }

                if (Name.Text.Trim().Length > 0 && Name.Text.Trim().All(char.IsLetter))
                {
                    currentSoldier.First_name = Name.Text.Trim();
                }
                else
                {
                    errors.AppendLine("Имя введена не корректно!");
                }

                if (!string.IsNullOrWhiteSpace(PatronymicBox.Text))
                {
                    if (PatronymicBox.Text.Trim().All(char.IsLetter))
                        currentSoldier.Surname = PatronymicBox.Text.Trim();
                    else
                        errors.AppendLine("Отчество введено некорректно.");
                }
                else
                {
                    currentSoldier.Surname = null;
                }



                if (PositionComboBox.SelectedItem is string pos)
                {
                    currentSoldier.Position = pos;
                }
                else
                { 
                    errors.AppendLine("Выберите должность.");
                }


                if (currentSoldier.Birth_year < 1960 || currentSoldier.Birth_year > DateTime.Now.Year)
                    errors.AppendLine("Некорректный год рождения.");

                if (currentSoldier.Start_service_year < currentSoldier.Birth_year + 18 ||
                    currentSoldier.Start_service_year > DateTime.Now.Year)
                    errors.AppendLine("Некорректный год начала службы.");

                currentSoldier.Service_experience_years = Math.Max(0, DateTime.Now.Year - currentSoldier.Start_service_year);


                if (string.IsNullOrWhiteSpace(currentSoldier.Images_personnel))
                    currentSoldier.Images_personnel = DefaultImagePath;



                if (PlatoonComboBox.SelectedItem is Platoon selectedPlatoon)
                {
                    currentSoldier.Id_platoon = selectedPlatoon.Id_platoon;
                }
                else
                {
                    errors.AppendLine("Выберите взвод.");
                }



                if (errors.Length > 0)
                {
                    MessageBox.Show(errors.ToString());
                    return;
                }

                if (currentSoldier.Id_personnel == 0)
                {
                    MilitaryDistrictEntities.GetContext().Personnel.Add(currentSoldier);
                }

                try
                {
                    MilitaryDistrictEntities.GetContext().SaveChanges();
                    MessageBox.Show("Информация сохранена");
                    Manager.MainFrame.Navigate(new soldiersPage());
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    string errorMessages = "";
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            errorMessages += $"- {validationError.PropertyName}: {validationError.ErrorMessage}\n";
                        }
                    }

                    MessageBox.Show("Ошибка валидации:\n" + errorMessages);
                }

            }
        }

        private void YearBoxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(BirthYearBox.Text, out var birth)) return;
            if (!int.TryParse(StartYearBox.Text, out var start)) return;

            if (start >= birth + 18 && start <= DateTime.Now.Year)
            {
                var exp = Math.Max(0, DateTime.Now.Year - start);
                currentSoldier.Service_experience_years = exp;

                if (ServiceYearsBox.Text != exp.ToString())
                    ServiceYearsBox.Text = exp.ToString();
            }
        }

        private void UpdateLocationText(Platoon platoon)
        {
            if (platoon == null || platoon.Unit == null || platoon.Unit.Location == null)
            {
                LocationTextBox.Text = string.Empty;
                return;
            }

            var loc = platoon.Unit.Location;

            var parts = new[]
            {
            loc.Country,
            loc.City,
            loc.Address
            }.Where(s => !string.IsNullOrWhiteSpace(s));

                LocationTextBox.Text = string.Join(" ", parts);
        }

        private void PlatoonComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlatoonComboBox.SelectedItem is Platoon selectedPlatoon)
            {
                currentSoldier.Id_platoon = selectedPlatoon.Id_platoon;
                UpdateLocationText(selectedPlatoon);
            }
        }

        private void BranchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                if (!(BranchComboBox.SelectedItem is Branch branch))
                {
                    PlatoonComboBox.ItemsSource = Platoons;
                    return;
                }

                var filtered = Platoons
                    .Where(p => p.Unit != null && p.Unit.Id_branch == branch.id_branch)
                    .ToList();

                PlatoonComboBox.ItemsSource = filtered;

                if (filtered.Any())
                {
                    var first = filtered.First();
                    PlatoonComboBox.SelectedItem = first;
                    currentSoldier.Id_platoon = first.Id_platoon;
                    UpdateLocationText(first);
                }
                else
                {
                    LocationTextBox.Text = string.Empty;
                }
            }
        }

        private void PositionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }
    }
}
