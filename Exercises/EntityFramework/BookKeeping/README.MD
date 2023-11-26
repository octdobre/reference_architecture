# Db Migration Tool commands

Usage: `dotnet ef [options] [command]`

## Drop the database
`dotnet ef database drop`

### Update database to specific migration
`dotnet ef database update`

## Migrations
```
Usage: dotnet ef migrations [options] [command]
Options:
  -h|--help        Show help information
  -v|--verbose     Show verbose output.
  --no-color       Don't colorize the output.
  --prefix-output  Prefix output with level.

Commands:
  add     Adds a new migration.
  list    Lists available migrations.
  remove  Removes the last migration.
  script  Generates a SQL script from migrations.

Use "migrations [command] --help" for more information about a command.
```
### Migrations add
```
Usage: dotnet ef migrations add [arguments] [options]
Arguments:
  <NAME>  The name of the migration.
Options:
  -o|--output-dir <PATH>                 The directory (and sub-namespace) to use. Paths are relative to the project directory. Defaults to "Migrations".
  --json                                 Show JSON output.
  -c|--context <DBCONTEXT>               The DbContext to use.
  -p|--project <PROJECT>                 The project to use.
  -s|--startup-project <PROJECT>         The startup project to use.
  --framework <FRAMEWORK>                The target framework.
  --configuration <CONFIGURATION>        The configuration to use.
  --msbuildprojectextensionspath <PATH>  The MSBuild project extensions path. Defaults to "obj".
  -e|--environment <NAME>                The environment to use. Defaults to "Development".
  -h|--help                              Show help information
  -v|--verbose                           Show verbose output.
  --no-color                             Don't colorize the output.
  --prefix-output                        Prefix output with level.
```
## Create migration
`dotnet ef migrations add Initial  (--project ProjectName)`

## Update database to migration
`dotnet ef database update`

## View applied migrations
`dotnet ef migrations list`

## Drop the database without confirmation(#nomercy)
`dotnet ef database drop -f`

# Properties

# Relationships