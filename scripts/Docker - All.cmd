cd ..

dotnet build --nologo -p:TargetArch=linux-musl-x64
docker-compose up --force-recreate --build login master world
PAUSE