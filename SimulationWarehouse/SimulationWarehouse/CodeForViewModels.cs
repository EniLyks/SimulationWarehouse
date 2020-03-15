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
        //Частица класса для работы с таблицей "Модели" и управления моделированием

        private void LoadListBoxModels()
        {
            //Добавдяем в listModels данные из таблицы Models
            List<Model> listModels = db.Models.Include(pp => pp.PropertiesProduct).OrderBy(p => p.IndexSort).ToList();
            //Заполняем listBox этими данными
            grBox_Models_ListBox_Models.ItemsSource = listModels;
        }


        //При нажатии на кнопку создания новой модели
        private void grBox_Models_Button_ModelAdd_Click(object sender, RoutedEventArgs e)
        {
            Model model = new Model();
            model.NameModel = "Модель №" + (grBox_Models_ListBox_Models.Items.Count + 1);
            model.PopulationCity = 100000;
            model.FEP = 1;
            //Устанавливаем максимальное значение для поля сортировки, для этого выбираем последний элемент ListBox'a или приравниваем сортировку к 1, если элементов ещё нет
            if (grBox_Models_ListBox_Models.Items.Count == 0)
                model.IndexSort = 1;
            else
            {
                Model modelWithMaxIndexSort = (Model)grBox_Models_ListBox_Models.Items[grBox_Models_ListBox_Models.Items.Count - 1];
                model.IndexSort = modelWithMaxIndexSort.IndexSort + 1;
            }

            //Заносим его в БД и сохраняем
            db.Models.Add(model);
            db.SaveChanges();
            //Обновляем listBox
            LoadListBoxModels();
        }

        //При нажатии на кнопку удаления модели
        private void grBox_Models_Button_ModelDelete_Click(object sender, RoutedEventArgs e)
        {
            //Извлекаем объект типа модели из выбранного в ListBox'e элемента
            Model model = GetSelectedModel();
            db.Models.Remove(model);
            db.SaveChanges();
            //Обновляем listBox
            LoadListBoxModels();

            LoadListBoxsInPropertyModel();
        }

        //Получаем текущую выбранную модель
        private Model GetSelectedModel()
        {
            //Извлекаем объект типа модели из выбранного в ListBox'e элемента
            Model model = (Model)grBox_Models_ListBox_Models.Items[grBox_Models_ListBox_Models.SelectedIndex];
            model.PropertiesProduct = db.PropertiesProducts.Where(pp => pp.IdModel == model.IdModel).ToList();
            return model;
        }

        //Закрытие(открытие) тех элементов, изменение которых может спровоцировать ошибки в процессе моделирования
        private void GeneralElementStatusInSimulation(bool openElement)
        {
            if (openElement)
            {
                grBox_Models_ListBox_Models.IsEnabled = true;
                grBox_Models_Button_ModelAdd.IsEnabled = true;
                grBox_Models_Button_ModelDelete.IsEnabled = true;
                grBox_PropertyModel_TextBox_ModelName.IsEnabled = true;
                grBox_PropertyModel_TextBox_PopulationCity.IsEnabled = true;
                grBox_PropertyModel_TextBox_FEP.IsEnabled = true;
                grBox_PropertyModel_Button_AutoFEP.IsEnabled = true;
                grBox_PropertyModel_ListBox_AllProducts.IsEnabled = true;
                groupBox_EditProduct.IsEnabled = true;

                //Но закрываем те окна, что относятся к моделированию
                grBox_Models_TextBox_SimulationPeriod.IsEnabled = false;
                grBox_Models_Button_NextDay.IsEnabled = false;
                grBox_Models_Button_AllPeriod.IsEnabled = false;
                grBox_Models_TextBox_SimulationAllPeriod.IsEnabled = false;
            }
            else
            {
                grBox_Models_ListBox_Models.IsEnabled = false;
                grBox_Models_Button_ModelAdd.IsEnabled = false;
                grBox_Models_Button_ModelDelete.IsEnabled = false;
                grBox_PropertyModel_TextBox_ModelName.IsEnabled = false;
                grBox_PropertyModel_TextBox_PopulationCity.IsEnabled = false;
                grBox_PropertyModel_TextBox_FEP.IsEnabled = false;
                grBox_PropertyModel_Button_AutoFEP.IsEnabled = false;
                grBox_PropertyModel_ListBox_AllProducts.IsEnabled = false;
                groupBox_EditProduct.IsEnabled = false;

                grBox_Models_TextBox_SimulationPeriod.IsEnabled = true;
                grBox_Models_Button_NextDay.IsEnabled = true;
                grBox_Models_Button_AllPeriod.IsEnabled = true;
                grBox_Models_TextBox_SimulationAllPeriod.IsEnabled = true;
            }
        }

        //Нажатие на кнопку начала/конца моделирования
        private void grBox_Models_Button_SimulationStatus_Click(object sender, RoutedEventArgs e)
        {
            //Проверяем правильность введённых данных
            if(!(Int32.TryParse(grBox_Models_TextBox_SimulationAllPeriod.Text,out int _)))
            {
                MessageBox.Show("Некорректный ввод поля \"Длительность моделирования\"!");
                return;
            }

            if (grBox_Models_ListBox_Models.SelectedIndex != -1)
            {
                Simulation(SystemAnalysis.SimulationStatus);
            }
            else
            {
                MessageBox.Show("Выберите модель!");
            }
        }

        //Обработка событий дня
        private void NewDayOfAnalysis()
        {

            //Объект случайного генератора
            RandomLemer randomLemer = new RandomLemer();
            Random random = new Random();

            Model model = GetSelectedModel();
            int countPurchase = (int)(model.PopulationCity*(model.FEP/365));//Общее число покупок, которое будет совершено

            //Заполняем лист с вероятностями
            SystemAnalysis.LoadListProbability();

            //Заполняем массив случайными числами. Позже, эти случайные числа будут отвечать за покупку конкретного
            //товары, выступая в качестве индекса элемента массива ListPurchaseProbability
            int[] PurchasedsGoods = new int[countPurchase] ; 

            for(int i=0; i < countPurchase; i++)
            {
                //PurchasedsGoods[i] = (int)Math.Round(randomLemer.GetTheNumber((double)0, SystemAnalysis.ListPurchaseProbability.Count - 1));
                PurchasedsGoods[i] = random.Next(0, SystemAnalysis.ListPurchaseProbability.Count);
            }

            //Закупаем товары
            foreach (ProductAnalysis productAnalysis in SystemAnalysis.ListProductAnalysis)
            {
                productAnalysis.MakeAnOrderIfNeeded();
            }

            //Совершаем покупки
            for (int i = 0; i < countPurchase; i++)
            {
                //Находим индекс элемента, к которому будет совершена операция покупки
                int index = PurchasedsGoods[i];
                //Выбираем этот элемент
                ProductAnalysis productAnalysis = SystemAnalysis.ListProductAnalysis
                    .First(pa => pa.PropertiesProduct.IdProduct == SystemAnalysis.ListPurchaseProbability[index]);
                productAnalysis.PurchaseOfGoods();
            }


            if (grBox_Result_ListBox_ProductsInModel.SelectedIndex != -1)
            {
                //Редактируем поля выводов конкретного товара
                //Извлекаем выбранный в ListBox'е продукт
                Product selectProduct = (Product)grBox_Result_ListBox_ProductsInModel
                    .Items[grBox_Result_ListBox_ProductsInModel.SelectedIndex];
                //Находим в общем списке данный анализируемый объект
                ProductAnalysis nullProductAnalysis = SystemAnalysis.ListProductAnalysis
                        .First(pa => pa.PropertiesProduct.IdProduct == selectProduct.IdProduct);
                //Выводим его данные в текстовые поля
                OutSelectProductProperties(nullProductAnalysis);
            }
            else
            {
                //Иначе оставляем поля пустыми
                //Создаём пустой объект, чтобы очистить текстовые вывода результатов продаж конкретного товара
                ProductAnalysis productAnalysis = null;
                OutSelectProductProperties(productAnalysis);
            }
            

        }

        //Моделирование сразу до конца периода
        private void grBox_Models_Button_AllPeriod_Click(object sender, RoutedEventArgs e)
        {
            int period = Convert.ToInt32(grBox_Models_TextBox_SimulationAllPeriod.Text) - Convert.ToInt32(grBox_Models_TextBox_SimulationPeriod.Text);

            for (int i = 0; i < period; i++)
            {
                NewDayOfAnalysis();
                //Указываем день моделирования
                SystemAnalysis.SimulationDay++;
            }
            OutAllProperties();

            //Перестраиваем графики для товара
            //Извлекаем выбранный в ListBox'е продукт
            Product selectProduct = (Product)grBox_Result_ListBox_ProductsInModel
                .Items[grBox_Result_ListBox_ProductsInModel.SelectedIndex];
            //Находим в общем списке данный анализируемый объект
            ProductAnalysis productAnalysis = SystemAnalysis.ListProductAnalysis
                    .First(pa => pa.PropertiesProduct.IdProduct == selectProduct.IdProduct);
            GraphBuilder(selectProduct, productAnalysis);

            
            grBox_Models_TextBox_SimulationPeriod.Text = Convert.ToString(SystemAnalysis.SimulationDay);
        }

        //Операции, производимые при начале или конца моделирования
        private void Simulation(bool status)
        {
            if (status == false)
            {
                SystemAnalysis.SimulationStatus = true;
                GeneralElementStatusInSimulation(false);
                grBox_Models_Button_SimulationStatus.Content = "Закончить моделирование";
                grBox_Models_TextBox_SimulationAllPeriod.IsEnabled = false;
                grBox_Models_TextBox_SimulationPeriod.Text = "0";

                //Очищаем объект SystemAnalysis для нового моделирования
                SystemAnalysis = null;
                //Загружаем в объект SystemAnalysis все данные
                LoadModelInSystemAnalysis();
                NewDayOfAnalysis();
                OutAllProperties();

                //Устанавливаем привязку между ListBox'ами
                grBox_Result_ListBox_ProductsInModel.ItemsSource = grBox_PropertyModel_ListBox_ProductInModel.Items;

                //Указываем день моделирования
                SystemAnalysis.SimulationDay++;
                grBox_Models_TextBox_SimulationPeriod.Text = Convert.ToString(SystemAnalysis.SimulationDay);
            }
            else
            {
                SystemAnalysis.SimulationStatus = false;
                GeneralElementStatusInSimulation(true);
                grBox_Models_Button_SimulationStatus.Content = "Начать моделирование";
                grBox_Models_TextBox_SimulationAllPeriod.IsEnabled = true;

                //Сбрасываем привязку между ListBox'ами
                grBox_Result_ListBox_ProductsInModel.ItemsSource = null;

                //Обнуляем текст бокс с периодом моделирования
                grBox_Models_TextBox_SimulationPeriod.Text = "";
                grBox_Models_TextBox_SimulationAllPeriod.Text = "";
            }
        }

        //Нажатие на кнопку следующего дня моделирования
        private void grBox_Models_Button_NextDay_Click(object sender, RoutedEventArgs e)
        {
            if (!(Convert.ToInt32(grBox_Models_TextBox_SimulationAllPeriod.Text) == Convert.ToInt32(grBox_Models_TextBox_SimulationPeriod.Text)))
            {
                NewDayOfAnalysis();
                OutAllProperties();


                //Перестраиваем графики для товара
                //Извлекаем выбранный в ListBox'е продукт
                Product selectProduct = (Product)grBox_Result_ListBox_ProductsInModel
                .Items[grBox_Result_ListBox_ProductsInModel.SelectedIndex];
                //Находим в общем списке данный анализируемый объект
                ProductAnalysis productAnalysis = SystemAnalysis.ListProductAnalysis
                        .First(pa => pa.PropertiesProduct.IdProduct == selectProduct.IdProduct);
                GraphBuilder(selectProduct, productAnalysis);

                //Указываем день моделирования

                SystemAnalysis.SimulationDay++;
                grBox_Models_TextBox_SimulationPeriod.Text = Convert.ToString(SystemAnalysis.SimulationDay);
            }
            else
            {
                MessageBox.Show("Достигнут предел длительности моделирования!");
            }
        }

    }
}
