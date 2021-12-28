using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFLeitorEnviador.Services
{
    internal class CSVWriter
    {
        private readonly string _pasta;
        private string _campeonato = "";
        private IProgress<string> _progress;
        private bool _isWriting;

        public CSVWriter(string pasta, IProgress<string> progress)
        {
            this._pasta = pasta;
            this._progress = progress;
        }

        public async Task Write(string dados, string campeonato)
        {
            this._campeonato = campeonato;
            //escolher arquivo
            var data = DateTime.Now;
            var textoData = $"{ data.Year}{ data.Month}{data.Day}-{data.Hour}{data.Minute}{data.Second}";




            var destinoSemArquivo = $"{_pasta}\\{_campeonato}";
            var destinoComArquivo = $"{_pasta}\\{_campeonato}\\{textoData}.csv";

            if (!File.Exists(destinoSemArquivo))
            {
                DirectoryInfo di = Directory.CreateDirectory(destinoSemArquivo);
            }


            var pastaBack = $"{_pasta}\\ANTIGOS\\{_campeonato}\\{textoData}.csv";


            //verificar se existe
            //if (!File.Exists(destinoSemArquivo))
            //{
                //criarnovo
            //    await CriarEEscrever(listaString, destinoComArquivo);
            //    return;
            //}

            //está muito grande?
            //var fileInfo = new FileInfo(destinoComArquivo);
            //if (fileInfo.Length > 5000000) //5 MB
            //{
                //backup do anterior
            //    File.Move(destinoComArquivo, pastaBack);

            while(_isWriting)
            {
                _progress.Report("Aguardando liberação para escrever...");
                Thread.Sleep(300);
            }

                //criar novo
               await CriarEEscrever(dados, destinoComArquivo);
            //}

            //escrever
            //AppendAoArquivo(listaString, destinoComArquivo);

        }

        private void AppendAoArquivo(string lista, string arquivo)
        {
            byte[] start = Encoding.UTF8.GetBytes(lista);
            //byte[] ending = Encoding.UTF8.GetBytes("</root>");

            byte[] data = File.ReadAllBytes(arquivo);

            int bom = (data[0] == 0xEF) ? 3 : 0;

            using (FileStream s = File.Create(arquivo))
            {
                if (bom > 0)
                {
                    s.Write(data, 0, bom);
                }
                s.Write(start, 0, start.Length);
                s.Write(data, bom, data.Length - bom);
                //s.Write(ending, 0, ending.Length);
            }
        }

        private async Task CriarEEscrever(string lista, string arquivo)
        {
             
            this._isWriting = true;
            await File.WriteAllTextAsync(arquivo,lista);
            this._isWriting = false;
        }
    }
}
