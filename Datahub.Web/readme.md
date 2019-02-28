# Datahub.Web

The ~~Data~~**ResourceHub** web application.

The renaming of the project to 'ResourceHub' happened late in the development cycle, and we've chosen not to rename any code artefacts. All references in code (as opposed to user-facing text) should refer to `Datahub`, and not `ResourceHub`. Elsewhere we may refer to the ResourceHub simply as the `Hub`.

The repository contains a .NET solution file at the top level (which isn't really necessary any more) containing a project called `Datahub.Web`, which is an ASP.NET Core Razor server-side web app.

It's important to make sure you're in the correct project directory when you're using the command line:

    cd Datahub.Web/

## Development

Created in .NET Core v2.1.4. Should work on any platform with .NET Core installed. Created with VS Code, but should be able to use Visual Studio 2017 too.

Environment variables and secrets are configured using the .env pattern. You can make a `.env` file with the appropriate variables. See the `.env.example` file for examples. (And see below for Elasticsearch .)

To restore local packages (equivalent to `npm install`, and VS Code might do this automatically):

    dotnet restore

To run locally (with a `watch` for recompilation on save):

    dotnet watch run

Open a browser at <http://localhost:5000/>

To add new packages:

    dotnet add package Newtonsoft.Json

### Elasticsearch

The Datahub uses Elasticsearch as a backing service.

See the `README.md` in Datahub.Backend to set up a local dev Elasticsearch and load it with dummy data.

You can check it's working by browsing to http://locahost:9200/_stats

You can also connect to the live AWS Elasticsearch Service and use a test index like `test` or `beta`.

Configure a local .aws profile (not recommended for deployment inside AWS, use instance profiles instead):

    ES_AWS_PROFILE=

Use a profile with sufficient permissions to connect to the production AWS Elasticsearch Service.

    aws configure --profile jncc-website-live-search-reader

Alternatively, configure static access keys (not recommended for deployment inside AWS, use instance profiles instead):

    ES_AWS_ACCESSKEY=
    ES_AWS_SECRETACCESSKEY=

Fill in the appropriate sections and inject the `IElasticsearchService` service to get a configured singleton client in the code. The code should fallback from defined access keys, local profile and then instance profile config if they are not configured.

## Deployment

Ensure the machine you are deploying from has awscli installed and confiured.

Build a docker image:

    docker build .

Run locally (on port 8000, with your existing development `.env` file):

    docker run -p 8000:80 --env-file .env {image-name}

From the **Amazon ECR** web console, copy the `jncc-datahub-web`  repository URI to clipboard (e.g. `123456789.dkr.ecr.eu-west-1.amazonaws.com/jncc-datahub-web`).

    docker tag {image-name} {aws-ecr-repository-uri}

Using an AWS profile with sufficient permissions, get the Docker login command:

    aws ecr get-login --no-include-email [--profile your-profile-name]

Run the command that you're given. Then run:

    docker push {aws-ecr-repository-uri}

All but one layer should already exist and you should see the new layer being uploaded.

### AWS Console deployment to Elastic beanstalk

Make a `Dockerrun.aws.json` file (see `Dockerrun.aws.json.template` and paste in the image URL `aws-ecr-image-uri`) of the latest image uploaded to ECR. This file will be`.gitignore`d.

In **AWS Elastic Beanstalk**, click Upload and Deploy > Choose file and upload the `Dockerrun.aws.json` file.

Provide an incremented version string (this appears to be required).

Done! (Phew.)

### Command line deployment to Elastic beanstalk

Install the elastic beanstalk cli if not already done so

    pip install awsebcli --upgrade --user

This will be installed in ~/.local/bin on liunx - you probably want to add this to your PATH statement.

Make a `Dockerrun.aws.json` file (see `Dockerrun.aws.json.template` and paste in the image URL `aws-ecr-image-uri`) of the latest image uploaded to ECR. This file will be`.gitignore`d.

Copy this file into {solution root}/Datahub.Web/eb-deploy

Edit {solution root}/Datahub.Web/eb-deploy/.elasticbeanstalk/config.yaml

Change branch-default/default/environment to the name of the datahub environment you want to deploy to

    cd {solution root}/Datahub.Web/eb-deploy

    eb deploy

## Notes

The green I used for the favicon is rgba(77, 219, 58, 1)


