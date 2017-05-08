FROM mono:latest

MAINTAINER Tim Laverty (https://github.com/timlav)

EXPOSE 8080

ENTRYPOINT ["mono", "Microsoft.Azure.IoTSolutions.IotHubManager.WebService.exe"]

COPY . /app/
WORKDIR /app
