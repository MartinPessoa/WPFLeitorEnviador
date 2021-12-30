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

        private void BtnSimular_Click(object sender, RoutedEventArgs e)
        {
            var converteuCincoMais = decimal.TryParse(TextCincoMais.Text, out decimal valorInicialCincoMais);
            var converteuOver35 = decimal.TryParse(TextOver35.Text, out decimal valorInicialOver35);

            var ListaAposta = new List<BaseAposta>();
            BaseAposta apostaDaVez;


            if(converteuCincoMais == false || converteuOver35 == false) { return; }

            DisplayTextResultados.Text = "Iniciando...\n";

            apostaDaVez = ApostaTeste4Tiros.Primeira(valorInicialOver35, valorInicialCincoMais);

            for (int i = 0;i<60;i++)
            {
                apostaDaVez.InformarResultado(GetAleatório());

                DisplayTextResultados.Text += "\n*************************************\n";

                DisplayTextResultados.Text += apostaDaVez.ToString();

                ListaAposta.Add(apostaDaVez);

                apostaDaVez = apostaDaVez.Proxima(valorInicialOver35, valorInicialCincoMais);
            }



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
