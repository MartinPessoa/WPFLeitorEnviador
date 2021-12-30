using iBets.Core;
using iBets.Core.SimulaçõesApostas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFLeitorEnviador
{
    /// <summary>
    /// Lógica interna para Teste4TirosWindow.xaml
    /// </summary>
    public partial class Teste4TirosWindow : Window
    {
        private Random rnd = new Random(0);

        public Teste4TirosWindow()
        {
            InitializeComponent();
        }

        private List<Resultado> Processar(string resultados)
        {
            resultados = resultados.Trim();
            resultados = resultados.Replace('	', ' ');
            resultados = resultados.Replace('	', ' ');
            resultados = resultados.Replace("\n", " ").Replace("\r", "");

            List<Resultado> retorno = new List<Resultado>();
            Resultado? resDaVez;

            var splitted = resultados.Split(' ');

            foreach (var token in splitted)
            {
                if (token == "" ) continue;

                if(token == "5+") 
                {
                    resDaVez = new Resultado("TESTE", DateTime.Now, 6, 0);
                    retorno.Add(resDaVez);
                    continue;
                }

                var subsplit = token.Split('x');

                resDaVez = GetResultadoFromStrings(subsplit[0], subsplit[1]);

                if(resDaVez == null)
                {
                    DisplayTextResultados.Text += "Não foi possível ler o resultado: " + token + "\n";
                } else
                {
                    retorno.Add(resDaVez);
                }
            }

            return retorno;
        }

        private static Resultado? GetResultadoFromStrings(string golsCasa, string golsVisitante)
        {
            int gc = -1;
            int gv = -1;


            if (golsCasa == "5+")
            {
                gc = 6;
            } else
            {
                if(!int.TryParse(golsCasa, out gc)) { return null; }
            }

            if (golsVisitante == "5+") 
            {
                gv = 6;
            }
            else
            {
                if (!int.TryParse(golsVisitante, out gv)) { return null; }
            }


            return new Resultado("TESTE", DateTime.Now, gc, gv);    
        }

        private async void BtnSimular_Click(object sender, RoutedEventArgs e)
        {
            if(InputResultados.Text == "") { return; }

            

            DisplayTextResultados.Text = "";

            var converteuCincoMais = decimal.TryParse(TextCincoMais.Text, out decimal valorInicialCincoMais);
            var converteuOver35 = decimal.TryParse(TextOver35.Text, out decimal valorInicialOver35);

            ConfigsIniciasDeAposta.Configurar(30,valorInicialOver35, valorInicialCincoMais);

            var ListaAposta = new List<BaseAposta>();
            BaseAposta apostaDaVez;

            var ListaResultados = Processar(InputResultados.Text);

            if(converteuCincoMais == false || converteuOver35 == false) { return; }

            apostaDaVez = ApostaTeste4Tiros.Primeira(valorInicialOver35, valorInicialCincoMais);

            IProgress<string> informarParada = new Progress<string>(InformarStatus);
            IProgress<string> informarAposta = new Progress<string>(InformarAposta);

            await Task.Run(() =>
            {
                int i = 0;
                int total = ListaResultados.Count();

                decimal ganhos = 0;
                decimal apostado = 0;
                decimal totalAcumulado = 0;

                foreach (var res in ListaResultados)
                {
                    i++;
                    informarParada.Report($"Lendo {i} de {total}");
                    apostaDaVez.InformarResultado(res);

                    informarAposta.Report("\n*************************************\n");

                    informarAposta.Report(apostaDaVez.ToString());

                    ListaAposta.Add(apostaDaVez);

                    ganhos += apostaDaVez.CalcularGanhos();
                    apostado += apostaDaVez.CalcularTotalApostado();
                    totalAcumulado += apostaDaVez.CalcularAcumuladoNãoGanho();

                    apostaDaVez = apostaDaVez.Proxima();



                    //informarParada.Report("lendo")

                }
                informarParada.Report($"Total resultados lidos: {total}");

                informarAposta.Report($"\n\n\n TOTAL GANHOS: {ganhos}\n TOTAL APOSTADO: {apostado}");


                //foreach (var item in ListaResultados)
                //{
                    //ganhos += item.gan

                //}

            });


        }

        private void InformarAposta(string aposta)
        {
            DisplayTextResultados.Text += aposta;
        }

        private void InformarStatus(string status)
        {
            LblStatus.Content = status;
        }
        private Resultado GetAleatório()
        {
            List<Tuple<int, int>> gols = new()
            {
                new Tuple<int, int>(4, 1),
                new Tuple<int, int>(5, 1),
                new Tuple<int, int>(0, 2),

                new Tuple<int, int>(3, 5),
                new Tuple<int, int>(2, 10),
                new Tuple<int, int>(1, 20),
            };



            var totalWeight = 0;
            foreach (var gol in gols)
            {
                totalWeight += gol.Item2;
            }
            var randomN = rnd.Next(0, totalWeight);

            int golsCasa = 0;
            int golsVisitante = 0;

            foreach (var g in gols)
            {
                if(randomN < g.Item2)
                {
                    golsCasa = g.Item1;
                }

                randomN = randomN - g.Item2;
            }

            randomN = rnd.Next(0, totalWeight);
            foreach (var g in gols)
            {
                if (randomN < g.Item2)
                {
                    golsVisitante= g.Item1;
                }
                randomN = randomN - g.Item2;
            }





            return new Resultado("TESTE", DateTime.Now, golsCasa, golsVisitante);
        }
    }
}
