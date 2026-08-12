# DbMCP

DbMCP exposes views, functions, and procedures from a SQL Server schema as MCP tools.

## Environment Variables
- `DBMCP_CONNECTION_STR`: SQL Server connection string.
- `DBMCP_SCHEMA`: SQL schema that contains MCP objects.
- `DBMCP_INSTANCE_NAME`: optional instance name to distinguish multiple DbMCP servers.

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

Hinweis: Beispieldaten wie Namen und Adressen können fiktiv sein.
