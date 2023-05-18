using System.Text;
using System.Text.Json;

namespace DocumentDbComparison.CouchDb;

public class BugCouchDbRepo
{
    public record BugDocument(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public  HttpClient Client { get; }
    public  string DatabaseName { get; }

    public BugCouchDbRepo(IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("couchdb");

        if (connectionString == null)
        {
            throw new ArgumentException("Connection string invalid.");
        }
        var uri = new Uri(connectionString);

        DatabaseName = "bugtracker";

        if (string.IsNullOrEmpty(uri.UserInfo))
            return;

        byte[] byteArray = Array.Empty<byte>();

        //extract user info from URI
        if (uri.UserInfo.Contains(":"))
        {
            var up = uri.UserInfo.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (up.Length == 2)
            {
                byteArray = Encoding.ASCII.GetBytes($"{up[0]}:{up[1]}");
            }

            //create new URI without user info
            uri = new UriBuilder(uri)
            {
                UserName = "",
                Password = ""
            }.Uri;

            Client = new HttpClient { BaseAddress = uri };

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            VerifyAndSetupDatabase();

            Client = new HttpClient { BaseAddress = new Uri(uri.AbsoluteUri + DatabaseName) };

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            //latest versions of CouchDB require a CSRF header
            Client.DefaultRequestHeaders.Referrer = new Uri(uri.AbsoluteUri);
        }
    }

    private void VerifyAndSetupDatabase()
    {
        var response = Client.GetAsync("/_all_dbs").Result;
        response.EnsureSuccessStatusCode();

        var listOfDatabases = JsonSerializer.Deserialize<IEnumerable<string>>(response.Content.ReadAsStringAsync().Result);

        if (listOfDatabases is { } && !listOfDatabases.Contains(DatabaseName))
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Client.BaseAddress + "/"+ DatabaseName)
            };

            response = Client.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;

                throw new Exception(responseContent);
            }
        }

        //create indexes
        response = Client.GetAsync($"/{DatabaseName}/_index").Result;
        response.EnsureSuccessStatusCode();
        var listOfIndexes = JsonSerializer.Deserialize<IndexesResult>(response.Content.ReadAsStringAsync().Result, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        if (!listOfIndexes.indexes.Any(i => i.name == "bug_title"))
        {
            var titleIndex = new IndexItem(new IntexField(new[] { "Title" }), "bug_title", "json");
            var serializedString = JsonSerializer.Serialize(titleIndex, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var content = new StringContent(serializedString, Encoding.UTF8, "application/json");
            response = Client.PostAsync($"/{DatabaseName}/_index", content).Result;
            response.EnsureSuccessStatusCode();
        }

        if (!listOfIndexes.indexes.Any(i => i.name == "bug-id"))
        {
            var titleIndex = new IndexItem(new IntexField(new[] { "_id" }), "bug_id", "json");
            var serializedString = JsonSerializer.Serialize(titleIndex, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var content = new StringContent(serializedString, Encoding.UTF8, "application/json");
            response = Client.PostAsync($"/{DatabaseName}/_index", content).Result;
            response.EnsureSuccessStatusCode();
        }

        //create design documents and view for count
        response = Client.GetAsync($"/{DatabaseName}/_design/bug").Result;

        if(!response.IsSuccessStatusCode)
        {
            //javascript function for map function
            var mapFunc = "function(doc){if(doc.Title && doc.Description){emit(doc._id, 1);}}";

            //create document
            var designDocument = new DesignDocumentResult("bug", new DesignDocumentView(new DesignDocumentViewFunction("_count", mapFunc)), "javascript");
            var serializedString = JsonSerializer.Serialize(designDocument, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var content = new StringContent(serializedString, Encoding.UTF8, "application/json");
            response = Client.PutAsync($"/{DatabaseName}/_design/bug", content).Result;
            response.EnsureSuccessStatusCode();
        }
    }

    record IndexesResult(int total_rows, IndexItem[] indexes);

    record IndexItem(IntexField index, string name,  string type);

    record IntexField(string[] fields);

    /* index
     *{
       "index": {
       "fields": ["foo"]
       },
       "name" : "foo-index",
       "type" : "json"
       }
     */

    record DesignDocumentResult(string _id, DesignDocumentView views, string language);

    record DesignDocumentView(DesignDocumentViewFunction count);

    record DesignDocumentViewFunction(string reduce, string map);
}