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
        //Частица класса для выводы результатов

            
        private void grBox_Result_ListBox_ProductsInModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //При изменении поля в ListBoxe'е результатов, меняем поле в ListBox'е со свойствами товаров в модели
            grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex = grBox_Result_ListBox_ProductsInModel.SelectedIndex;

            if (grBox_Result_ListBox_ProductsInModel.SelectedIndex != -1)
            {
                //Извлекаем выбранный в ListBox'е продукт
                Product selectProduct = (Product)grBox_Result_ListBox_ProductsInModel
                    .Items[grBox_Result_ListBox_ProductsInModel.SelectedIndex];
                //Находим в общем списке данный анализируемый объект
                ProductAnalysis productAnalysis = SystemAnalysis.ListProductAnalysis
                        .First(pa => pa.PropertiesProduct.IdProduct == selectProduct.IdProduct);
                //Выводим его данные в текстовые поля
                OutSelectProductProperties(productAnalysis);
                //Строим графики
                GraphBuilder(selectProduct, productAnalysis);
            }
            else
            {
                //Иначе оставляем поля пустыми
                ProductAnalysis productAnalysis = null;
                OutSelectProductProperties(productAnalysis);
                //Очищаем графики
                GraphBuilder();
            }
        }

        //Загружаем данные в объект SystemAnalysis
        private void LoadModelInSystemAnalysis()
        {
            SystemAnalysis = new SystemAnalysis();
            SystemAnalysis.Model = GetSelectedModel();

            // Добавляем все товары, принадлежащие к выбранной модели в классы анализа
            //то есть, создаём в SystemAnalysis list с ProductAnalysis
            foreach (PropertiesProduct pp in SystemAnalysis.Model.PropertiesProduct)
            {
                ProductAnalysis productAnalysis = new ProductAnalysis(pp);
                SystemAnalysis.ListProductAnalysis.Add(productAnalysis);
            }
        }

        //Выносим в textBox'ы общие результаты моделирования
        private void OutAllProperties()
        {
            grBox_Result_TextBox_AllCountOrders.Text = Convert.ToString(SystemAnalysis.AllCountOrders());
            grBox_Result_TextBox_AllSuccessfulOrders.Text = Convert.ToString(SystemAnalysis.AllSuccessfulOrders());
            grBox_Result_TextBox_AllUnSuccessfulOrders.Text = Convert.ToString(SystemAnalysis.AllUnSuccessfulOrders());
            grBox_Result_TextBox_ProportionUnsuccessfulAllOrders.Text = Convert.ToString(Math.Round(SystemAnalysis.ProportionUnsuccessfulAllOrders(),5)*100) +" %";
        }

        private void OutSelectProductProperties(ProductAnalysis productAnalysis)
        {
            if (productAnalysis != null)
            {
                grBox_Result_TextBox_ProductCountOrders.Text = Convert.ToString(productAnalysis.CountOrders()) + "  |  "
                    + Convert.ToString(productAnalysis.CountOrdersToday());
                grBox_Result_TextBox_ProductSuccessfulOrders.Text = 
                    Convert.ToString(productAnalysis.SuccessfulOrders) + "  |  "
                    + Convert.ToString(productAnalysis.SuccessfulOrdersToday);
                grBox_Result_TextBox_ProductUnSuccessfulOrders.Text = 
                    Convert.ToString(productAnalysis.UnSuccessfulOrders) +"  |  "
                    + Convert.ToString(productAnalysis.UnSuccessfulOrdersToday);
                grBox_Result_TextBox_ProportionUnsuccessfulProductOrders.Text = 
                    Convert.ToString(Math.Round(productAnalysis.ProportionUnsuccessfulOrders(), 5) * 100) + "%  |  "
                    + Convert.ToString(Math.Round(productAnalysis.ProportionUnsuccessfulOrdersToday(), 5) * 100) + "%";
                grBox_Result_TextBox_BalanceProduct.Text = Convert.ToString(productAnalysis.CurrentAmount);
            }
            else
            {
                grBox_Result_TextBox_ProductCountOrders.Text = "";
                grBox_Result_TextBox_ProductSuccessfulOrders.Text = "";
                grBox_Result_TextBox_ProductUnSuccessfulOrders.Text = "";
                grBox_Result_TextBox_ProportionUnsuccessfulProductOrders.Text = "";
                grBox_Result_TextBox_BalanceProduct.Text = "";
            }
        }

        //Вывод информации о результате моделировании товара на графики
        private void GraphBuilder(Product product = null, ProductAnalysis productAnalysis = null)
        {
            if (product != null)
            {
                grBox_Graph_TextBlock_ProductName.Text = product.NameProduct;

                //Заполняем график с информацией о моделировании товара за весь период
                List<KeyValuePair<string, int>> DataForAllPeriod = new List<KeyValuePair<string, int>>();
                DataForAllPeriod.Add(new KeyValuePair<string, int>("Проданные товары", productAnalysis.SuccessfulOrders));
                DataForAllPeriod.Add(new KeyValuePair<string, int>("Недостаток", productAnalysis.UnSuccessfulOrders));
                PieChartAllPeriod.DataContext = DataForAllPeriod;

                //Заполняем график с информацией о моделировании товара за текущий день
                List<KeyValuePair<string, int>> DataForOneDay = new List<KeyValuePair<string, int>>();
                DataForOneDay.Add(new KeyValuePair<string, int>("Проданные товары", productAnalysis.SuccessfulOrdersToday));
                DataForOneDay.Add(new KeyValuePair<string, int>("Недостаток", productAnalysis.UnSuccessfulOrdersToday));
                PieChartToday.DataContext = DataForOneDay;
            }
            else
            {
                //Если товар не выбран, очищаем графики
                grBox_Graph_TextBlock_ProductName.Text = "Название товара";
                PieChartAllPeriod.DataContext = null;
                PieChartToday.DataContext = null;
            }
        }

    }
}
