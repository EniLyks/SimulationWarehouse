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
        //Частица класса для работы с таблицей "Свойства товаров", конкретного элемента таблицы "Модели"
        // и управления моделированием путем изменения свойств товаров в данной модели

            //Заполнение лист боксов
        private void LoadListBoxsInPropertyModel()
        {
            if (grBox_Models_ListBox_Models.SelectedIndex != -1)
            {
                List<Product> listProducts_All = db.Products.ToList();//Все товары
                //Отсюда будем удалять товары, которые уже есть в модели и потом выведем этот список в правый ListBox
                List<Product> listProducts_AllButNotCompaire = db.Products.ToList();
                List<Product> listProducts_NotEmptyProperty = db.Products.Include(pp => pp.PropertiesProduct).Where(pp => pp.PropertiesProduct.Count > 0).ToList();
                List<Product> listProducts_InModel = new List<Product>();
                Model model = GetSelectedModel();

                //Далее просто жопа, смысл которой прост. Мы ищем товары, которые уже добавлены в текщую модель
                //чтобы исключить их из ListBox'а со "всеми" объектами (тот, который левее). И собственно, на выходе
                //получаем List со списком не добавленных для данной модели товаров

                foreach (Product prod_NotInModel in listProducts_All)
                {
                    foreach (Product prod_InModels in listProducts_NotEmptyProperty)
                    {
                        bool remove = false;
                        if (remove)
                            break;
                        foreach (PropertiesProduct propertiesProduct in prod_InModels.PropertiesProduct)
                        {
                            if ((prod_NotInModel.IdProduct == propertiesProduct.IdProduct) && (model.IdModel == propertiesProduct.IdModel))
                            {
                                listProducts_InModel.Add(prod_NotInModel);
                                listProducts_AllButNotCompaire.Remove(prod_NotInModel);
                                remove = true;
                                break;
                            }
                        }
                    }
                }

                grBox_PropertyModel_ListBox_AllProducts.ItemsSource = listProducts_AllButNotCompaire.OrderBy(p => p.IndexSort);
                grBox_PropertyModel_ListBox_ProductInModel.ItemsSource = listProducts_InModel;
            }
            else
            {
                grBox_PropertyModel_ListBox_AllProducts.ItemsSource = null;
                grBox_PropertyModel_ListBox_ProductInModel.ItemsSource = null;
            }
        }


        //Блокируем все окно с конкретным свойством модели
        private void BlockingAllElementInPropertyModel(int enable)
        {
            if(enable!=-1)
            {
                grBox_PropertyModel.IsEnabled = true;
                //Также разблокируем кнопку удаление элемента в окне выбора моделей
                grBox_Models_Button_ModelDelete.IsEnabled = true;
            }
            else
            {
                grBox_PropertyModel.IsEnabled = false;
                //Также заблокируем кнопку удаление элемента в окне выбора моделей
                grBox_Models_Button_ModelDelete.IsEnabled = false;
            }
        }

        //Блокируем только те элементы, которые отвечают за редактирования свойств товаров в модели
        private void BlockingEditPropertyProductElement(int enable)
        {
            if (enable != -1)
            {
                grBox_PropertyModel_TextBox_CountInOrder.IsEnabled = true;
                grBox_PropertyModel_TextBox_MinCount.IsEnabled = true;
                grBox_PropertyModel_TextBox_DemandFactor.IsEnabled = true;
            }
            else
            {
                grBox_PropertyModel_TextBox_CountInOrder.IsEnabled = false;
                grBox_PropertyModel_TextBox_MinCount.IsEnabled = false;
                grBox_PropertyModel_TextBox_DemandFactor.IsEnabled = false;
            }
        }


        //Изменение выделенного элемента модели в ListBox'e с моделями
        private void grBox_Models_ListBox_Models_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (grBox_Models_ListBox_Models.SelectedIndex != -1)
            {
                //Загружаем текущую модель
                Model model = GetSelectedModel();             

                //Заполняем поля в форме
                grBox_PropertyModel_TextBox_ModelName.Text = model.NameModel;
                grBox_PropertyModel_TextBox_PopulationCity.Text = Convert.ToString(model.PopulationCity);
                grBox_PropertyModel_TextBox_FEP.Text = Convert.ToString(model.FEP);
                //Выводим список связанных товаров в ListBox
                LoadListBoxsInPropertyModel();
            }
            //Разблокируем или заблокируем окно свойств модели
            BlockingAllElementInPropertyModel(grBox_Models_ListBox_Models.SelectedIndex);

            //Сбрасываем выделение в ListBox'e с товарами модели
            grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex = -1;
        }

        //Смена элемента в listBox'e с товарами конкретной модели
        private void grBox_PropertyModel_ListBox_ProductInModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex!=-1)
            {
                Model model = GetSelectedModel();
                Product product = (Product)grBox_PropertyModel_ListBox_ProductInModel.Items[grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex];
                PropertiesProduct propertiesProduct = db.PropertiesProducts.Where(pp => pp.IdModel == model.IdModel).First(pp => pp.IdProduct == product.IdProduct);

                //Устанавливаем значение свойств товара в текстовые поля
                grBox_PropertyModel_TextBox_CountInOrder.Text = Convert.ToString(propertiesProduct.CountInOrder);
                grBox_PropertyModel_TextBox_MinCount.Text = Convert.ToString(propertiesProduct.MinCount);
                grBox_PropertyModel_TextBox_DemandFactor.Text = Convert.ToString(propertiesProduct.DemandFactor);

                if(SystemAnalysis.SimulationStatus)
                {
                    grBox_Result_ListBox_ProductsInModel.SelectedIndex = grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex;
                }
            }
            else
            {
                grBox_PropertyModel_TextBox_CountInOrder.Text = "";
                grBox_PropertyModel_TextBox_MinCount.Text = "";
                grBox_PropertyModel_TextBox_DemandFactor.Text = "";
            }
            BlockingEditPropertyProductElement(grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex);
        }

        //Двойной клик по объекту в ListBox'e с товарами, не принадлежащим к текущей модели
        private void grBox_PropertyModel_ListBox_AllProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (grBox_PropertyModel_ListBox_AllProducts.SelectedIndex != -1)
            {
                Model model = GetSelectedModel();
                Product product = (Product)grBox_PropertyModel_ListBox_AllProducts.Items[grBox_PropertyModel_ListBox_AllProducts.SelectedIndex];
                PropertiesProduct propertiesProduct = new PropertiesProduct();
                propertiesProduct.IdModel = model.IdModel;
                propertiesProduct.IdProduct = product.IdProduct;
                propertiesProduct.MinCount = 0;
                propertiesProduct.DemandFactor = 1;
                propertiesProduct.CountInOrder = 10;

                //Назначаем элементу максимальный индекс сортировки (будет отображаться в самом конце)
                if (grBox_PropertyModel_ListBox_ProductInModel.Items.Count == 0)
                    propertiesProduct.IndexSort = 1;
                else
                {
                    int maxIndex = db.PropertiesProducts.Where(pp => pp.IdModel == model.IdModel).Max(pp => pp.IndexSort);
                    propertiesProduct.IndexSort = maxIndex + 1;
                }

                db.PropertiesProducts.Add(propertiesProduct);
                db.SaveChanges();

                LoadListBoxsInPropertyModel();

            }
        }


        //Двойной клик по объекту в ListBox'e с товарами, принадлежащим к текущей модели
        private void grBox_PropertyModel_ListBox_ProductInModel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(SystemAnalysis.SimulationStatus))
            {
                if (grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex != -1)
                {
                    Model model = GetSelectedModel();
                    Product product = (Product)grBox_PropertyModel_ListBox_ProductInModel.Items[grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex];
                    PropertiesProduct propertiesProduct = db.PropertiesProducts.Where(pp => pp.IdModel == model.IdModel).First(pp => pp.IdProduct == product.IdProduct);

                    db.PropertiesProducts.Remove(propertiesProduct);
                    db.SaveChanges();

                    LoadListBoxsInPropertyModel();
                }
            }
        }

        //Сохранение изменений в модели и свойств товаров, входящих в неё
        private void grBox_PropertyModel_Button_SaveChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                Model model = GetSelectedModel();     

                //Изменяем текущую модель
                //Индекс выбранной модели
                int selectedIndexListBoxModels = grBox_Models_ListBox_Models.SelectedIndex;
                if (grBox_PropertyModel_TextBox_ModelName.Text != "")
                    model.NameModel = grBox_PropertyModel_TextBox_ModelName.Text;
                if (grBox_PropertyModel_TextBox_PopulationCity.Text != "")
                    model.PopulationCity = Convert.ToInt32(grBox_PropertyModel_TextBox_PopulationCity.Text);
                if (grBox_PropertyModel_TextBox_FEP.Text != "")
                    model.FEP = Convert.ToDouble(grBox_PropertyModel_TextBox_FEP.Text);

                //Сохраняем изменения
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();

                //Изменяем текущий товар
                //Индекс выбранного элемента в ListBox'e с товарами конкретной модели
                int selectedIndexListBoxPropertiesProduct = -1;
                if (grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex != -1)
                {
                    Product product = (Product)grBox_PropertyModel_ListBox_ProductInModel.Items[grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex];
                    PropertiesProduct propertiesProduct = db.PropertiesProducts.Where(pp => pp.IdModel == model.IdModel).First(pp => pp.IdProduct == product.IdProduct);
                    selectedIndexListBoxPropertiesProduct = grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex;

                    if (grBox_PropertyModel_TextBox_CountInOrder.Text != "")
                        propertiesProduct.CountInOrder = Convert.ToInt32(grBox_PropertyModel_TextBox_CountInOrder.Text);
                    if (grBox_PropertyModel_TextBox_MinCount.Text != "")
                        propertiesProduct.MinCount = Convert.ToInt32(grBox_PropertyModel_TextBox_MinCount.Text);
                    if (grBox_PropertyModel_TextBox_DemandFactor.Text != "")
                        propertiesProduct.DemandFactor = Convert.ToInt32(grBox_PropertyModel_TextBox_DemandFactor.Text);

                    //Сохраняем изменения
                    db.Entry(propertiesProduct).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //Индекс выделенного объекта в ListBox'e с товарами, не принадлежащих к выбранной модели
                int selectedIndexListBoxProduct = grBox_PropertyModel_ListBox_AllProducts.SelectedIndex;

                //Перезагружаем список с моделями и устанавливаем выделение на нужную модели
                LoadListBoxModels();
                grBox_Models_ListBox_Models.SelectedIndex = selectedIndexListBoxModels;

                //Перезагружаем лист боксы в окне редактирования модели и устанавливаем нужное выделение в лист боксе
                LoadListBoxsInPropertyModel();
                grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex = selectedIndexListBoxPropertiesProduct;
                grBox_PropertyModel_ListBox_AllProducts.SelectedIndex = selectedIndexListBoxProduct;

                //Если запущено моделирование, нужно установить выделение и в ListBox'e в окне вывода результатов, и
                //изменить свойства у товаров в соответствующем объекте ProductAnalysis
                if (SystemAnalysis.SimulationStatus)
                {
                    grBox_Result_ListBox_ProductsInModel.SelectedIndex = selectedIndexListBoxPropertiesProduct;
                    //Находим изменённый товар
                    Product product = (Product)grBox_PropertyModel_ListBox_ProductInModel.Items[grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex];
                    //Находим изменённое свойство товар
                    PropertiesProduct propertiesProduct = db.PropertiesProducts.Where(pp => pp.IdModel == model.IdModel).First(pp => pp.IdProduct == product.IdProduct);
                    //Находим это свойство в списке со всеми свойствами, и заменяем его на новое
                    foreach(ProductAnalysis pa in SystemAnalysis.ListProductAnalysis)
                    {
                        if(pa.PropertiesProduct.IdProduct == propertiesProduct.IdProduct)
                        {
                            pa.ChangeProductAnalysis(propertiesProduct);
                            break;
                        }
                    }
                }

            }
            catch
            {
                MessageBox.Show("Недопустимые значения в тестовых полях!");
            }
        }

        //Автоматический подсчет КПП
        private void grBox_PropertyModel_Button_AutoFEP_Click(object sender, RoutedEventArgs e)
        {
            Model model = GetSelectedModel();
            double AllFEP = 0;
            foreach(PropertiesProduct pp in model.PropertiesProduct)
            {
                Product product = db.Products.First(p => p.IdProduct == pp.IdProduct);
                AllFEP += product.IFEP;
            }
            grBox_PropertyModel_TextBox_FEP.Text = Convert.ToString(AllFEP);
        }

    }
}
