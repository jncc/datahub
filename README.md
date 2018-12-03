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

Docker
------
Work in progress. You should be able to build a production Docker image of the web app with

    docker build .

Then run on port 8000 with

    docker run -it -p 8000:80 {image-name}

Notes
-----
The green I used for the favicon is rgba(77, 219, 58, 1)

Elasticsearch Config
-----
Elasticsearch is configured via a .env pattern, so environment variables or a .env file in the solution, .env.example gives an example of .env file options

```
ELASTICSEARCH_DOMAIN=domain.url
ELASTICSEARCH_AWS_REGION=aws.region
```

Configured using static access keys (not recommended for deployment inside AWS, use instance profiles instead)
```
ELASTICSEARCH_AWS_ACCESSKEY=
ELASTICSEARCH_AWS_SECRETACCESSKEY=
```

Configured using a local .aws profile (not recommended for deployment inside AWS, use instance profiles instead)
```
ELASTICSEARCH_AWS_PROFILE=
```

Fill in the appropriate sections and inject the IElasticsearchService service to get a configured singleton client in the code, the code should fallback from defined access keys, local profile and then instance profile config if they are not configured.
