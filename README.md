## A cli for nyoka

### To debug/run server

* Navigate to RepositoryServer in terminal.\
Create ZMODServerFiles directory in home directory (~/ZMODServerFiles), with the directories Models, Code and Data inside it. Then:
    
        dotnet run

### To debug/run client

* Navigate to client in terminal

        dotnet run

### To build client to native executable (from inside the client directory)
##### Note: You apparently can only build for a platform from inside that platform. You can't build the windows exe from inside linux.

* Linux:
    
        dotnet publish -r linux-x64 -c release
		
* Mac

        dotnet publish -r osx-x64 -c release


* Windows:
            
	##### Note: You'll probably need Visual Studio (community edition is OK), with C++ tools installed, to run this.
        dotnet publish -r win-x64 -c release
    
On Linux or Mac, dotnet may give an error stating that platform linker 'clang-3.9' is not found. In this case, install clang (if it isn
t already present), run "export CppCompilerAndLinker=clang" in the shell, then run the publish command again.

After building the client to native, you can find the executable in "client/bin/release/netcoreapp2.1/ linux-x64 or osx-x64 or win-x64 /native"

To start using the executable on linux or mac, copy it nyoka to somewhere in your path (like /usr/local/bin)
    
