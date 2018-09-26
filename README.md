JNCC Datahub
============

This is a proof-of-concept Datahub app.

Development
-----------
Created in .NET Core v2.1.4. Should work on any platform with .NET Core installed. Should be able to use Visual Studio 2017 or VS Code. 

There is a solution file with a single project called Datahub.Web, which is an ASP.NET Core Razor web app. It's important to make sure you're in the correct project directory when you're using the command line:

    cd Datahub.Web/

To restore local packages (equivalent to `npm install`, VS Code might do this automatically):

    dotnet restore

To run locally (with a `watch` for recompilation on save):

    dotnet watch run

Open a browser at http://localhost:5000/

To add packages, e.g.

    dotnet add package Newtonsoft.Json

Deployment
----------
Manual deployments to jncc-datahub-test env:

Ensure the machine you are deploying from has awscli installed and confiured.

Install the eb deploy tool:

    dotnet tool install -g Amazon.ElasticBeanstalk.Tools

Deploy:

    cd /Datahub.web
    dotnet eb deploy-environment -app datahub -env jncc-datahub-test --region eu-west-1

Notes
-----
The green I used for the favicon is rgba(77, 219, 58, 1)
