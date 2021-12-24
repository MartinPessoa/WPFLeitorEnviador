using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFLeitorEnviador.Services
{
    public static class CSVReaderWriterWorker
    {
        internal static Func<Task> GetWorker(CSVReaderWriterWorkerConfig config)
        {
            return async () =>
            {
                Debug.WriteLine("Thread funcionando");
                var readers = new List<CSVReader>();
                CSVWriter writer = new(config.pastaDestino, config.progressReporterCopa);

                if (config.pastaCopa != "")
                {
                    readers.Add(new CSVReader(config.pastaCopa, "COPA", config.processorCopa, config.progressReporterCopa));
                }

                if (config.pastaEuro != "")
                {
                    readers.Add(new CSVReader(config.pastaEuro, "EURO", config.processorEuro, config.progressReporterEuro));
                }


                if (config.pastaPremier != "")
                {
                    readers.Add(new CSVReader(config.pastaPremier, "PREMIER", config.processorPremier, config.progressReporterPremier));
                }

                if (config.pastaSuper != "")
                {
                    readers.Add(new CSVReader(config.pastaSuper, "SUPER", config.processorSuper, config.progressReporterSuper));
                }

                while (true)
                {

                    if (config.cancelTok.IsCancellationRequested)
                    {
                        config.informarParada.Report("Cancelada Leitura.");
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
                        config.progressReporterCopa.Report("Escrevendo...");
                        await writer.Write(s.Item1, s.Item2);
                        Thread.Sleep(150);
                    }

                    for (int i = 5; i > 0; i--)
                    {
                        if (config.cancelTok.IsCancellationRequested)
                        {
                            config.informarParada.Report("Cancelada Leitura.");
                            return;
                        }

                        config.progressReporterCopa.Report($"Escrita finalizada. Aguardando {10 * i} segundos...");
                        Thread.Sleep(1000 * 10);
                    }

                    config.progressReporterCopa.Report("Reiniciando leitura...");
                }
            };
        }
    }

    internal class CSVReaderWriterWorkerConfig
    {
        internal string pastaDestino {get; set;}
        internal string pastaCopa {get; set;}
        internal string pastaEuro {get; set;}
        internal string pastaPremier {get; set;}
        internal string pastaSuper {get; set;}
        internal IProgress<string> informarParada {get; set;}
        internal IProgress<string> progressReporterCopa {get; set;}
        internal Progress<string> progressReporterEuro {get; set;}
        internal Progress<string> progressReporterPremier {get; set;}
        internal Progress<string> progressReporterSuper {get; set;}
        internal IListProcessor processorCopa {get; set;}
        internal IListProcessor processorEuro {get; set;}
        internal IListProcessor processorSuper {get; set;}
        internal IListProcessor processorPremier {get; set;}
        internal CancellationToken cancelTok { get; set; }

        public CSVReaderWriterWorkerConfig(string pastaDestino, string pastaCopa, string pastaEuro, string pastaPremier, string pastaSuper, IProgress<string> informarParada, IProgress<string> progressReporterCopa, Progress<string> progressReporterEuro, Progress<string> progressReporterPremier, Progress<string> progressReporterSuper, IListProcessor processorCopa, IListProcessor processorEuro, IListProcessor processorSuper, IListProcessor processorPremier, CancellationToken cancelTok)
        {
            this.pastaDestino = pastaDestino ?? throw new ArgumentNullException(nameof(pastaDestino));
            this.pastaCopa = pastaCopa ?? throw new ArgumentNullException(nameof(pastaCopa));
            this.pastaEuro = pastaEuro ?? throw new ArgumentNullException(nameof(pastaEuro));
            this.pastaPremier = pastaPremier ?? throw new ArgumentNullException(nameof(pastaPremier));
            this.pastaSuper = pastaSuper ?? throw new ArgumentNullException(nameof(pastaSuper));
            this.informarParada = informarParada ?? throw new ArgumentNullException(nameof(informarParada));
            this.progressReporterCopa = progressReporterCopa ?? throw new ArgumentNullException(nameof(progressReporterCopa));
            this.progressReporterEuro = progressReporterEuro ?? throw new ArgumentNullException(nameof(progressReporterEuro));
            this.progressReporterPremier = progressReporterPremier ?? throw new ArgumentNullException(nameof(progressReporterPremier));
            this.progressReporterSuper = progressReporterSuper ?? throw new ArgumentNullException(nameof(progressReporterSuper));
            this.processorCopa = processorCopa ?? throw new ArgumentNullException(nameof(processorCopa));
            this.processorEuro = processorEuro ?? throw new ArgumentNullException(nameof(processorEuro));
            this.processorSuper = processorSuper ?? throw new ArgumentNullException(nameof(processorSuper));
            this.processorPremier = processorPremier ?? throw new ArgumentNullException(nameof(processorPremier));
            this.cancelTok = cancelTok;
        }
    }
}
