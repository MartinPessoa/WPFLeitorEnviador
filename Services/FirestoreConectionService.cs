using Google.Apis.Auth.OAuth2;
using Google.Apis.Script.v1;
using Google.Apis.Util.Store;
using Google.Cloud.Firestore;
using System.Threading;
using System.Threading.Tasks;

namespace WPFLeitorEnviador.Services
{
    class FirestoreConectionService
    {
        private UserCredential? credentials = null;
        private static FirestoreDb? _db = null;
        private static readonly string _projectId = "937364024256";

        protected async Task LoginAsync()
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



        public static async Task<FirestoreDb> GetDBAsync()
        {
            if(_db != null) { return _db; }

            _db = await FirestoreDb.CreateAsync(_projectId);

            return _db;
        }

    }
}
