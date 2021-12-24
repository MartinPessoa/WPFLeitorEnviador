﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using WPFLeitorEnviador.Domain;
using WPFLeitorEnviador.Services;

namespace WPFLeitorEnviador
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _começouLeitura = false;
        //private readonly List<CSVReader> _cSVReaders = new();
        private bool stopPressed = false;

        private CancellationTokenSource cancelSource = new();


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
            DependencyProperty.Register("Status", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty StatusResultadosProperty =
            DependencyProperty.Register("StatusResultados", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

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


        public string Status
        {
            get { return (string)this.GetValue(StatusProperty); }
            set { this.SetValue(StatusProperty, value); }
        }

        public string StatusResultados
        {
            get { return (string)this.GetValue(StatusResultadosProperty); }
            set { this.SetValue(StatusResultadosProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnPastaFonte_Click(object sender, RoutedEventArgs e)
        {
            var botao = sender as Button;

            if(botao == null)  return;

            var campeonato = "";

            switch (botao.Name)
            {
                case "btnPastaFonteEuro":
                    campeonato = "EURO";
                    break;
                case "btnPastaFonteCopa":
                    campeonato = "COPA";
                    break;
                case "btnPastaFontePremier":
                    campeonato = "PREMIER";
                    break;
                case "btnPastaFonteSuper":
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

                switch(campeonato)
                {
                    case "EURO":
                        this.PastaEuroFonte = ExtrairPasta(filename);
                        break;
                    case "COPA":
                        this.PastaCopaFonte = ExtrairPasta(filename);
                        break;
                    case "PREMIER":
                        this.PastaPremierFonte = ExtrairPasta(filename);
                        break;
                    case "SUPER":
                        this.PastaSuperFonte = ExtrairPasta(filename);
                        break;
                    default:
                        return;
                }
            }
        }


        private void BtnPastaDestino_Click(object sender, RoutedEventArgs e)
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

        private async Task ProcessarOdds(string pastaDestino, string pastaCopa, string pastaEuro, string pastaPremier, string pastaSuper,Action<string> CallbackStatus, CancellationToken cancelToken)
        {
            IProgress<string> informarParada = new Progress<string>(ParouDaemonOdds);
            
            IProgress<string> progress = new Progress<string>(CallbackStatus);
            var progress1 = new Progress<string>(CallbackStatus);
            var progress2 = new Progress<string>(CallbackStatus);
            var progress3 = new Progress<string>(CallbackStatus);

            var oddProcessorCopa = new OddListProcessor("COPA");
            var oddProcessorEuro = new OddListProcessor("EURO");
            var oddProcessorSuper = new OddListProcessor("SUPER");
            var oddProcessorPremier = new OddListProcessor("PREMIER");

            _começouLeitura = true;
            InformarStatus("Lendo...");
            try
            {
                await Task.Factory.StartNew(CSVWorker(pastaDestino, pastaCopa, pastaEuro, pastaPremier, pastaSuper, informarParada, progress, progress1, progress2, progress3, oddProcessorCopa, oddProcessorEuro, oddProcessorSuper, oddProcessorPremier, cancelToken), TaskCreationOptions.LongRunning);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                ParouDaemonOdds("");
                InformarStatus("Parado por causa de exception.");
            } 
        }

        private static Func<Task> CSVWorker(string pastaDestino, string pastaCopa, string pastaEuro, string pastaPremier, string pastaSuper, IProgress<string> informarParada, IProgress<string> progressReporterCopa, Progress<string> progressReporterEuro, Progress<string> progressReporterPremier, Progress<string> progressReporterSuper, IListProcessor processorCopa, IListProcessor processorEuro, IListProcessor processorSuper, IListProcessor processorPremier, CancellationToken cancelToken)
        {
            return async () =>
            {
                Debug.WriteLine("Thread funcionando");
                var readers = new List<CSVReader>();
                CSVWriter writer = new(pastaDestino, progressReporterCopa);

                if (pastaCopa != "")
                {
                    readers.Add(new CSVReader(pastaCopa, "COPA", processorCopa, progressReporterCopa));
                }

                if (pastaEuro != "")
                {
                    readers.Add(new CSVReader(pastaEuro, "EURO", processorEuro, progressReporterEuro));
                }


                if (pastaPremier != "")
                {
                    readers.Add(new CSVReader(pastaPremier, "PREMIER", processorPremier, progressReporterPremier));
                }

                if (pastaSuper != "")
                {
                    readers.Add(new CSVReader(pastaSuper, "SUPER", processorSuper, progressReporterSuper));
                }

                while (true)
                {

                    if (cancelToken.IsCancellationRequested)
                    {
                        informarParada.Report("Cancelada Leitura de Odds.");
                        break;
                    }


                    var listaStrings = new List<(string, string)>();
                    foreach (CSVReader reader in readers)
                    {
                        //CallbackStatus("Iniciando " + reader.Campeonato);
                        var leu = reader.Read();
                        if (leu)
                        {
                            //CallbackStatus(reader.Campeonato + " finalizado. Lido " + odds.Count + " odds.");
                            listaStrings.Add((reader.Processor.GetStrings(), reader.Processor.GetCampeonato()));
                        }
                    }

                    Debug.Write("temos " + listaStrings.Count + "listas lidas");

                    foreach (var s in listaStrings)
                    {
                        //Debug.WriteLine("Escrevendo lista com " + odd.Count() + " itens");
                        progressReporterCopa.Report("Escrevendo...");
                        await writer.Write(s.Item1, s.Item2);
                        Thread.Sleep(150);
                    }

                    for (int i = 5; i > 0; i--)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            informarParada.Report("Cancelada Leitura de Odds.");
                            return;
                        }

                        progressReporterCopa.Report($"Escrita finalizada. Aguardando {10 * i} segundos...");
                        Thread.Sleep(1000 * 10);
                    }

                    progressReporterCopa.Report("Reiniciando leitura...");
                }
            };
        }

        private void ParouDaemonOdds(string info)
        {
            _começouLeitura = false;

            btnStop.IsEnabled = false;
            btnStart.IsEnabled = true;
            InformarStatus("Parado.");
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            InformarStatus("Parada Solicitada.... Tentando parar...");
            cancelSource.Cancel();
            this.stopPressed = true;
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Tentando Iniciar, _começouLeitura é: " + _começouLeitura.ToString());
            if (_começouLeitura) return;

            btnStop.IsEnabled = true;
            btnStart.IsEnabled = false;

            cancelSource.Dispose();
            cancelSource = new CancellationTokenSource();
            var token = cancelSource.Token;

            try
            {
                Debug.WriteLine("Vai Começar...");
                await ProcessarOdds(PastaDestino, PastaCopaFonte, PastaEuroFonte, PastaPremierFonte, PastaSuperFonte, InformarStatus, token);
                Debug.WriteLine("Terminou");
            } catch (Exception ex)
            {
                Debug.WriteLine("Exception");
                Debug.WriteLine(ex);
            }
        }

        private void InformarStatus(string status)
        {
            this.Status = status;
        }
    }
}
