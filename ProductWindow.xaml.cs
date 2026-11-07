using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace ProductShop
{
    public partial class ProductWindow : UserControl
    {
        private Product _editingProduct;
        private string _newImagePath;
        private bool _isNew;

        // События для связи с MainWindow
        public event Action<Product> OnSave;
        public event Action OnCancel;

        public bool IsNewProduct => _isNew;

        public ProductWindow()
        {
            InitializeComponent();
        }

        public void SetProduct(Product product)
        {
            _editingProduct = product;
            _isNew = product == null;

            if (_isNew)
            {
                HeaderTitle.Text = "Добавление продукта";
                TxtName.Text = "";
                TxtPrice.Text = "";
                TxtCount.Text = "";
                TxtDescription.Text = "";
                TxtImage.Text = "";
                PreviewImage.Source = null;
                _newImagePath = null;
            }
            else
            {
                HeaderTitle.Text = "Изменение продукта";
                TxtName.Text = product.Name;
                TxtPrice.Text = product.Price.ToString();
                TxtCount.Text = product.Count.ToString();
                TxtDescription.Text = product.Description;
                TxtImage.Text = product.Image;
                _newImagePath = null;
                UpdatePreview(product.Image);
            }
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите изображение продукта",
                Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Все файлы|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedPath = openFileDialog.FileName;
                try
                {
                    BitmapImage test = new BitmapImage(new Uri(selectedPath));
                    _newImagePath = CopyImageToProject(selectedPath);
                    TxtImage.Text = _newImagePath;
                    PreviewImage.Source = test;
                }
                catch (Exception)
                {
                    MessageBox.Show("Выбранный файл не является корректным изображением.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string CopyImageToProject(string sourcePath)
        {
            string destFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            string ext = Path.GetExtension(sourcePath);
            string fileName = $"{Guid.NewGuid()}{ext}";
            string destPath = Path.Combine(destFolder, fileName);

            File.Copy(sourcePath, destPath, true);
            return destPath;
        }

        private void UpdatePreview(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                PreviewImage.Source = null;
                return;
            }
            try
            {
                PreviewImage.Source = new BitmapImage(new Uri(path));
            }
            catch
            {
                PreviewImage.Source = null;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            { ShowError("Название не может быть пустым!"); return; }
            if (!decimal.TryParse(TxtPrice.Text, out decimal price) || price <= 0)
            { ShowError("Цена должна быть положительным числом!"); return; }
            if (!int.TryParse(TxtCount.Text, out int count) || count < 0)
            { ShowError("Количество должно быть целым неотрицательным числом!"); return; }
            if (string.IsNullOrWhiteSpace(TxtImage.Text))
            { ShowError("Необходимо выбрать изображение продукта!"); return; }

            Product result = new Product
            {
                Name = TxtName.Text.Trim(),
                Price = decimal.Parse(TxtPrice.Text),
                Count = int.Parse(TxtCount.Text),
                Description = TxtDescription.Text.Trim(),
                Image = _newImagePath ?? (_editingProduct?.Image ?? TxtImage.Text.Trim())
            };

            OnSave?.Invoke(result);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancel?.Invoke();
        }

        private void ShowError(string msg) =>
            MessageBox.Show(msg, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);

        private void PreviewImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
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