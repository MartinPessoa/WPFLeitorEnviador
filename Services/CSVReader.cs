using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFLeitorEnviador.Domain;
using WPFLeitorEnviador.Services.DTO;

namespace WPFLeitorEnviador.Services
{
    internal class CSVReader
    {
        private readonly string _pasta;
        private readonly CSVWriter _csvWriter;
        private readonly string _CSVSearchPattern = "*.csv";

        private Action<string>? _informarResultado;
        
        private TemporaryOddInfoProcessor _oddProcessor;
        private string _campeonato;
        public CSVReader(string pasta, CSVWriter csvwriter, string campeonato, Action<string> progress)
        {
            this._pasta = pasta;    
            this._csvWriter = csvwriter;
            this._informarResultado = progress;
            this._campeonato = campeonato;

            _oddProcessor = new TemporaryOddInfoProcessor(this._campeonato);
        }

        ~CSVReader()
        {
            _informarResultado = null;
        }

        private string[] GetListaArquivos(string PastaOrigem)
        {
            return Directory.GetFiles(PastaOrigem, this._CSVSearchPattern);
        }

        public async Task<List<Odd>?> Read()
        {
            //Encontrar Arquivo
            var filename = GetListaArquivos(this._pasta);

            Debug.WriteLine("**************** ENTROU EM READ **********************");
            Debug.WriteLine("Lendo da Pasta: " + this._pasta);
            Debug.WriteLine("Primeiro Arquivo Encontrado: " + String.Join(" - ", filename));

            if ( !GuardTemPastaEArquivos(filename) ) return null;

            InformarProgresso("Encontramos "+ filename.Length.ToString() + " arquivos. Começando a Leitura...");
            //Ler
            await this.Ler(filename[0]);

            InformarProgresso("Leitura Finalizada.");
            //string lido = "";

            return GerarOdds();
        }

        private List<Odd> GerarOdds()
        {
            return _oddProcessor.GetOdds();
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

                        _oddProcessor.ProcessarLinha(fields[1].Split(" "));


                        Debug.WriteLine(fields[1]);
                    }
      
                }
            });
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

            _informarResultado.Invoke(Mensagem);
        }

        private class TemporaryOddInfoProcessor
        {
            private List<TemporaryOddInfoAccumulattor> _lista = new();
            private string _campeonato = "";

            public TemporaryOddInfoProcessor(string campeonato)
            {
                this._campeonato=campeonato;
            }

            internal List<Odd> GetOdds()
            {
                List<Odd> odds = new();
                foreach(var odd in this._lista)
                {
                    if (odd.EstaCompleto)
                    {
                        odds.Add(odd.GetOdd());
                    }
                }

                return odds;
            }

            internal void ProcessarLinha(string[] fields)
            {
                Debug.Write(" -> linha recebida: " + String.Join(" ",fields));
                //ler numero #
                int n = PegarInt(fields[0]);

                if(n<0) { Debug.Write(" || int menor que zero: "); return; }

                //encontrar elemento com #-1
                TemporaryOddInfoAccumulattor tempOddDaVez = EncontrarTempOdd(n);

                //processar linha
                tempOddDaVez.ProcessarLinha(fields, n);
            }

            private TemporaryOddInfoAccumulattor EncontrarTempOdd(int n)
            {
                if (this._lista.Count < 1)
                {
                    Debug.Write("*********** || PRIMEIRO TEMP ACCUMULATTOR || ***************");
                    var novo = new TemporaryOddInfoAccumulattor();
                    novo.Campeonato = this._campeonato;
                    novo.UltimoValor = n;
                    _lista.Add(novo);
                    return novo;
                }

                    var find = this._lista.Find(x => x.UltimoValor == n + 1);

                    if (find == null)
                    {
                        Debug.Write(" >>> Não encontrou # que é igual a " + (n + 1) + " criando novo <<<");
                        find = new TemporaryOddInfoAccumulattor();
                        find.Campeonato = _campeonato;
                        find.UltimoValor = n;
                        _lista.Add(find);

                        return find;
                    }

                    if (find.EstaCompleto)
                    {
                        Debug.Write(" >>> Encontrou mas está completo " + (find.ToString()) + " criando novo, #" + n + " <<<");
                        find = new TemporaryOddInfoAccumulattor();
                        find.Campeonato = _campeonato;
                        find.UltimoValor = n;
                        _lista.Add(find);
                    }

                    return find;
                
            }

            private int PegarInt(string v)
            {
                if ( int.TryParse(v.Substring(1), out int valor) )
                {
                    return valor;
                }

                return -1;
            }
        }

        private class TemporaryOddInfoAccumulattor
        {
            public enum TempOddInfoCampo
            {
                campeonato,
                horaminuto,
                overs
            }

            public string? Campeonato { get; set; }

            public string? Hora { get; set;  }

            public string? Minuto { get; set; }
            public string? Over15 { get; set; }
            public string? Over25 { get; set; }

            public int UltimoValor { get; set; }

            public bool ProcessarLinha(string[] linhaSplitada, int hashNLinha) 
            {
                Debug.Write(" || PROCESSAR LINHA || ");
                if (this.UltimoValor != hashNLinha + 1)
                {
                    //throw new Exception("Erro: tentando processar linha mas não é o próximo da linha!");
                    return false;
                }

                Debug.Write(" * Ultimo Valor antes: " + UltimoValor);

                this.UltimoValor = hashNLinha;

                Debug.Write(" * Ultimo Valor depois: " + UltimoValor);

                if (linhaSplitada[1] != "Extrair")
                {

                    Debug.Write(" * linhaSplitada[1]: " + linhaSplitada[1] + " SAINDO * || <- \n");
                    return true;
                }



                    //hora e minuto
                    if (linhaSplitada[3].Contains(':'))
                {
                    Debug.Write(" * Vou Anotar minuto e hora: " + linhaSplitada[3] + "EU SOU: " + ToString());
                    return AnotarMinutoEHora(linhaSplitada[3]);
                }

                //odd
                if (linhaSplitada[3].Contains('.'))
                {
                    Debug.Write(" * Vou Anotar alguma odd: " + linhaSplitada[3] + "EU SOU: " + ToString());
                    return AnotarOdd(linhaSplitada[3]);
                }

                Debug.Write(" Chegou até o final???? Eu sou: " + ToString());

                //mesmo que não adicionamos as informações,
                // mas processamos a linha. Então retornamos true.
                return true;
            }

            public override string ToString()
            {
                return $"OBJECTO TEMPORARIO: hora: {Hora}, minuto: {Minuto}, campeonato: {Campeonato}, odd15: {Over15}, over25: {Over25}";
            }

            private bool AnotarMinutoEHora(string minutoEHora)
            {
                var splitado = minutoEHora.Split(':');

                if (Hora != null || Minuto != null)
                {
                    Debug.WriteLine("Tentando sobreescrever Hora ou Minuto");
                    return false;
                }

                Hora = splitado[0];
                Minuto = splitado[1];

                return true;
            }

            private bool AnotarOdd(string odd)
            {
                
                if (double.TryParse(odd, System.Globalization.NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"),out double teste))
                {
                    //Debug.WriteLine(teste + "é maior que 2? " + (teste > 2));
                    if (teste >= 2)
                    {
                        if(Over25 != null) 
                        {
                            Debug.WriteLine("Tentando sobreescrever Over25");
                            return false;
                        }
                        Over25 = odd;
                    }
                    else
                    {
                        if (Over15 != null) 
                        {
                            Debug.WriteLine("Tentando sobreescrever Over15");
                            return false; 
                        }
                        Over15 = odd;
                    }

                    return true;
                }
                return false;
                //throw new Exception("Erro: Não foi possível converter um provavél valor de odd em double: " + odd);
            }

            public bool EstaCompleto { 
                get
                {
                    return (
                        Campeonato != null &&
                        Hora != null &&
                        Minuto != null &&
                        Over15 != null &&
                        Over25 != null);
                } 
            }

            public Odd? GetOdd()
            {
                if(!EstaCompleto)
                {
                    return null;
                }

                return new Odd(Campeonato, Hora, Minuto, Over15, Over25);

            }

        }
    }


}
