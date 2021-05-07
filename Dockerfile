FROM ubuntu:latest

MAINTAINER JOSEANTONIO seriamente.fs@gmail.com


RUN apt-get -y update && apt-get -y upgrade && apt-get install -y build-essential

