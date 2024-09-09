
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using DotNetEnv;

Env.Load();

if (args.Length == 0) {
    Console.WriteLine("You must specify the name of a file to be loaded.");
    return;
}

string pdfFile = args[0];

Console.WriteLine($"Loading: {pdfFile}");
MemoryStream memoryStream = new(File.ReadAllBytes(pdfFile));

Console.WriteLine($"Running Document Analysis...");
DocumentAnalysisClient daClient = new (
    new Uri(Env.GetString("AzureDocumentIntelligenceEndpoint")), 
    new AzureKeyCredential(Env.GetString("AzureDocumentIntelligenceKey"))
    );

var daOperation = await daClient.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            "prebuilt-read",
            memoryStream);

var pdfContent = daOperation.Value;

Console.WriteLine($"Saving into MSSQL..");
using(SqlConnection conn = new(Env.GetString("SqlConnectionString")))
{
    conn.Execute("INSERT INTO dbo.Documents (metadata, content) VALUES (@m, @c)", new
    {
        @m = JsonSerializer.Serialize(new { File = pdfFile }),
        @c = pdfContent.Content
    });
}

Console.WriteLine("Done");
