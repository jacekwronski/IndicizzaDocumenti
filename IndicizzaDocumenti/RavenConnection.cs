using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IndicizzaDocumenti
{
    public class RavenConnection
    {
        private static IDocumentStore documentStore;

        public static IDocumentStore DocumentStore
        {
            get
            {
                if (documentStore == null)
                {
                    documentStore = CreateDocumentStore();
                }

                return documentStore;
            }
        }

        private static IDocumentStore CreateDocumentStore()
        {
            var documentStore = new DocumentStore();

            documentStore.ConnectionStringName = "s4";

            documentStore.Initialize();

            //DatabaseConnection.DefineShardedConventions(null, documentStore);

            IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), documentStore);

            return documentStore;
        }
    }
}
