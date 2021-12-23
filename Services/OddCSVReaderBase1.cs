using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WPFLeitorEnviador.Domain;
using WPFLeitorEnviador.Services.DTO;

namespace WPFLeitorEnviador.Services
{
    internal class OddCSVReaderBase1
    {
        protected readonly string _pasta;
        protected readonly string _CSVSearchPattern = "*.csv";

        protected IProgress<string>? _informarResultado;

        protected IGenericEntityListProcessor _oddProccessor;
        protected List<IEntityBase> _itensGerando = new List<IEntityBase>();
        public string Campeonato { get; private set; }

        protected OddCSVReaderBase1(string pasta, string campeonato, IGenericEntityListProcessor processor, IProgress<string>? progress = null)
        {
            _pasta = pasta;
            Campeonato = campeonato;
            _informarResultado = progress;
            _oddProccessor = processor;
        }

        protected virtual List<IEntityBase> GerarItens()
        {
            return _oddProccessor.GetItems();
        }

        public List<IEntityBase> Read()
        {
            //Encontrar Arquivo
            var filename = GetListaArquivos(this._pasta);
            var qtdArquivos = filename.Length;
            var arquivoAtual = 0;

            Debug.WriteLine("**************** ENTROU EM READ **********************");
            Debug.WriteLine("Lendo da Pasta: " + this._pasta);
            Debug.WriteLine("Primeiro Arquivo Encontrado: " + String.Join(" - ", filename));

            if (!GuardTemPastaEArquivos(filename)) return null;

            InformarProgresso("Encontramos " + qtdArquivos.ToString() + " arquivo(s). Começando a Leitura...");
            try
            {
                //Ler
                foreach (string arquivo in filename)
                {
                    arquivoAtual++;
                    InformarProgresso("Lendo arquivo " + arquivoAtual.ToString() + " de " + qtdArquivos.ToString() + " arquivo(s).");
                    this.Ler(arquivo).Wait();
                    this._itensGerando.AddRange(GerarItens());
                    _oddProccessor = new OddListProcessor(this.Campeonato);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }



            InformarProgresso("Leitura da Pasta Finalizada.");
            //string lido = "";

            return _itensGerando.Distinct().OrderBy(o => o.Data).ToList();
        }

        private async Task<bool> CompararComÚltimaDataLida(DateTime data)
        {
            // TODO: Comparar com ultima data gravada....
            return await Task.Run(() => { return true; });
        }

        private string[] GetListaArquivos(string PastaOrigem)
        {
            return Directory.GetFiles(PastaOrigem, this._CSVSearchPattern, System.IO.SearchOption.TopDirectoryOnly);
        }

        private bool GuardTemPastaEArquivos(string[] listaArquivos)
        {
            if (listaArquivos == null)
            {
                InformarProgresso("Pasta não encontrada.");
                return false;
            }

            if (listaArquivos.Length < 1)
            {
                InformarProgresso("Nenhum arquivo encontrado na pasta " + this._pasta);
                return false;
            }

            return true;
        }

        private void InformarProgresso(string Mensagem)
        {
            if (this._informarResultado == null)
            {
                return;
            }
            Debug.WriteLine("Informando: " + Mensagem);
            _informarResultado.Report($"Leitor {Campeonato}: {Mensagem}");
        }

        private async Task Ler(string filename)
        {
            await Task.Run(async () =>
            {
                using (TextFieldParser parser = new TextFieldParser(filename))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();

                        if (fields == null)
                        {
                            continue;
                        }

                        // pegar a data
                        var data = DateTimeConverterModel.Converter(fields[0]);

                        Debug.Write(" -> Data: " + fields[0]);

                        //conferir data com ultima data lida
                        var dataNãoFoiLida = await CompararComÚltimaDataLida(data);

                        if (!dataNãoFoiLida)
                        {
                            return;
                        }

                        _oddProccessor.ProcessarLinha(fields[1].Split(" "), data);


                        Debug.WriteLine(fields[1]);
                    }

                }
            });
            try
            {
                var data = DateTime.Now;

                var textoData = $"{ data.Year}{ data.Month}{data.Day}-{data.Hour}{data.Minute}{data.Second}";

                var destinoSemArquivo = $"{_pasta}\\ANTIGOS\\{Campeonato}\\";
                var destinoComArquivo = $"{ destinoSemArquivo}\\LIDO_EM_{textoData}.csv";
                if (!File.Exists(destinoSemArquivo))
                {
                    DirectoryInfo di = Directory.CreateDirectory(destinoSemArquivo);
                }


                //mover o arquivo lido pra pasta backup
                File.Move(filename, destinoComArquivo, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }
    }
}