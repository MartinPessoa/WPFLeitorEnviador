using iBets.Core.SimulaçõesApostas;
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
using System.Windows.Shapes;

namespace WPFLeitorEnviador
{
    /// <summary>
    /// Lógica interna para Teste4TirosWindow.xaml
    /// </summary>
    public partial class Teste4TirosWindow : Window
    {
        public Teste4TirosWindow()
        {
            InitializeComponent();
        }

        private void BtnSimular_Click(object sender, RoutedEventArgs e)
        {
            BaseAposta b = ApostaTeste4Tiros.Primeira(1, 1);
            
        }
    }
}
