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
using System.Data.Entity;
using LemerGenerator;

namespace SimulationWarehouse
{
    public partial class MainWindow : Window
    {
        //Частица класса для работы с таблицей "Товары"



        //Загружаем данные товаров в listBox
        private void LoadListBoxProduct()
        {
            //Добавдяем в listProducts данные из таблицы Products
            List<Product> listProducts = db.Products.OrderBy(p => p.IndexSort).ToList();
            //Заполняем listBox этими данными
            groupBox_EditProduct_ListBox_Products.ItemsSource = listProducts;
        }

        //Включение/отключение элементов
        private void EnableElementsEditProduct(int enable)
        {
            if(enable!=-1)
            {
                groupBox_EditProduct_TextBox_NameProduct.IsEnabled = true;
                groupBox_EditProduct_TextBox_IFEP.IsEnabled = true;
                groupBox_EditProduct_Button_DeleteProduct.IsEnabled = true;
                groupBox_EditProduct_Button_SaveProduct.IsEnabled = true;
                groupBox_EditProduct_Button_ProductUp.IsEnabled = true;
                groupBox_EditProduct_Button_ProductDown.IsEnabled = true;
                groupBox_EditProduct_TextBox_Divider.IsEnabled = true;
            }
            else
            {
                groupBox_EditProduct_TextBox_NameProduct.IsEnabled = false;
                groupBox_EditProduct_TextBox_IFEP.IsEnabled = false;
                groupBox_EditProduct_Button_DeleteProduct.IsEnabled = false;
                groupBox_EditProduct_Button_SaveProduct.IsEnabled = false;
                groupBox_EditProduct_Button_ProductUp.IsEnabled = false;
                groupBox_EditProduct_Button_ProductDown.IsEnabled = false;
                groupBox_EditProduct_TextBox_Divider.IsEnabled = false;
            }
        }

        //При изменении поля в ListBox'e
        private void groupBox_EditProduct_ListBox_Products_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableElementsEditProduct(groupBox_EditProduct_ListBox_Products.SelectedIndex);

            //Изменяем значения элементов TextBox в соответствии с выбранным объектом в ListBox
            if(groupBox_EditProduct_ListBox_Products.SelectedIndex!=-1)
            {
                Product product = (Product)groupBox_EditProduct_ListBox_Products.Items[groupBox_EditProduct_ListBox_Products.SelectedIndex];
                groupBox_EditProduct_TextBox_NameProduct.Text = product.NameProduct;
                groupBox_EditProduct_TextBox_IFEP.Text = Convert.ToString(product.IFEP);
            }
            else
            {
                groupBox_EditProduct_TextBox_NameProduct.Text = "";
                groupBox_EditProduct_TextBox_IFEP.Text = "";
            }
        }

        //При нажатии на кнопку создания нового товара
        private void groupBox_EditProduct_Button_NewProduct_Click(object sender, RoutedEventArgs e)
        {
            //Создаём новый товар
            Product product = new Product();
            product.NameProduct = "Товар №" + (groupBox_EditProduct_ListBox_Products.Items.Count + 1);
            product.IFEP = 1;

            //Устанавливаем максимальное значение для поля сортировки, для этого выбираем последний элемент ListBox'a или приравниваем сортировку к 1, если элементов ещё нет
            if (groupBox_EditProduct_ListBox_Products.Items.Count == 0)
                product.IndexSort = 1;
            else
            {
                Product productWithMaxIndexSort = (Product)groupBox_EditProduct_ListBox_Products.Items[groupBox_EditProduct_ListBox_Products.Items.Count - 1];
                product.IndexSort = productWithMaxIndexSort.IndexSort + 1;
            }

            //Заносим его в БД и сохраняем
            db.Products.Add(product);
            db.SaveChanges();
            //Обновляем listBox
            LoadListBoxProduct();
            //И ещё обновляем listBox в окне свойств конкретной модели
            LoadListBoxsInPropertyModel();
            //Устанваливаем фокус на последнем элементе
            groupBox_EditProduct_ListBox_Products.SelectedIndex = groupBox_EditProduct_ListBox_Products.Items.Count - 1;
        }

        //При нажатии на кнопку сохранения товара
        private void groupBox_EditProduct_Button_SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Индекс выбранного элемента
                int selectedIndex = groupBox_EditProduct_ListBox_Products.SelectedIndex;
                //Извлекаем объект типа товара из выбранного в ListBox'e элемента
                Product product = (Product)groupBox_EditProduct_ListBox_Products.Items[groupBox_EditProduct_ListBox_Products.SelectedIndex];
                if (groupBox_EditProduct_TextBox_NameProduct.Text != "")
                    product.NameProduct = groupBox_EditProduct_TextBox_NameProduct.Text;
                product.IFEP = Math.Round(Convert.ToDouble(groupBox_EditProduct_TextBox_IFEP.Text)/ Convert.ToDouble(groupBox_EditProduct_TextBox_Divider.Text));
                
                //Возвращаем TextBox'у с делителем обычное значение
                groupBox_EditProduct_TextBox_Divider.Text = "1";
                //А TextBox'у со значением ИККП товара результат вычисления
                groupBox_EditProduct_TextBox_IFEP.Text = Convert.ToString(product.IFEP);

                //Сохраняем изменения в БД
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                //Обновляем listBox
                LoadListBoxProduct();
                //И ещё обновляем listBox в окне свойств конкретной модели
                LoadListBoxsInPropertyModel();
                //Устанваливаем фокус на сохранённом товаре
                groupBox_EditProduct_ListBox_Products.SelectedIndex = selectedIndex;
            }
            catch
            {
                MessageBox.Show("Недопустимые значения в тестовых полях!");
            }
        }

        //При нажатии на кнопку удаления товара
        private void groupBox_EditProduct_Button_DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            //Извлекаем объект типа товара из выбранного в ListBox'e элемента
            Product product = (Product)groupBox_EditProduct_ListBox_Products.Items[groupBox_EditProduct_ListBox_Products.SelectedIndex];
            db.Products.Remove(product);
            db.SaveChanges();
            //Обновляем listBox
            LoadListBoxProduct();
            //И ещё обновляем listBox в окне свойств конкретной модели
            LoadListBoxsInPropertyModel();
        }

        //Сдвигаем позицию элемента вверх на единицу
        private void groupBox_EditProduct_Button_ProductUp_Click(object sender, RoutedEventArgs e)
        {
            //Проверяем, вдруг выбранный элемент уже и так выше всех
            if(groupBox_EditProduct_ListBox_Products.SelectedIndex!=0)
            {
                //Индекс выбранного элемента
                int selectedIndex = groupBox_EditProduct_ListBox_Products.SelectedIndex;

                //Элемент, который мы, собственно, и сдвигаем вверх
                Product productMoveUp = (Product)groupBox_EditProduct_ListBox_Products.Items[groupBox_EditProduct_ListBox_Products.SelectedIndex];

                //Элемент, который мы  и сдвигаем вниз
                Product productMoveDown = (Product)groupBox_EditProduct_ListBox_Products.Items[groupBox_EditProduct_ListBox_Products.SelectedIndex-1];

                //Сохраняем индексы элементов в переменные
                int upperIndex = productMoveDown.IndexSort;
                int lowerIndex = productMoveUp.IndexSort;

                productMoveUp.IndexSort = upperIndex;
                //Сохраняем изменения в БД
                db.Entry(productMoveUp).State = EntityState.Modified;
                db.SaveChanges();

                productMoveDown.IndexSort = lowerIndex;
                //Сохраняем изменения в БД
                db.Entry(productMoveDown).State = EntityState.Modified;
                db.SaveChanges();

                //Обновляем listBox
                LoadListBoxProduct();
                //И ещё обновляем listBox в окне свойств конкретной модели
                LoadListBoxsInPropertyModel();
                //Устанваливаем фокус на передвинутом товаре
                groupBox_EditProduct_ListBox_Products.SelectedIndex = selectedIndex-1;
            }
        }

        //Сдвигаем позицию элемента вниз на единицу
        private void groupBox_EditProduct_Button_ProductDown_Click(object sender, RoutedEventArgs e)
        {
            //Проверяем, вдруг выбранный элемент уже и так выше всех
            if (groupBox_EditProduct_ListBox_Products.SelectedIndex != groupBox_EditProduct_ListBox_Products.Items.Count - 1)
            {
                //Индекс выбранного элемента
                int selectedIndex = groupBox_EditProduct_ListBox_Products.SelectedIndex;

                //Элемент, который мы  сдвигаем вверх
                Product productMoveUp = (Product)groupBox_EditProduct_ListBox_Products.Items[groupBox_EditProduct_ListBox_Products.SelectedIndex + 1];

                //Элемент, который мы, собственно, и сдвигаем вниз
                Product productMoveDown = (Product)groupBox_EditProduct_ListBox_Products.Items[groupBox_EditProduct_ListBox_Products.SelectedIndex];

                //Сохраняем индексы элементов в переменные
                int upperIndex = productMoveDown.IndexSort;
                int lowerIndex = productMoveUp.IndexSort;

                productMoveUp.IndexSort = upperIndex;
                //Сохраняем изменения в БД
                db.Entry(productMoveUp).State = EntityState.Modified;
                db.SaveChanges();

                productMoveDown.IndexSort = lowerIndex;
                //Сохраняем изменения в БД
                db.Entry(productMoveDown).State = EntityState.Modified;
                db.SaveChanges();

                //Обновляем listBox
                LoadListBoxProduct();
                //Устанваливаем фокус на передвинутом товаре
                groupBox_EditProduct_ListBox_Products.SelectedIndex = selectedIndex + 1;
            }
        }
    }
}
