using CommandLine;
using CommandLine.Text;

namespace AlarmPeople.Bcp;

public class Options
{
    public const bool Default_CreateTable = true;
    public const bool Default_Truncate = true;
    public const int Default_BatchSize = 1000;
    public const string Default_DbDataType = "varchar(500) null";

    public string? Tablename { get; set; }
    public string? Database { get; set; }


    [Value(0, MetaName = "Database and Table", HelpText = "[database]..[tablename]")]
    public string DbTable {
        get => $"{Database}..{Tablename}";
        set {
            var v1 = value.Split("..");
            if (v1.Length != 2)
                throw new ArgumentException("First argument must be [database]..[tablename]");
            Database = v1[0];
            Tablename = v1[1];
        }
    }

    [Value(1, MetaName = "Action", HelpText = "in")]
    public string? Verb { get; set; }

    [Value(2, MetaName = "Excel file", HelpText = "Excel filename")]
    public string? Filename { get; set; }


    [Option('S', "server", HelpText = "SQL Server")]
    public string? Server { get; set; }

    [Option('U', "user", HelpText = "SQL user login name")]
    public string? User { get; set; } 

    [Option('P', "password", HelpText = "SQL user login password")]
    public string? Password { get; set; }

    [Option("nocreate", HelpText = "Don't create table")]
    public bool? NoCreateTable { get; set; }
    public bool? CreateTable => !NoCreateTable;

    // [Option("drop", HelpText = "Drop table (if already exists)")]
    // public bool DoDropTableIfExists { get; set; } = true;

    [Option("keep", HelpText = "Keep existing data in table")]
    public bool? KeepExistingData { get; set; }
    public bool? Truncate => !KeepExistingData;

    // [Option("datatype", HelpText = "Default column datatype")]
    // public string DefaultSqlDataType { get; set; } = "varchar(256) not null";

    [Option('f', "formatfile", HelpText = "TOML configuration file")]
    public string? FormatFile { get; set; }

    [Option('F', "firstrow", HelpText = "First row of import - counting from line 1")]
    public int? FirstRowNum { get; set; }

    [Option('L', "lastrow", HelpText = "Last row of import - counting from line 1")]
    public int? LastRowNum { get; set; }

    [Option('b', "batchsize", HelpText = "Number of rows to commit in a batch")]
    public int? BatchSize { get; set; }

    [Usage(ApplicationAlias = "apbcp")]
    public static IEnumerable<Example> Examples
    {
        get
        {
            return new List<Example>() 
            {
                new Example("Basic import", new Options { DbTable = "mydb..mytbl", Verb = "in", Filename = "myXls.xlsx" })
            };
        }
    }
    
    /*
    public static Options Parse(string[] args)
    {
        // apbcp [database]..[tablename] in [excel-file] -S <server> -U <user> -P <password>

        Options res = new Options();

        if (args.Length < 3)
            throw new ArgumentException("Must have at least 3 arguments");

        // parse [database]..[tablename]
        var v1 = args[0].Split("..");
        if (v1.Length != 2)
            throw new ArgumentException("First argument must be [database]..[tablename]");
        res.Database = v1[0];
        res.Tablename = v1[1];

        if (args[1] != "in")
            throw new ArgumentException("apbcp only supports 'in' ");

        res.Filename = args[2];

        res.Server = Environment.GetEnvironmentVariable("APBCP_SERVER") ?? "";
        res.User = Environment.GetEnvironmentVariable("APBCP_USER") ?? "";
        res.Password = Environment.GetEnvironmentVariable("APBCP_PASSWORD") ?? "";

        var i = 3;
        while(i < args.Length)
        {
            var j = i;
            (res.Server, i) = GetOpt(res.Server, args, i, "-S");
            (res.User, i) = GetOpt(res.User, args, i, "-U");
            (res.Password, i) = GetOpt(res.Password, args, i, "-P");
            if (j==i)
                throw new ArgumentException($"Unsupported argument: {args[i]}");
        }

        return res;
    }
    private static (string, int) GetOpt(string value, string[] args, int ix, string opt)
    {
        var s = args[ix];
        if (s.Substring(0,2)==opt)
        {
            if (s.Length>2)   
            {
                return (s.Substring(2), ix + 1);
            }
            else
            {
                if (ix == args.Length)
                    throw new ArgumentException($"Missing value for argument: {s}");
                return (args[ix + 1], ix + 2);
            }
        }
        return (value, ix);
    }
    */
}