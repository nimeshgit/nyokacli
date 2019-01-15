A cli for nyoka

To run server:
    Navigate to example_server in terminal
    execute "dotnet run"

To run client:
    Navigate to client in terminal
    execute "dotnet run"

To build client to native:
    run "dotnet publish -r linux-x64 -c release", replacing linux-x64 with the appropriate platform name.
    Dotnet may give an error stating that platform linker 'clang-3.9' is not found. In this case, install the latest clang and run "export CppCompilerAndLinker=clang" in bash, then run the publish command again.
