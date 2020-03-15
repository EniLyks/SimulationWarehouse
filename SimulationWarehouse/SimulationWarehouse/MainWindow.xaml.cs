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
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataModelContext db = new DataModelContext();
        //Объект со всеми данными для расчетов и вывода результатов
        SystemAnalysis SystemAnalysis;
        public MainWindow()
        {
            InitializeComponent();

            LoadListBoxProduct();//Заполняем ListBox товарами в окне редактирования товаров
            LoadListBoxModels();//Загружаем список моделей в ListBox с моделями
            

            BlockingAllElementInPropertyModel(grBox_PropertyModel_ListBox_AllProducts.SelectedIndex);
            BlockingEditPropertyProductElement(grBox_PropertyModel_ListBox_ProductInModel.SelectedIndex);


        }


    }
}
