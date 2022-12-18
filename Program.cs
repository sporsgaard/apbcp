using System.Data.SqlClient;
using System.Diagnostics;
using System.Text.Json;
using CommandLine;
using CommandLine.Text;
using Serilog;

/*
  apbcp [database]..[tablename] in [excel-filename] -S <server> -U <user> -P <password>
*/

/*
First impl.
Scan header for cols
create table from header with "varchar(256) null"
*/

namespace AlarmPeople.Bcp;
class Program
{
    // run as 
    // dotnet run -- sbnwork..test in test.xlsx -S localhost/sbnms1 -U sa -P sbntests
    static int Main(string[] args)
    {
        int result = -1;
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // Change to .MinimumLevel.Verbose() if more info is needed
            .WriteTo.Console()
            .CreateLogger();
        try
        {
            var sw = new Stopwatch();
            sw.Start();
            // Parse arguments
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            
            parserResult
            .WithParsed<Options>(opt =>
            {
                var (valid, msg) = ValidateOptions(opt);
                if (!valid)
                {
                    Console.WriteLine("Validation fail");
                    Console.WriteLine(msg);
                    // PrintHelp(parserResult);
                    result = 1;
                }
                Run(opt);
                // parsing successful; go ahead and run the app
                Log.Verbose("ARGS WHERE:");
                Log.Verbose("{x}", JsonSerializer.Serialize(opt));
                Log.Information("DONE in {x} ms", sw.ElapsedMilliseconds);
                result = 0;
            })
            .WithNotParsed<Options>(e =>
            {
                // parsing unsuccessful; deal with parsing errors
                result = 1;
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return 1;
        }
        return result;
    }

    static void PrintHelp(ParserResult<Options> parserResult)
    {
        var helpText = GetHelp<Options>(parserResult);
        Console.WriteLine(helpText);
    }

    //Generate Help text
    static string GetHelp<T>(ParserResult<T> result)
    {
        // use default configuration
        // you can customize HelpText and pass different configurations
        //see wiki
        // https://github.com/commandlineparser/commandline/wiki/How-To#q1
        // https://github.com/commandlineparser/commandline/wiki/HelpText-Configuration
        return HelpText.AutoBuild(result, h => h, e => e);
    }
    static int Run(Options opt)
    {
        using var excel = new ExcelReader(opt);

        var ctrl = excel.GetBcpController(0);
        foreach (var r in ctrl.Fields)
        {
            Log.Verbose("Got Field Ix: {ix}, Nm: {nm}, Def: {def}", r.ColIx, r.Name, r.Definition);
        }

        using var conn = new MssqlConnection(opt.Server!, opt.User!, opt.Password!, opt.Database!);
        conn.Open();
        if (opt.CreateTable ?? Options.Default_CreateTable)
        {
            conn.DropTable(opt.Tablename!);
            conn.CreateTable(opt.Tablename!, ctrl);
        }

        if (opt.Truncate ?? Options.Default_Truncate)
            conn.TruncateTable(opt.Tablename!);

        using (var bulkInsert = new SqlBulkCopy(conn.DSN))
        {
            bulkInsert.DestinationTableName = opt.Tablename;
            var bchSize = opt.BatchSize ?? Options.Default_BatchSize;
            foreach (var tbl in ctrl.GetData(batchSize: bchSize))
            {
                bulkInsert.WriteToServer(tbl);
                Log.Warning("Inserted {r} rows", tbl.Rows.Count);
            }
        }
 
        return 0;
    }

    static (bool, string) ValidateOptions(Options opt)
    {
        if (opt.Verb != "in")
            return (false, "Only 'in' action allowed");
        if (string.IsNullOrWhiteSpace(opt.Server))
            return (false, "Missing server");
        if (string.IsNullOrWhiteSpace(opt.User))
            return (false, "Missing user");
        if (string.IsNullOrWhiteSpace(opt.Password))
            return (false, "Missing password");
        if (string.IsNullOrWhiteSpace(opt.Tablename))
            return (false, "Missing table name");
        if (string.IsNullOrWhiteSpace(opt.Filename))
            return (false, "Missing filename");

        if (!File.Exists(opt.Filename!))
            return (false, $"File: {opt.Filename} does not exist");

        return (true, "");
    }
}