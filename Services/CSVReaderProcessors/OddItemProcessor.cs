using System;
using System.Diagnostics;
using System.Globalization;
using WPFLeitorEnviador.Domain;

namespace WPFLeitorEnviador.Services
{

        internal class OddItemProcessor
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

            public Odd? GetItem()
            {
                if(!EstaCompleto)
                {
                    return null;
                }

                return new Odd(Campeonato, Over15, Over25, DataHora);

            }

        }
    }

