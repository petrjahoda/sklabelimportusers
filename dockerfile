﻿FROM microsoft/dotnet:2.2-runtime

RUN cp /usr/share/zoneinfo/Europe/Prague /etc/localtime
RUN apt-get update && apt-get install nano -y && apt-get install vim -y && apt-get install iputils-ping -y 

WORKDIR /publish
COPY /publish /publish
ENTRYPOINT dotnet sklabelimportusers.dll