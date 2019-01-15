A cli for nyoka

To debug/run server:
    Navigate to example_server in terminal
    execute "dotnet run"

To debug/run client:
    Navigate to client in terminal
    execute "dotnet run"

To build client to native executable:
    Linux:
        dotnet publish -r linux-x64 -c release
    Mac:
        dotnet publish -r osx-x64 -c release
    Windows:
        haven't tried yet on windows
    Dotnet may give an error stating that platform linker 'clang-3.9' is not found. In this case, install the latest clang and run "export CppCompilerAndLinker=clang" in bash, then run the publish command again.
    
