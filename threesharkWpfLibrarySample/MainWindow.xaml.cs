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

namespace threesharkWpfLibrarySample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Hoge.Content = new Person()
            {
                Name = "あああ",
                Age = 20,
            };
        }

        Random random = new Random(0);

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            //Hoge.Content = new Person()
            //{
            //    Name = System.IO.Path.GetRandomFileName(),
            //    Age = this.random.Next(100),
            //};
            Hoge.Content = new System.IO.FileInfo(@"D:\Users\threeshark\source\gitrepos\threesharkWpfLibrary\threesharkWpfLibrary.sln");
        }
    }

    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}
