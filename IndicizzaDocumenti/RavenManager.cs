using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.FileSystem;
using Raven.Client.FileSystem.Shard;
using Raven.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndicizzaDocumenti
{
    public class RavenManager
    {
        public List<SearchResult> SearchText(IDocumentStore documentStore, string searchString)
        {

            List<SearchResult> array = new List<SearchResult>();
            using (var session = documentStore.OpenSession())
            {
                if (!String.IsNullOrEmpty(searchString))
                {

                    var searchArray = searchString.Split(' ');

                    var stringSearchTerms = String.Empty;

                    foreach (var el in searchArray)
                    {
                        if (el != " " && String.IsNullOrEmpty(el) == false)
                            stringSearchTerms += el.Replace(" ", "") + "* ";
                    }

                    var result = session.Query<Documento, DocumentoIndex>()
                        .Search(x => x.Contenuto, stringSearchTerms, 1, SearchOptions.Or, EscapeQueryOptions.AllowPostfixWildcard)
                        .ToFacets(new List<Facet>() { new Facet<Documento>() { Name = x => x.Contenuto }, new Facet<Documento>() { Name = x => x.Titolo } });



                    //array = result.Select(x => new SearchResult() { NomeFile = x.NomeFile, Indirizzo = x.Indirizzo }).ToList();

                    array = result.Results.Select(x => new SearchResult() { Indirizzo = x.Key + " " + x.Value.Values.ToList().Select(x => x. }).ToList();
                }

                return array;
            }
        }

        public string Suggestions(IDocumentStore documentStore, string searchString)
        {

            string output = String.Empty;
            using (var session = documentStore.OpenSession())
            {
                if (!String.IsNullOrEmpty(searchString))
                {
                    var result = documentStore.DatabaseCommands.Suggest("DocumentoIndex", new SuggestionQuery() { Field = "Titolo", Term = searchString });

                    // var result = session.Query<Company, CompanyByNameIndex>().Where(x => x.Name.StartsWith(searchString)).Search(x => x.Name, "", 1, SearchOptions.

                    //.Search(x => x.ProductName, searchString, 2, SearchOptions.Or, EscapeQueryOptions.RawQuery).OfType<Company>().ToList();
                    //var result = session.Query<Documento, DocumentoIndex>()
                    //    .Search(x => x.Contenuto, searchString + "*", 1, SearchOptions.Guess, EscapeQueryOptions.AllowPostfixWildcard);
                    //var result = session.Query<PeopleCompaniesSearchDue.FullTextResult, PeopleCompaniesSearchDue>()
                    //    //.Where(x => x.FirstName == searchString)
                    //    .Search(x => x.Content, searchString + "*", 2, SearchOptions.Guess, EscapeQueryOptions.AllowPostfixWildcard)
                    //        .ProjectFromIndexFieldsInto<PeopleCompaniesSearch.FullTextResult>().ToList();

                    //string[] array = result.Select(x => x.NomeFile).ToArray();

                    string[] array = result.Suggestions; //result.Select(x => "Name: " + x.Name + " Company Address: " + x.Address.City).ToArray();

                    for (int i = 0; i < array.Length; i++)
                    {
                        output += array[i] + "\r";
                    }
                }

                return output;
            }
        }

        public IList<string> GetFilesList(IFilesStore filesStore)
        {
            var files = filesStore.AsyncFilesCommands.BrowseAsync(0, 100).Result;

            List<string> filesList = new List<string>();

            foreach (var file in files)
            {
                filesList.Add(file.FullPath);
            }

            return filesList;
        }

        public void DeleteFile(IDocumentSession document, IFilesStore fileStore, string file)
        {
         
        }

        public string UploadFile(string filePath, string country)
        {
            var command = new AsyncShardedFilesServerClient(FileStore.ShardStrategy);

            string fileName = String.Empty;

            try
            {
                using (FileStream fileStream = File.Open(filePath, FileMode.Open))
                {
                    string file = Path.GetFileName(filePath);

                    fileName = command.UploadAsync(file, new RavenJObject()
                    {
                        {
                            "Country", country
                        }
                    }, fileStream).Result;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return fileName;
        }
    }

    public class SearchResult
    {
        public string Indirizzo { get; set; }

        public string NomeFile { get; set; }
    }
}
