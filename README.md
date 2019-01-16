## A cli for nyoka

### To debug/run server

* Navigate to RepositoryServer in terminal.\
Create ZMODServerFiles directory in home directory (~/ZMODServerFiles), with the directories Models, Code and Data inside it. Then:
    
        dotnet run

### To debug/run client

* Navigate to client in terminal

        dotnet run

### To build client to native executable (from inside the client directory)

* Linux:
    
		dotnet publish -r linux-x64 -c release
		
* Mac

        dotnet publish -r osx-x64 -c release

* Windows:
            
    <i>haven't tried yet on windows</i>
    
Dotnet may give an error stating that platform linker 'clang-3.9' is not found. In this case, install clang (if it isn
t already present), run "export CppCompilerAndLinker=clang" in the shell, then run the publish command again.

Copy the resulting file nyoka_cli somewhere in your path (like /usr/local/bin)
    
