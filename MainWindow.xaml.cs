using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPFLeitorEnviador.Services;
using WPFLeitorEnviador.Services.CSVReaderProcessors;

namespace WPFLeitorEnviador
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _começouLeituraOdds = false;
        private bool _começouLeituraResultados = false;

        private CancellationTokenSource cancelSourceOdds = new();
        private CancellationTokenSource cancelSourceResultados = new();

        // **************************************** ODDS **************************************************************************
        public static readonly DependencyProperty PastaEuroFonteProperty =
            DependencyProperty.Register("PastaEuroFonte", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PastaCopaFonteProperty =
            DependencyProperty.Register("PastaCopaFonte", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PastaPremierFonteProperty =
            DependencyProperty.Register("PastaPremierFonte", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PastaSuperFonteProperty =
            DependencyProperty.Register("PastaSuperFonte", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PastaDestinoProperty =
           DependencyProperty.Register("PastaDestino", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("StatusOdds", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public string PastaEuroFonte 
        {
            get { return (string)this.GetValue(PastaEuroFonteProperty); }
            set { this.SetValue(PastaEuroFonteProperty, value); }
        }

        public string PastaCopaFonte
        {
            get { return (string)this.GetValue(PastaCopaFonteProperty); }
            set { this.SetValue(PastaCopaFonteProperty, value); }
        }

        public string PastaPremierFonte
        {
            get { return (string)this.GetValue(PastaPremierFonteProperty); }
            set { this.SetValue(PastaPremierFonteProperty, value); }
        }

        public string PastaSuperFonte
        {
            get { return (string)this.GetValue(PastaSuperFonteProperty); }
            set { this.SetValue(PastaSuperFonteProperty, value); }
        }

        public string PastaDestino 
        {
            get { return (string) this.GetValue(PastaDestinoProperty); }
            set { this.SetValue(PastaDestinoProperty, value); }
        }


        public string StatusOdds
        {
            get { return (string)this.GetValue(StatusProperty); }
            set { this.SetValue(StatusProperty, value); }
        }

        // **************************************** RESULTADOS *********************************************************************
        public static readonly DependencyProperty StatusResultadosProperty =
            DependencyProperty.Register("StatusResultados", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PastaEuroResultadosFonteProperty =
    DependencyProperty.Register("PastaEuroResultadosFonte", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PastaCopaResultadosFonteProperty =
            DependencyProperty.Register("PastaCopaResultadosFonte", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PastaPremierResultadosFonteProperty =
            DependencyProperty.Register("PastaPremierResultadosFonte", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PastaSuperResultadosFonteProperty =
            DependencyProperty.Register("PastaSuperResultadosFonte", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PastaDestinoResultadosProperty =
           DependencyProperty.Register("PastaDestinoResultados", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));



        public string PastaEuroResultadosFonte
        {
            get { return (string)this.GetValue(PastaEuroResultadosFonteProperty); }
            set { this.SetValue(PastaEuroResultadosFonteProperty, value); }
        }

        public string PastaCopaResultadosFonte
        {
            get { return (string)this.GetValue(PastaCopaResultadosFonteProperty); }
            set { this.SetValue(PastaCopaResultadosFonteProperty, value); }
        }

        public string PastaPremierResultadosFonte
        {
            get { return (string)this.GetValue(PastaPremierResultadosFonteProperty); }
            set { this.SetValue(PastaPremierResultadosFonteProperty, value); }
        }

        public string PastaSuperResultadosFonte
        {
            get { return (string)this.GetValue(PastaSuperResultadosFonteProperty); }
            set { this.SetValue(PastaSuperResultadosFonteProperty, value); }
        }

        public string PastaDestinoResultados
        {
            get { return (string)this.GetValue(PastaDestinoResultadosProperty); }
            set { this.SetValue(PastaDestinoResultadosProperty, value); }
        }

        public string StatusResultados
        {
            get { return (string)this.GetValue(StatusResultadosProperty); }
            set { this.SetValue(StatusResultadosProperty, value); }
        }

        // **************************************** ENVIADOR PLANILHA GOOGLE *********************************************************************
        public static readonly DependencyProperty StatusEnviadorGoogleSheetsProperty =
            DependencyProperty.Register("StatusEnviadorGoogleSheets", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public string StatusEnviadorGoogleSheets
        {
            get { return (string)this.GetValue(StatusEnviadorGoogleSheetsProperty); }
            set { this.SetValue(StatusEnviadorGoogleSheetsProperty, value); }
        }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnPastaFonte_Click(object sender, RoutedEventArgs e)
        {

            if (sender is not Button botao) return;

            string? campeonato;
            switch (botao.Name)
            {
                case "btnPastaFonteEuro":
                case "btnPastaFonteResultadosEuro":
                    campeonato = "EURO";
                    break;
                case "btnPastaFonteCopa":
                case "btnPastaFonteResultadosCopa":
                    campeonato = "COPA";
                    break;
                case "btnPastaFontePremier":
                case "btnPastaFonteResultadosPremier":
                    campeonato = "PREMIER";
                    break;
                case "btnPastaFonteSuper":
                case "btnPastaFonteResultadosSuper":
                    campeonato = "SUPER";
                    break;
                default:
                    return;
            }

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Escolha uma pasta para " + campeonato;
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

                switch (botao.Name)
                {
                    case "btnPastaFonteEuro":
                        this.PastaEuroFonte = ExtrairPasta(filename);
                        break;
                    case "btnPastaFonteCopa":
                        this.PastaCopaFonte = ExtrairPasta(filename);
                        break;
                    case "btnPastaFontePremier":
                        this.PastaPremierFonte = ExtrairPasta(filename);
                        break;
                    case "btnPastaFonteSuper":
                        this.PastaSuperFonte = ExtrairPasta(filename);
                        break;

                    case "btnPastaFonteResultadosEuro":
                        this.PastaEuroResultadosFonte = ExtrairPasta(filename);
                        break;
                    case "btnPastaFonteResultadosCopa":
                        this.PastaCopaResultadosFonte = ExtrairPasta(filename);
                        break;
                    case "btnPastaFonteResultadosPremier":
                        this.PastaPremierResultadosFonte = ExtrairPasta(filename);
                        break;
                    case "btnPastaFonteResultadosSuper":
                        this.PastaSuperResultadosFonte = ExtrairPasta(filename);
                        break;
                    default:
                        return;
                }

            }
        }


        private void BtnPastaDestino_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button botao) return;

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

                if(botao.Name == "btnPastaResultadosDestino")
                {
                    PastaDestinoResultados = ExtrairPasta(filename);
                    return;
                }

                if(botao.Name == "btnPastaDestino")
                {
                    this.PastaDestino = ExtrairPasta(filename);
                    return;
                }

                throw new Exception("Não foi possível salvar a pasta destino");
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

        private async Task ProcessarOdds(CancellationToken cancelToken)
        {
            IProgress<string> informarParada = new Progress<string>(ParouDaemonOdds);
            
            IProgress<string> progress = new Progress<string>(InformarStatusOdds);
            var progress1 = new Progress<string>(InformarStatusOdds);
            var progress2 = new Progress<string>(InformarStatusOdds);
            var progress3 = new Progress<string>(InformarStatusOdds);

            var oddProcessorCopa = new OddListProcessor("COPA");
            var oddProcessorEuro = new OddListProcessor("EURO");
            var oddProcessorSuper = new OddListProcessor("SUPER");
            var oddProcessorPremier = new OddListProcessor("PREMIER");

            var config = new CSVReaderWriterWorkerConfig
                (
                    PastaDestino,
                    PastaCopaFonte,
                    PastaEuroFonte,
                    PastaPremierFonte,
                    PastaSuperFonte,
                    informarParada,
                    progress,
                    progress1,
                    progress2,
                    progress3,
                    oddProcessorCopa,
                    oddProcessorEuro,
                    oddProcessorPremier,
                    oddProcessorSuper,
                    50,
                    cancelToken
                );

            _começouLeituraOdds = true;
            InformarStatusOdds("Lendo...");
            try
            {
                await Task.Factory.StartNew(CSVReaderWriterWorker.GetWorker(config), TaskCreationOptions.LongRunning);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                ParouDaemonOdds("");
                InformarStatusOdds("Parado por causa de exception.");
            } 
        }

        private async Task ProcessarResultados(CancellationToken cancelToken)
        {
            IProgress<string> informarParada = new Progress<string>(ParouDaemonResultados);

            IProgress<string> progress = new Progress<string>(InformarStatusResultados);
            var progress1 = new Progress<string>(InformarStatusResultados);
            var progress2 = new Progress<string>(InformarStatusResultados);
            var progress3 = new Progress<string>(InformarStatusResultados);

            var resultadoProcessorCopa = new ResultadoListProcessor("COPA");
            var resultadoProcessorEuro = new ResultadoListProcessor("EURO");
            var resultadoProcessorSuper = new ResultadoListProcessor("SUPER");
            var resultadoProcessorPremier = new ResultadoListProcessor("PREMIER");

            var config = new CSVReaderWriterWorkerConfig
                (
                    PastaDestinoResultados,
                    PastaCopaResultadosFonte,
                    PastaEuroResultadosFonte,
                    PastaPremierResultadosFonte,
                    PastaSuperResultadosFonte,
                    informarParada,
                    progress,
                    progress1,
                    progress2,
                    progress3,
                    resultadoProcessorCopa,
                    resultadoProcessorEuro,
                    resultadoProcessorPremier,
                    resultadoProcessorSuper,
                    3,
                    cancelToken,
                    new Progress<string>(InformarStatusEnviadorGoogle)
                );

            _começouLeituraResultados = true;
            InformarStatusResultados("Lendo...");
            try
            {
                await Task.Factory.StartNew(CSVReaderWriterWorker.GetWorker(config), TaskCreationOptions.LongRunning);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                ParouDaemonResultados("");
                InformarStatusOdds("Parado por causa de exception.");
            }
        }

        private void ParouDaemonOdds(string info)
        {
            _começouLeituraOdds = false;

            btnStop.IsEnabled = false;
            btnStart.IsEnabled = true;
            InformarStatusOdds("Parado.");
        }


        private void ParouDaemonResultados(string info)
        {
            _começouLeituraResultados = false;

            btnStopResultados.IsEnabled = false;
            btnStartResultados.IsEnabled = true;
            InformarStatusResultados("Parado.");
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            InformarStatusOdds("Parada Solicitada.... Tentando parar...");
            cancelSourceOdds.Cancel();
        }

        private void BtnStopResultados_Click(object sender, RoutedEventArgs e)
        {
            InformarStatusResultados("Parada Solicitada.... Tentando parar...");
            cancelSourceResultados.Cancel();
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Tentando Iniciar, _começouLeitura é: " + _começouLeituraOdds.ToString());
            if (_começouLeituraOdds) return;

            btnStop.IsEnabled = true;
            btnStart.IsEnabled = false;

            cancelSourceOdds.Dispose();
            cancelSourceOdds = new CancellationTokenSource();
            var token = cancelSourceOdds.Token;

            try
            {
                Debug.WriteLine("Vai Começar...");
                await ProcessarOdds(token);
                Debug.WriteLine("Terminou");
            } catch (Exception ex)
            {
                Debug.WriteLine("Exception");
                Debug.WriteLine(ex);
            }
        }

        private void InformarStatusOdds(string status)
        {
            this.StatusOdds = status;
        }

        private void InformarStatusEnviadorGoogle(string status)
        {
            this.StatusEnviadorGoogleSheets = status;
        }

        private void InformarStatusResultados(string status)
        {
            this.StatusResultados = status;
        }

        private async void btnStartResultados_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Tentando Iniciar, _começouLeitura é: " + _começouLeituraResultados.ToString());
            if (_começouLeituraResultados) return;

            btnStopResultados.IsEnabled = true;
            btnStartResultados.IsEnabled = false;

            cancelSourceResultados.Dispose();
            cancelSourceResultados = new CancellationTokenSource();
            var token = cancelSourceResultados.Token;

            try
            {
                Debug.WriteLine("Vai Começar...");
                await ProcessarResultados(token);
                Debug.WriteLine("Terminou");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception");
                Debug.WriteLine(ex);
            }
        }

        //private void BtnTesteAppsScripts_Click(object sender, RoutedEventArgs e)
        //{
         //   var enviadorPlanilhaService = new EnviadorPlanilhaService();
         //   enviadorPlanilhaService.EnviarDadosResultadosAsync(null).Wait();
        //}
    }
}
