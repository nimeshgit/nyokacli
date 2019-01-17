from nyoka_cli.file_utils import FileUtils
from nyoka_cli.cli_parsing import CLIParser, InitAction, AddAction, ListAction
from nyoka_cli.package_manager import PackageManager

def handle_init_action(init_action):
    PackageManager.init_dirs()

def handle_add_action(add_action):
    PackageManager.add_package(
        add_action.resource_description
    )

def handle_list_action(list_action):
    PackageManager.list_local_resources()

if __name__ == "__main__":
    import sys

    # remove first item from argv, the name of the nyoka program
    CLIParser(sys.argv[1:]) \
        .with_action(InitAction, handle_init_action) \
        .with_action(AddAction, handle_add_action) \
        .with_action(ListAction, handle_list_action) \