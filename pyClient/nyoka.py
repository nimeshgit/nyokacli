
from nyokaCli.cliParsing import CLIParser, InitAction, AddAction

def handle_init_action(init_action):
    print(init_action)

def handle_add_action(add_action):
    print(add_action)

if __name__ == "__main__":
    import sys

    # remove first item from argv, the name of the nyoka program
    CLIParser(sys.argv[1:]) \
        .with_action(InitAction, handle_init_action) \
        .with_action(AddAction, handle_add_action) \