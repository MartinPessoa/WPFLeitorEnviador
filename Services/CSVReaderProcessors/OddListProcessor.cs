using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WPFLeitorEnviador.Domain;

namespace WPFLeitorEnviador.Services
{

        internal class OddListProcessor
        {
            private List<OddItemProcessor> _lista = new();
            private string _campeonato = "";

            public OddListProcessor(string campeonato)
            {
                this._campeonato=campeonato;
            }

            internal List<Odd> GetOdds()
            {
                List<Odd> odds = new();
                foreach(var odd in this._lista)
                {
                    if (odd == null) continue;

                    var nova = odd.GetItem();

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
                OddItemProcessor tempOddDaVez = EncontrarTempOdd(n);

                // se tem hora mas não tem odd...
                // ... e ainda por cima estamos tentando adicionar odd...
                // tem algo errado, pq no arquivo vem primeiro odd depois hora.
                // criamos uma nova temp, só com a odd, pq a hora certa deve estar chegando na leitura....
                if(
                    tempOddDaVez.Hora != null &&
                    (tempOddDaVez.Over15 == null || tempOddDaVez.Over25 == null) &&
                    OddItemProcessor.EhInformacaoDeOdd(fields)
                    )
                {
                    tempOddDaVez = new OddItemProcessor();
                    tempOddDaVez.Campeonato = _campeonato;
                    tempOddDaVez.UltimoValor = n;
                    _lista.Add(tempOddDaVez);
                }


                //processar linha
                tempOddDaVez.ProcessarLinha(fields, n, data);
            }

            private OddItemProcessor EncontrarTempOdd(int n)
            {
                if (this._lista.Count < 1)
                {
                    Debug.Write("*********** || PRIMEIRO TEMP ACCUMULATTOR || ***************");
                    var novo = new OddItemProcessor();
                    novo.Campeonato = this._campeonato;
                    novo.UltimoValor = n;
                    _lista.Add(novo);
                    return novo;
                }

                    var find = this._lista.Find(x => x.UltimoValor == n + 1);

                    if (find == null)
                    {
                        Debug.Write(" >>> Não encontrou # que é igual a " + (n + 1) + " criando novo <<<");
                        find = new OddItemProcessor();
                        find.Campeonato = _campeonato;
                        find.UltimoValor = n;
                        _lista.Add(find);

                        return find;
                    }

                    if (find.EstaCompleto)
                    {
                        Debug.Write(" >>> Encontrou mas está completo " + (find.ToString()) + " criando novo, #" + n + " <<<");
                        find = new OddItemProcessor();
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
    }
