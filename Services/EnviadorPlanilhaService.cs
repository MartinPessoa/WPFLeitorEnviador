using Google.Apis.Auth.OAuth2;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPFLeitorEnviador.Domain;
using static Google.Apis.Script.v1.ScriptsResource;

namespace WPFLeitorEnviador.Services
{
    class EnviadorPlanilhaService
    {
        private UserCredential? credentials = null;
        IProgress<string>? _informarResultado;

        // Obs: por enquanto está no HEAD da macro
        private readonly string implantacaoUrl = "AKfycbyxQmW_MAa9oe9lQIlrfUjOXNu8pZf748UPxIAhntof";

        ~EnviadorPlanilhaService()
        {
            _informarResultado = null;
        }

        public async Task LoginAsync()
        {
            this.credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "937364024256-7hb9123veusbra3r7k56s4ra54l31gbl.apps.googleusercontent.com",
                    ClientSecret = "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"
                },
               new[] { ScriptService.Scope.Spreadsheets },
               "user",
                CancellationToken.None,
                new FileDataStore("Scripts.List"));
        }

        public async Task EnviarDadosResultadosAsync(List<object>? resultados, IProgress<string>? _informarResultado)
        {
            if(resultados == null)
            {
                return;
            }

            this._informarResultado = _informarResultado;

            InformarProgresso("Checando credencias do Google...");
            if(this.credentials == null)
            {
                await this.LoginAsync();
            }

            InformarProgresso("Iniciando serviço de envio...");
            var service = new ScriptService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = this.credentials!,
                ApplicationName = "Enviador Resultados",
                
            });

            var request = new ExecutionRequest();
            request.Function = "receber";
            request.Parameters = new List<object>();

            foreach(var resultado in resultados)
            {
                request.Parameters.Add(resultado);
            }



            ScriptsResource.RunRequest runreq = service.Scripts.Run(request, implantacaoUrl);

            try
            {
                InformarProgresso("Tentando enviar...");
                var op = runreq.Execute();

                op.Response.ToList().ForEach(x =>
                {
                    Debug.WriteLine(x);
                });
                //Debug.WriteLine(op.Response);
            }   catch (Exception ex)
            {
                InformarProgresso("Não foi possível enviar. Erro: " + ex.Message);
                Debug.WriteLine(ex);
                return;
            }

            InformarProgresso("Envio com sucesso.");

        }

        private void InformarProgresso(string Mensagem)
        {
            if (this._informarResultado == null)
            {
                return;
            }
            //Debug.WriteLine("Informando: " + Mensagem);
            _informarResultado.Report($"{Mensagem}");
        }

    }
}

