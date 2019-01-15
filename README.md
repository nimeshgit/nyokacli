## A cli for nyoka

### To debug/run server

* Navigate to example_server in terminal
    
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
    
Dotnet may give an error stating that platform linker 'clang-3.9' is not found. In this case, install the latest clang and run "export CppCompilerAndLinker=clang" in bash, then run the publish command again.

Copy the resukting file nyoka_cli somewhere in your path (like /usr/local/bin)
    
