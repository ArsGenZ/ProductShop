using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProductShop
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Product> products = new ObservableCollection<Product>();
        private int nextId = 1;
        private Product selectedProduct;

        // Цвета
        private static readonly SolidColorBrush ActiveColor = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // зелёный #4CAF50
        private static readonly SolidColorBrush EnabledColor = Brushes.White;                                  // белый
        private static readonly SolidColorBrush DisabledColor = Brushes.Gray;                                  // серый


        public MainWindow()
        {
            InitializeComponent();
            ProductsListBox.ItemsSource = products;

            ProductFormControl.OnSave += HandleProductSave;
            ProductFormControl.OnCancel += HandleProductCancel;

            LoadInitData();
            ShowList();
        }

        private void LoadInitData()
        {
            products.Add(new Product
            {
                Id = nextId++,
                Name = "Яблоки",
                Price = 370m,
                Count = 140,
                Description = "Представляем вашему вниманию сочные и ароматные яблоки, " +
                              "выращенные в экологически чистых садах с заботой о вашем здоровье!",
                Image = @"\\iscsi\profstud\NikitinAN\Pictures\Screenshots\яблоко.png"
            });
            products.Add(new Product
            {
                Id = nextId++,
                Name = "Виноград",
                Price = 370m,
                Count = 270,
                Description = "Зеленый",
                Image = @"\\iscsi\profstud\NikitinAN\Pictures\Screenshots\Виноград.png"
            });
        }

        private void ApplyButtonStyle(Button button, bool isActive, bool isEnabled)
        {
            bool finalEnabled = isActive || isEnabled;
            button.IsEnabled = finalEnabled;

            if (isActive)
            {
                button.Background = ActiveColor;
                button.FontWeight = FontWeights.Bold;
            }
            else if (isEnabled)
            {
                button.Background = EnabledColor;
                button.FontWeight = FontWeights.Normal;
            }
            else
            {
                button.Background = DisabledColor;
                button.FontWeight = FontWeights.Normal;
            }
        }


        private void UpdateButtonsState(string activeTab)
        {
            bool hasSelection = ProductsListBox.SelectedItem != null;
            bool isListVisible = ProductsListBox.Visibility == Visibility.Visible;

            ApplyButtonStyle(ViewButton, activeTab == "View", true);

            ApplyButtonStyle(AddButton, activeTab == "Add", true);

            ApplyButtonStyle(DeleteButton, activeTab == "Delete", isListVisible && hasSelection);
            ApplyButtonStyle(EditButton, activeTab == "Edit", isListVisible && hasSelection);
        }

        private void ShowList()
        {
            ProductsListBox.SelectedItem = null;
            ProductsListBox.Visibility = Visibility.Visible;
            ProductFormControl.Visibility = Visibility.Collapsed;
            UpdateButtonsState("View");
        }

        private void ShowForm(string tab)
        {
            bool hasSelection = ProductsListBox.SelectedItem != null;
            ProductsListBox.Visibility = Visibility.Collapsed;
            ProductFormControl.Visibility = Visibility.Visible;
            UpdateButtonsState(tab);
        }

        private void ProductsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsListBox.Visibility == Visibility.Visible)
            {
                UpdateButtonsState("View");
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            ShowList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ProductFormControl.SetProduct(null);
            ShowForm("Add");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsListBox.SelectedItem is Product prod)
            {
                selectedProduct = prod;
                ProductFormControl.SetProduct(prod);
                ShowForm("Edit");
            }
        }

        private void HandleProductSave(Product product)
        {
            if (ProductFormControl.IsNewProduct)
            {
                product.Id = nextId++;
                products.Add(product);
                MessageBox.Show("Продукт успешно добавлен!", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (selectedProduct != null)
                {
                    string oldImagePath = selectedProduct.Image;
                    selectedProduct.Name = product.Name;
                    selectedProduct.Price = product.Price;
                    selectedProduct.Description = product.Description;
                    selectedProduct.Count = product.Count;

                    if (oldImagePath != product.Image && IsProjectImage(oldImagePath))
                    {
                        try { File.Delete(oldImagePath); } catch { }
                    }
                    selectedProduct.Image = product.Image;

                    int index = products.IndexOf(selectedProduct);
                    products.RemoveAt(index);
                    products.Insert(index, selectedProduct);
                }
                MessageBox.Show("Продукт успешно изменен!", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            ShowList();
        }

        private void HandleProductCancel()
        {
            ShowList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsListBox.SelectedItem is Product prod)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить продукт \"{prod.Name}\"?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (IsProjectImage(prod.Image))
                    {
                        try { File.Delete(prod.Image); } catch { }
                    }

                    products.Remove(prod);
                    MessageBox.Show("Продукт успешно удален!", "Уведомление",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            ShowList();
        }

        private bool IsProjectImage(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string imagesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            return path.StartsWith(imagesFolder, StringComparison.OrdinalIgnoreCase);
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (sender is Image img)
            {
                try
                {
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/placeholder.png"));
                }
                catch { }
            }
        }
    }
}