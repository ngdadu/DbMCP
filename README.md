# DbMCP

DbMCP exposes views, functions, and procedures from a SQL Server schema as MCP tools.

## Environment Variables
- `DBMCP_CONNECTION_STR`: SQL Server connection string.
- `DBMCP_SCHEMA`: SQL schema that contains MCP objects.
- `DBMCP_INSTANCE_NAME`: optional instance name to distinguish multiple DbMCP servers.
- `DBMCP_HTTP`: set to `true` to serve MCP over HTTP at `/mcp`; defaults to the stdio transport.

## Metadata and Descriptions
- Database extended properties:
  - `Mcp_Title`: MCP server title.
  - `Mcp_Description`: MCP server description.
- Object descriptions for views, functions, procedures, columns, and parameters are read from `MS_Description`.

## Macros in `Mcp_Title` and `Mcp_Description`
- `%APPNAME%`: assembly name (DbMCP).
- `%APPVERSION%`: assembly version (for example `1.0.0.0`).
- `%INSTANCE%`: instance name.
- `%DBNAME%`: SQL database name.
- `%SERVER%`: SQL server name.
- `%SCHEMA%`: MCP schema name.

## Build
```powershell
dotnet build DbMCP.slnx
```

## Run and Debug in VS Code

This repository already contains ready-to-use configuration files:
- [/.vscode/mcp.json](.vscode/mcp.json)
- [/.vscode/launch.json](.vscode/launch.json)
- [/.vscode/tasks.json](.vscode/tasks.json)

### Recommended Debug Flow
1. Set a breakpoint (for example in `DbMCP/SqlTools.cs` at `BuildServerOptions`).
2. Start debug profile `DbMCP: Launch (project, stdio)`.
3. Trigger an MCP request from your MCP client.
4. Breakpoints should bind and hit as soon as code is executed.

### Why This Works
- Logging is configured to stderr so stdout remains clean for MCP stdio transport.
- `preLaunchTask` builds before debug start.
- You can either launch directly or attach with `DbMCP: Attach to running process`.

## Run and Debug in Visual Studio 2026

Use profile settings in:
- [DbMCP/Properties/launchSettings.json](DbMCP/Properties/launchSettings.json)

Steps:
1. Open `DbMCP.slnx`.
2. Set project `DbMCP` as startup project.
3. Select profile `DbMCP` or `DbMCP.Prebuilt`.
4. Press `F5`.

## MCP Server Startup Options (`mcp.json`)

You can start DbMCP in two common ways:
- `dotnet run --project ...` (development-friendly).
- direct executable `${workspaceFolder}/DbMCP/bin/Debug/net10.0/DbMCP.exe` (debugger-friendly and no wrapper process).

Current workspace config can include both options under different server names.

For HTTP, start the server with `DBMCP_HTTP=true` (and optionally `ASPNETCORE_URLS`, such as `http://localhost:5000`). Configure the MCP client with the resulting `/mcp` URL.

## Example Prompt

Prompt:
- _Lege 20 Kunden an, die eine Email aus Nachname.Vorname und einer bekannten Top-Level Domain besitzen. Die Straßen- und Ortsnamen können Umlaute besitzen. Die Umlaute der Emails müssen als gültige Emails-Adressen konvertiert werden._
- _Gib mir die 10 zuletzt angelegten Kunden als Tabelle._

Answer:
```text
Ich hole jetzt die 10 zuletzt angelegten Kunden über die höchste `Id` ab und formatiere das Ergebnis als Tabelle.

Ran `view_all_customers`
Completed with input: {
  "Top": 10,
  "Where": "1=1 ORDER BY Id DESC"
}
```

# DbMCP

provides the views, functions and procedures with all columns and parameter in a schema of a MSSQL database as tools

## Environments
* `DBMCP_CONNECTION_STR`: Connection string to the database,
* `DBMCP_SCHEMA`: Schema for MCP functionalities (MCP tools)
* `DBMCP_INSTANCE_NAME`: optinal instance name to identify the MCP server - It's helpful if multiple server instances exist

## Descriptions from database for server and tools
* extended properties of database:
  * `Mcp_Title`: Server title
  * `Mcp_Description`: Server description
* extended properties for descriptions of views, functions, procedures, columns and paremeters are in `MS_Description`

## Macros for `Mcp_Title`and `Mcp_Description`
* `%APPNAME%`: Assembly name (DbMCP)
* `%APPVERSION%`: Assembly version (1.0.0.0)
* `%INSTANCE%`: Instance name
* `%DBNAME%`: SQL database name
* `%SERVER%`: SQL server name
* `%SCHEMA%`: SQL schema name for MCP

## Example prompt

**prompt**: 
  * _lege weitere 20 Kunden an, die eine Email aus Nachname.Vorname und einer bekannten Top-Level Domain besitzen. Die Straßen- und Ortsnamen können Umlaute besitzen._
  * _gib mir die 10 zuletzt angelegten Kunden als Tabelle_

**answer**:
```
Ich hole jetzt die 10 zuletzt angelegten Kunden über die höchste `Id` ab und formatiere das Ergebnis als Tabelle.

Ran `view_all_customers` 
Completed with input: {
  "Top": 10,
  "Where": "1=1 ORDER BY Id DESC"
}
```
Hier sind die 10 zuletzt angelegten Kunden:

| Id | Name | Email | Address |
|---:|---|---|---|
| 44 | Đại Dương Nguyễn | dai.duong@nguyen.vn | N/A |
| 43 | Julia Keller | keller.julia@postservice.eu | Domstraße 12, 50668 Köln |
| 42 | Sebastian Vogel | vogel.sebastian@bürgerforum.org | Prinzenstraße 16, 47051 Duisburg |
| 41 | Mia Schäfer | schaefer.mia@direktnetz.net | Breite Straße 74, 18055 Rostock |
| 40 | Tim Weber | weber.tim@softwareone.com | Jakobstraße 30, 93047 Regensburg |
| 39 | Laura Werner | werner.laura@rheinpost.de | Theodor-Heuss-Ring 18, 24103 Kiel |
| 38 | Niklas Schmitt | schmitt.niklas@servicepunkt.eu | Marktplatz 5, 90403 Nürnberg |
| 37 | Hannah Zimmermann | zimmermann.hannah@stadtverein.org | Bahnhofstraße 62, 45127 Essen |
| 36 | David Hartmann | hartmann.david@infobrief.net | Ludwigstraße 45, 80539 München |
| 35 | Lea Krüger | krueger.lea@dialoghaus.com | Böttcherstraße 8, 28195 Bremen |

Hinweis: `Id 44` ist der zuvor versehentlich angelegte Testdatensatz.

**⚠️ Attention:** This address list was generated randomly. The names and addresses are fictitious.

