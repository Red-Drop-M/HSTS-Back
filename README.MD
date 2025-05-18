# Prerequiset : 
    . Docker 
    . Git 
    . Dotnet 9 SDK
    . PostgreSQL & PgAdmin 
```
    docker pull bitnami/kafka:3.4
```
how to build and run  the docker image 
```
 cd ~/kafka-Image-Directory/
docker compose up -d 
cd /project-folder/
docker compose up --build
```
