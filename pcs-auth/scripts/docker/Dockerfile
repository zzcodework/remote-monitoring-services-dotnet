FROM microsoft/dotnet:1.1.2-runtime-jessie

MAINTAINER Devis Lucato (https://github.com/dluc)

LABEL Tags="Azure,IoT,Solutions,authz,authn,.NET"

ENTRYPOINT ["/bin/bash", "/app/run.sh"]

COPY . /app/
WORKDIR /app
