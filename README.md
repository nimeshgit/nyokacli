## A cli for nyoka

### To debug/run server

* Navigate to RepositoryServer in terminal.\
Create ZMODServerFiles directory in home directory (~/ZMODServerFiles), with the directories Models, Code and Data inside it. Then:

        dotnet run

### To debug/run client

* Navigate to client in terminal

        dotnet run

### To build client

* Linux:

        dotnet publish -r linux-x64 -c release

* Mac

        dotnet publish -r osx-x64 -c release


* Windows:

	##### Note: You'll probably need Visual Studio (community edition is OK) with C++ tools installed to run this.
        dotnet publish -r win-x64 -c release

## After building, to alias nyoka on mac or linux:
alias nyoka="path/to/ny_cli/client/bin/release/ ** insert netcoreapp version ** / ** platform identifier ** / publish/nyoka"
