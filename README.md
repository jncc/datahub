JNCC Datahub
============

This is a proof-of-concept Datahub app.

Development
-----------
Created in .NET Core v2.1.4. Should work on any platform with .NET Core installed. 

There is a solution file but it's not linked up to any projects yet. There is a single project called Datahub.Web which is an ASP.NET Core Razor web app. 

To restore local packages:

    dotnet restore

To run locally:

    dotnet run

Open a browser at http://localhost:5000/

Deployment
----------
The plan is to deploy to Elastic Beanstalk.