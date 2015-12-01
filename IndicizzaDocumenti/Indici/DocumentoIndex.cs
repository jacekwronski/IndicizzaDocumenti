using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndicizzaDocumenti
{
    public class DocumentoIndex : AbstractIndexCreationTask<Documento>
    {
        public class FullTextResult
        {
            public string Titolo { get; set; }
            public string NomeFile { get; set; }
            public string Indirizzo { get; set; }
        }

        public DocumentoIndex()
        {
            Map = documenti => from documento in documenti
                                          select new
                                          {
                                              Titolo = documento.Titolo,
                                              Indirizzo = documento.Indirizzo,
                                              Contenuto = documento.Contenuto
                                          };


            this.Analyze(x => x.Contenuto, typeof(Lucene.Net.Analysis.Standard.StandardAnalyzer).AssemblyQualifiedName);

            this.Index(x => x.Contenuto, FieldIndexing.Analyzed);

            this.Index(x => x.Titolo, FieldIndexing.Analyzed);
            this.Index(x => x.Indirizzo, FieldIndexing.NotAnalyzed);

            this.Suggestion(x => x.Titolo);
        }

    }
}
