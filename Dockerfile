FROM mcr.microsoft.com/dotnet/sdk:6.0.301-bullseye-slim AS build

RUN apt-get update

#---------------------------------------------------------------------
# Tools setup
#---------------------------------------------------------------------
RUN dotnet tool install --global coverlet.console
RUN dotnet tool install --global dotnet-reportgenerator-globaltool

# sonarcloud
ARG SONAR_PROJECT_KEY=Tech-Fabric_CloudFabric.JobHandler
ARG SONAR_OGRANIZAION_KEY=tech-fabric
ARG SONAR_HOST_URL=https://sonarcloud.io
ARG SONAR_TOKEN
ARG GITHUB_TOKEN
RUN dotnet tool install --global dotnet-sonarscanner
RUN apt-get update && apt-get install -y openjdk-11-jdk
#sonarcloud

ENV PATH="/root/.dotnet/tools:${PATH}" 

RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | sh
#---------------------------------------------------------------------
# /Tools setup
#---------------------------------------------------------------------


#---------------------------------------------------------------------
# Test database setup
#---------------------------------------------------------------------
RUN apt-get install -y postgresql postgresql-client postgresql-contrib

USER postgres

RUN echo "local   all   all               md5" >> /etc/postgresql/13/main/pg_hba.conf &&\
    echo "host    all   all   0.0.0.0/0   md5" >> /etc/postgresql/13/main/pg_hba.conf

RUN echo "listen_addresses='*'" >> /etc/postgresql/13/main/postgresql.conf
RUN service postgresql start \
    && psql --command "CREATE ROLE fiber_pim_dev WITH CREATEDB NOSUPERUSER NOCREATEROLE INHERIT NOREPLICATION CONNECTION LIMIT -1 LOGIN PASSWORD 'fiber_pim_dev';" \
    && psql --command "DROP DATABASE IF EXISTS cloudfabric_fiber_task_test;" \
    && psql --command "CREATE DATABASE cloudfabric_fiber_task_test WITH OWNER = fiber_pim_dev ENCODING = 'UTF8' CONNECTION LIMIT = -1;" \
    && psql --command "GRANT ALL ON DATABASE cloudfabric_fiber_task_test TO postgres;"
#---------------------------------------------------------------------
# /Test database setup
#---------------------------------------------------------------------


USER root
WORKDIR /

#---------------------------------------------------------------------
# Nuget restore 
# !Important: this is a nice hack to avoid package restoration on each docker build step.
# Since we only copy nuget.config and csproj files, docker will not run restore unless nuget.config or csproj files change.
#---------------------------------------------------------------------
#COPY nuget.config /src/nuget.config

COPY Processor/CloudFabric.JobHandler.Processor.csproj /src/Processor/CloudFabric.JobHandler.Processor.csproj
COPY CloudFabric.JobHandler.Processor.Tests/CloudFabric.JobHandler.Processor.Tests.csproj /src/CloudFabric.JobHandler.Processor.Tests/CloudFabric.JobHandler.Processor.Tests.csproj

RUN dotnet restore /src/Processor/CloudFabric.JobHandler.Processor.csproj
RUN dotnet restore /src/CloudFabric.JobHandler.Processor.Tests/CloudFabric.JobHandler.Processor.Tests.csproj
#---------------------------------------------------------------------
# /Nuget restore 
#---------------------------------------------------------------------

#---------------------------------------------------------------------
# Build artifacts
#---------------------------------------------------------------------
COPY /. /src

ARG PULLREQUEST_TARGET_BRANCH
ARG PULLREQUEST_BRANCH
ARG PULLREQUEST_ID
ARG BRANCH_NAME

# Start Sonar Scanner
# Sonar scanner has two different modes - PR and regular with different set of options
RUN if [ -n "$SONAR_TOKEN" ] && [ -n "$PULLREQUEST_TARGET_BRANCH" ] ; then echo "Running sonarscanner in pull request mode: sonar.pullrequest.base=$PULLREQUEST_TARGET_BRANCH, sonar.pullrequest.branch=$PULLREQUEST_BRANCH, sonar.pullrequest.key=$PULLREQUEST_ID" && dotnet sonarscanner begin \
  /k:"$SONAR_PROJECT_KEY" \
  /o:"$SONAR_OGRANIZAION_KEY" \
  /d:sonar.host.url="$SONAR_HOST_URL" \
  /d:sonar.login="$SONAR_TOKEN" \
  /d:sonar.pullrequest.base="$PULLREQUEST_TARGET_BRANCH" \
  /d:sonar.pullrequest.branch="$PULLREQUEST_BRANCH" \
  /d:sonar.pullrequest.key="$PULLREQUEST_ID" \
  /d:sonar.cs.opencover.reportsPaths=/artifacts/tests/*/coverage.opencover.xml ; elif [ -n "$SONAR_TOKEN" ] ; then echo "Running sonarscanner in branch mode: sonar.branch.name=$BRANCH_NAME" && dotnet sonarscanner begin \
  /k:"$SONAR_PROJECT_KEY" \
  /o:"$SONAR_OGRANIZAION_KEY" \
  /d:sonar.host.url="$SONAR_HOST_URL" \
  /d:sonar.login="$SONAR_TOKEN" \
  /d:sonar.branch.name="$BRANCH_NAME" \
  /d:sonar.cs.opencover.reportsPaths=/artifacts/tests/*/coverage.opencover.xml ; fi

USER postgres
RUN service postgresql start && \
    psql -U "postgres" -d "cloudfabric_fiber_task_test" -a -f "/src/Processor/Db/JobDb.sql"

USER root

RUN service postgresql start && sleep 20 && \
    dotnet test /src/CloudFabric.JobHandler.Processor.Tests/CloudFabric.JobHandler.Processor.Tests.csproj --logger trx --results-directory /artifacts/tests --configuration Release --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=json,cobertura,lcov,teamcity,opencover

ARG COVERAGE_REPORT_GENERATOR_LICENSE
ARG COVERAGE_REPORT_TITLE
ARG COVERAGE_REPORT_TAG
ARG COVERAGE_REPORT_GENERATOR_HISTORY_DIRECTORY

RUN reportgenerator "-reports:/artifacts/tests/*/coverage.cobertura.xml" -targetdir:/artifacts/code-coverage "-reporttypes:HtmlInline_AzurePipelines_Light;SonarQube;TextSummary" "-title:$COVERAGE_REPORT_TITLE" "-tag:$COVERAGE_REPORT_TAG" "-license:$COVERAGE_REPORT_GENERATOR_LICENSE" "-historydir:$COVERAGE_REPORT_GENERATOR_HISTORY_DIRECTORY"

# End Sonar Scanner
RUN if [ -n "$SONAR_TOKEN" ] ; then dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN" ; fi

ARG PACKAGE_VERSION

RUN sed -i "s|<Version>.*</Version>|<Version>$PACKAGE_VERSION</Version>|g" /src/Processor/CloudFabric.JobHandler.Processor.csproj && \
    dotnet pack /src/Processor/CloudFabric.JobHandler.Processor.csproj -o /artifacts/nugets -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg && \

ARG NUGET_API_KEY
RUN if [ -n "$NUGET_API_KEY" ] ; then dotnet nuget push /artifacts/nugets/CloudFabric.JobHandler.$PACKAGE_VERSION.nupkg --skip-duplicate -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json ; fi

#---------------------------------------------------------------------
# /Build artifacts
#---------------------------------------------------------------------