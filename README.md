A cli for nyoka

To run server:
    Navigate to example_server in terminal
    execute "dotnet run"

To run client:
    Navigate to client in terminal
    execute "dotnet run"

If you're using bash, to avoid building client each time it's run, you may want to put something this in your ~/.bashrc or ~/.bash_profile file:

alias nyoka "dotnet run --project --nobuild path/to/ny_cli/client"
