DMI Extraction layer
====================

H3 - Big Data (modeller og datamodellering)

Requirements
------------

1. [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
2. [DMI API](https://confluence.govcloud.dk/display/FDAPI)
3. [MongoDB](https://www.mongodb.com/)

Setup
-----

In the `appsettings.json` file, there are two sections which you will need to modify in. `ApiKeys` and `ConnectionStrings`

Start by setting your DMI ApiKey.\
Set `ApiKeys:DMI` to the API key given by DMI.

Set `ConnectionStrings:MongoDB` to the connection string you get from your mongodb provider.

User Secrets
************

You can also add User Secrets to the project, to prevent API keys being pushed to the repository.

Open a command prompt window, and navigate to the Extract folder.

Run the following command to initialzie the user-secrets for the project:\
`dotnet user-secrets init`

And to add the keys to the user-secrets, you need to run the following commands, but **remember** to change value to the actual value needed.

```dotnet user-secrets set "ApiKeys:DMI" "VALUE"```

```dotnet user-secrets set "ConnectionStrings:MongoDB" "VALUE"```

Improvements
------------

TBD

Known Bugs
----------

TBD

Changelog
---------

[18/12/2023] Added BackgroundService to fetch data from DMI and store on MongoDB\
[18/12/2023] Added project