# JNCC Datahub

This is the JNCC Datahub app.

## Development

Created in .NET Core v2.1.4. Should work on any platform with .NET Core installed. Should be able to use Visual Studio 2017 or VS Code.

There is a solution file with a single project called Datahub.Web, which is an ASP.NET Core Razor web app. It's important to make sure you're in the correct project directory when you're using the command line:

    cd Datahub.Web/

To restore local packages (equivalent to `npm install`, VS Code might do this automatically):

    dotnet restore

To run locally (with a `watch` for recompilation on save):

    dotnet watch run

Open a browser at `http://localhost:5000/`

To add packages, e.g.

    dotnet add package Newtonsoft.Json

## Environment variables

Elasticsearch is configured via the .env pattern. You can make a `.env` file in the solution, or set environment variables. `.env.example` gives an example of .env file options.

    ELASTICSEARCH_DOMAIN=domain.url
    ELASTICSEARCH_AWS_REGION=aws.region

TODO: See the `README.md` in Datahub.Backend to set up a local dev Elasticsearch.

You can also connect to the production AWS Elasticsearch Service.

Configure a local .aws profile (not recommended for deployment inside AWS, use instance profiles instead):

    ELASTICSEARCH_AWS_PROFILE=

Use a profile with sufficient permissions to connect to the production AWS Elasticsearch Service.

    aws configure --profile jncc-website-live-search-reader

Alternatively, configure static access keys (not recommended for deployment inside AWS, use instance profiles instead):

    ELASTICSEARCH_AWS_ACCESSKEY=
    ELASTICSEARCH_AWS_SECRETACCESSKEY=

Fill in the appropriate sections and inject the `IElasticsearchService` service to get a configured singleton client in the code. The code should fallback from defined access keys, local profile and then instance profile config if they are not configured.

## Deployment

Ensure the machine you are deploying from has awscli installed and confiured.

Install the eb deploy tool:

    dotnet tool install -g Amazon.ElasticBeanstalk.Tools

Build a docker image:

    docker build .

Run locally (on port 8000 with your exising dev .env file):

    docker run -p 8000:80 --env-file ./Datahub.Web/.env {image-name}

From the AWS console, copy the `jncc-datahub-web` Amazon ECR repository URI to clipboard (e.g. `123456789.dkr.ecr.eu-west-1.amazonaws.com/jncc-datahub-web`).

    docker tag {image-name} {aws-ecr-repository-uri}

Using an AWS profile with sufficient permissions, get the Docker login command:

    aws ecr get-login --no-include-email [--profile your-profile-name]

Run the command that you're given. Then run:

    docker push {aws-ecr-repository-uri}

Make a Dockerrun.aws.json file (see Dockerrun.aws.json.example) with the aws-ecr-repository-uri (it should be .gitignored).

Then in Elastic Beanstalk, click Upload and Deploy > Choose file and upload the Dockerrun.aws.json file.

TODO: There's an error at this point.

Done! (Phew.)

## Notes

The green I used for the favicon is rgba(77, 219, 58, 1)
