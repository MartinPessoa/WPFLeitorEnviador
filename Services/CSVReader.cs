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

        private IProgress<string>?_informarResultado;
        
        private TemporaryOddInfoProcessor _oddProcessor;
        private List<Odd> _oddsGerando = new List<Odd>();
        public string Campeonato { get; private set; }
        public CSVReader(string pasta, CSVWriter csvwriter, string campeonato, IProgress<string> progress = null)
        {
            this._pasta = pasta;    
            this._csvWriter = csvwriter;
            this._informarResultado = progress;
            this.Campeonato = campeonato;

            _oddProcessor = new TemporaryOddInfoProcessor(this.Campeonato);
        }

        ~CSVReader()
        {
            _informarResultado = null;
        }

        private string[] GetListaArquivos(string PastaOrigem)
        {
            return Directory.GetFiles(PastaOrigem, this._CSVSearchPattern, System.IO.SearchOption.TopDirectoryOnly);
        }

        public List<Odd> Read()
        {
            //Encontrar Arquivo
            var filename = GetListaArquivos(this._pasta);
            var qtdArquivos = filename.Length;
            var arquivoAtual = 0;

            Debug.WriteLine("**************** ENTROU EM READ **********************");
            Debug.WriteLine("Lendo da Pasta: " + this._pasta);
            Debug.WriteLine("Primeiro Arquivo Encontrado: " + String.Join(" - ", filename));

            if ( !GuardTemPastaEArquivos(filename) ) return null;

            InformarProgresso("Encontramos "+ qtdArquivos.ToString() + " arquivo(s). Começando a Leitura...");
            try
            {
                //Ler
                foreach (string arquivo in filename)
                {
                    arquivoAtual++;
                    InformarProgresso("Lendo arquivo " + arquivoAtual.ToString() + " de " + qtdArquivos.ToString() + " arquivo(s).");
                    this.Ler(arquivo).Wait();
                    this._oddsGerando.AddRange(GerarOdds());
                    _oddProcessor = new TemporaryOddInfoProcessor(this.Campeonato);
                }
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString()); 
            }
    
            

            InformarProgresso("Leitura da Pasta Finalizada.");
            //string lido = "";

            return _oddsGerando.Distinct().OrderBy(o => o.Data).ToList();
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

                        _oddProcessor.ProcessarLinha(fields[1].Split(" "), data);


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
                    if (odd == null) continue;

                    var nova = odd.GetOdd();

                    if(nova != null)
                    {
                        odds.Add(nova);
                    }
                }

                var retorno = odds.Distinct().OrderBy(o => o.Data).ToList();

                return retorno;
            }

            internal void ProcessarLinha(string[] fields, DateTime data)
            {
                Debug.Write(" -> linha recebida: " + String.Join(" ",fields));
                //ler numero #
                int n = PegarInt(fields[0]);

                if(n<0) { Debug.Write(" || int menor que zero: "); return; }

                //encontrar elemento com #-1
                TemporaryOddInfoAccumulattor tempOddDaVez = EncontrarTempOdd(n);

                // se tem hora mas não tem odd...
                // ... e ainda por cima estamos tentando adicionar odd...
                // tem algo errado, pq no arquivo vem primeiro odd depois hora.
                // criamos uma nova temp, só com a odd, pq a hora certa deve estar chegando na leitura....
                if(
                    tempOddDaVez.Hora != null &&
                    (tempOddDaVez.Over15 == null || tempOddDaVez.Over25 == null) &&
                    TemporaryOddInfoAccumulattor.EhInformacaoDeOdd(fields)
                    )
                {
                    tempOddDaVez = new TemporaryOddInfoAccumulattor();
                    tempOddDaVez.Campeonato = _campeonato;
                    tempOddDaVez.UltimoValor = n;
                    _lista.Add(tempOddDaVez);
                }


                //processar linha
                tempOddDaVez.ProcessarLinha(fields, n, data);
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

            public DateTime DataHora { get; set; }

            public int UltimoValor { get; set; }

            public static bool EhInformaçãoProcessável(string[] linhaSplitada)
            {
                if(linhaSplitada[1] != "Extrair") return false;

                return true;
            }

            public static bool EhInformacaoDeOdd(string[] linhaSplitada)
            {
                if(!EhInformaçãoProcessável(linhaSplitada)) return false;

                if (linhaSplitada[3].Contains('.')) return true;

                return false;
            }

            public static bool EhInformacaoDeHora(string[] linhaSplitada)
            {
                if (!EhInformaçãoProcessável(linhaSplitada)) return false;

                if (linhaSplitada[3].Contains(':')) return true;

                return false;
            }

            public bool ProcessarLinha(string[] linhaSplitada, int hashNLinha, DateTime data) 
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

                //hora e minuto
                if (EhInformacaoDeHora(linhaSplitada))
                {
                    Debug.Write(" * Vou Anotar minuto e hora: " + linhaSplitada[3] + "EU SOU: " + ToString());
                    return AnotarMinutoEHora(linhaSplitada[3], data);
                }

                //odd
                if (EhInformacaoDeOdd(linhaSplitada))
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
                return $"OBJETO TEMPORARIO: hora: {Hora}, minuto: {Minuto}, campeonato: {Campeonato}, odd15: {Over15}, over25: {Over25}";
            }

            private bool AnotarMinutoEHora(string minutoEHora, DateTime data)
            {
                var splitado = minutoEHora.Split(':');

                if (Hora != null || Minuto != null)
                {
                    Debug.WriteLine("Tentando sobreescrever Hora ou Minuto");
                    return false;
                }

                

                Hora = splitado[0];
                Minuto = splitado[1];

                var intHora = int.Parse(Hora);
                var intMinuto = int.Parse(Minuto);

                DataHora = new DateTime(data.Year, data.Month, data.Day, intHora, intMinuto, 0);

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
                        Over25 != null 
                        );
                } 
            }

            public Odd? GetOdd()
            {
                if(!EstaCompleto)
                {
                    return null;
                }

                return new Odd(Campeonato, Over15, Over25, DataHora);

            }

        }
    }


}
