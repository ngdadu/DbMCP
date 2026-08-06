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
| 44 | Dai Duong Nguyen | dai.duong@nguyen.vn | N/A |
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
