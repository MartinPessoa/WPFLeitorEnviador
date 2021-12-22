using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using WPFLeitorEnviador.Services;

namespace WPFLeitorEnviador
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty PastaFonteProperty =
            DependencyProperty.Register("PastaFonte", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PastaDestinoProperty =
           DependencyProperty.Register("PastaDestino", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty StatusProperty =
   DependencyProperty.Register("Status", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public string PastaFonte 
        {
            get { return (string)this.GetValue(PastaFonteProperty); }
            set { this.SetValue(PastaFonteProperty, value); }
        }



        public string PastaDestino 
        {
            get { return (string) this.GetValue(PastaDestinoProperty); }
            set { this.SetValue(PastaDestinoProperty, value); }
        }


        public string Status
        {
            get { return (string)this.GetValue(StatusProperty); }
            set { this.SetValue(StatusProperty, value); }
        }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnPastaFonte_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = ""; // Default file name
            dialog.DefaultExt = ".csv"; // Default file extension
            dialog.Filter = "Text documents (.csv)|*.csv"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dialog.FileName;

                this.PastaFonte = ExtrairPasta(filename);
            }
        }


        private void btnPastaDestino_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = ""; // Default file name
            dialog.DefaultExt = ".csv"; // Default file extension
            dialog.Filter = "Arquivos CSV (.csv)|*.csv"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dialog.FileName;

                this.PastaDestino = ExtrairPasta(filename);
            }
        }


        private string ExtrairPasta(string filename)
        {

            Debug.WriteLine("Nome Arquivo: " + String.Join("\\", filename.Split('\\')));
            Debug.WriteLine("Nome Arquivo: " + String.Join("\\", filename.Split('\\').ToList().SkipLast(1)));
            //Debug.WriteLine("Nome Arquivo: " + filename);
            //Debug.WriteLine("Nome Arquivo: " + filename);


            return String.Join("\\", filename.Split('\\').ToList().SkipLast(1));
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            CSVWriter writer = new(this.PastaDestino, InformarStatus);
            CSVReader reader = new(this.PastaFonte, writer, "TESTE",InformarStatus);

            var odds = await reader.Read();

            if(odds == null)
            {
                InformarStatus("Não foi possível ler nenhuma odd");
                return;
            }

            await writer.Write(odds);
        }

        private void InformarStatus(string status)
        {
            this.Status = status;
        }
    }
}
