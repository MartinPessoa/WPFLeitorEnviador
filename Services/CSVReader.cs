﻿using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WPFLeitorEnviador.Services.DTO;

namespace WPFLeitorEnviador.Services
{
    internal class CSVReader
    {
        private readonly string _pasta;
        private readonly string _CSVSearchPattern = "*.csv";

        private IProgress<string>? _informarResultado;
        
        public IListProcessor Processor { get; private set; }

        public string Campeonato { get; private set; }
        public CSVReader(string pasta, string campeonato, IListProcessor processor, IProgress<string> progress)
        {
            this._pasta = pasta;    
            this._informarResultado = progress;
            this.Campeonato = campeonato;

            Processor = processor;
        }

        ~CSVReader()
        {
            _informarResultado = null;
        }

        private string[] GetListaArquivos(string PastaOrigem)
        {
            return Directory.GetFiles(PastaOrigem, this._CSVSearchPattern, System.IO.SearchOption.TopDirectoryOnly);
        }

        public bool Read()
        {
            //Encontrar Arquivo
            var filename = GetListaArquivos(this._pasta);
            var qtdArquivos = filename.Length;
            var arquivoAtual = 0;

            Debug.WriteLine("**************** ENTROU EM READ **********************");
            Debug.WriteLine("Lendo da Pasta: " + this._pasta);
            Debug.WriteLine("Primeiro Arquivo Encontrado: " + String.Join(" - ", filename));

            if ( !GuardTemPastaEArquivos(filename) ) return false;

            InformarProgresso("Encontramos "+ qtdArquivos.ToString() + " arquivo(s). Começando a Leitura...");
            try
            {
                //Ler
                foreach (string arquivo in filename)
                {
                    if (!EhDesteCampeonato(arquivo)) continue;
                    arquivoAtual++;
                    InformarProgresso("Lendo arquivo " + arquivoAtual.ToString() + " de " + qtdArquivos.ToString() + " arquivo(s).");
                    this.Ler(arquivo).Wait();
                    //this._oddsGerando.AddRange(GerarItens());
                    Processor.AcumularEZerar();
                }
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
    
            

            InformarProgresso("Leitura da Pasta Finalizada.");
            //string lido = "";

            return true;
        }

        private bool EhDesteCampeonato(string arquivo)
        {
            string strComparar = "";

            Debug.WriteLine(" @@ Vamos verificar nome de arquivo: " + arquivo);

            switch (Campeonato)
            {
                case "EURO":
                    strComparar += "Euro";
                    break;
                case "COPA":
                    strComparar += "Copa";
                    break;
                case "SUPER":
                    strComparar += "Super";
                    break;
                case "PREMIER":
                    strComparar += "Premier";
                    break;

                default:
                    Debug.WriteLine(" @@ Saindo porque case default.");
                    return false;
            }

            if (arquivo.Split("\\").Last().Contains(strComparar))
            {
                Debug.WriteLine(" @@ Match com: " + strComparar);
                return true;
            }

            Debug.WriteLine(" @@ Não deu match com: " + strComparar);
            return false;
        }

        /*private List<Odd> GerarItens()
        {
            return _oddProcessor.GetOdds();
        }*/

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

                        Debug.WriteLine(" -> Data: " + fields[0] + "\n");

                        //conferir data com ultima data lida
                        var dataNãoFoiLida = await CompararComÚltimaDataLida(data);

                        if (!dataNãoFoiLida)
                        {
                            return;
                        }

                        Processor.ProcessarLinha(fields[1].Split(" "), data);


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
                File.Move(filename, destinoComArquivo,true);
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        } 

        private async Task<bool> CompararComÚltimaDataLida(DateTime data)
        {
            // TODO: Comparar com ultima data gravada....
            return await Task.Run(() => { return true; });
        }

        private void InformarProgresso(string Mensagem)
        {
            if(this._informarResultado == null)
            {
                return ;
            }
            Debug.WriteLine("Informando: " + Mensagem);
            _informarResultado.Report($"Leitor {Campeonato}: {Mensagem}");
        }
    }


}
