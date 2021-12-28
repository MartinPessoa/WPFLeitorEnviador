using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFLeitorEnviador.Domain;

namespace WPFLeitorEnviador.Services.CSVReaderProcessors
{
    internal class ResultadoItemProcessor : BaseItemProcessor
    {
        
        public int? GolsCasa { get; private set; }

        public int? GolsVisitante { get; private set; }

        public new static bool EhInformaçãoProcessável(string[] linhaSplitada)
        {
            if (!BaseItemProcessor.EhInformaçãoProcessável(linhaSplitada))
            {
                Debug.WriteLine("Saiu porque base não é processável");
                return false;
            }

            Debug.WriteLine("LinhaSplitada = " + string.Join(" ", linhaSplitada));

            if (linhaSplitada.Where(x => x == "Failed.").FirstOrDefault() != null) return false;

            return true;
        }

        public bool ProcessarLinha(string[] linhaSplitada, int hashNLinha, DateTime data)
        {
            if (!ResultadoItemProcessor.EhInformaçãoProcessável(linhaSplitada)) return false;
            // Observação: não vai dar pra processar linha splitada, eu acho.
            // Pois os nomes dos times podem ter mais de uma palavra. Acho melhor ir por Substrings...
            // OBS2: até o "3" deve servir, que é o nome do campeonato

            // OBS3: vamos tentar splitar a string no "-". Horário vai estar no [1], com ou sem 
            //outro texto ("sujeira", veja também OBS4)



            //   0     1       2     3   4  5       6       7     8 
            // "#13 Extrair Texto: Euro Cup - 12.08Bélgica1 - 0País de Gales"

            //   0     1       2     3         4   5   6     7         8    9
            //"#13 Extrair Texto: Copa do Mundo - 16.01Argentina0 - 3Brasil"

            //   0     1       2     3         4        5             6     
            //"#13 Extrair Texto: Premier League - 16.03Everton3 - 1Liverpool"

            //   0     1       2     3   4  5       6       7     8 
            // "#12 Extrair Texto: Superleague - 16.07Milan2 - 1Celtic"

            // OBS4: Caso extra, 5+ gols:
            // "#15 Extrair Texto: Euro Cup - 12.35 - undefined"
            Debug.WriteLine("linhaSplitada[0] É: " + linhaSplitada[0] + " e count é: " + linhaSplitada.Count().ToString());
            if(linhaSplitada.Count() < 4) return false;

            var campeonato = "";
            Debug.WriteLine("Vamos testar linhaSplitada[3] É: " + linhaSplitada[3] );
            switch (linhaSplitada[3])
            {
                case "Euro":
                    campeonato = "EURO";
                    break;
                case "Copa":
                    campeonato = "COPA";
                    break;
                case "Premier":
                    campeonato = "PREMIER";
                    break;
                case "Superleague":
                    campeonato = "SUPER";
                    break;
                default:
                    return false;
            }

            this.Campeonato = campeonato;

            var linhaJuntada = string.Join(" ", linhaSplitada);

            // primeiro encontramos o horário:
            var linhaSplitadaEmTraço = linhaJuntada.Split('-');
            var stringSujaHoraETalvezGolsCasa = linhaSplitadaEmTraço[1].Trim();
            Debug.WriteLine("******* stringSujaHora = " + stringSujaHoraETalvezGolsCasa);
            if (stringSujaHoraETalvezGolsCasa.Count() < 5) return false;

            AnotarMinutoEHora(stringSujaHoraETalvezGolsCasa[..5], data, '.');

            if(linhaSplitadaEmTraço[2] == "undefined")
            {
                Debug.WriteLine("******* Era undefined, 5x-1");
                this.GolsCasa = 5;
                this.GolsVisitante = -1;
                return true;
            }

            // se chegou aqui, é porque não é o caso da OBS4.
            // Temos então que extrair os gols.
            var golCasa = stringSujaHoraETalvezGolsCasa.Last().ToString();
            var golsVisita = linhaSplitadaEmTraço[2].Trim().First().ToString();

            if(!int.TryParse(golCasa, out int gc)) {
                Debug.WriteLine("******* Não conseguiu parsear em int " + gc);
                return false;
            }


            if (!int.TryParse(golsVisita, out int gv))
            {
                Debug.WriteLine("******* Não conseguiu parsear em int " + gv);
                return false;
            }

            GolsCasa = gc;
            GolsVisitante = gv;
            Debug.WriteLine("******* Parseou com sucesso resultados " + GolsCasa + " - " + GolsVisitante);


            Debug.WriteLine($"******* RESULTADO : camp {Campeonato} | golscasa {GolsCasa} | golsvisi {GolsVisitante} | Hora {Hora} | Minuto {Minuto}");
            return true;
        }

        internal Resultado? getItem()
        {
            if(Campeonato==null) return null;
            if (GolsCasa == null) return null;
            if(GolsVisitante == null) return null;
            return new Resultado(Campeonato, DataHora, GolsCasa.Value, GolsVisitante.Value);
        }
    }
}
