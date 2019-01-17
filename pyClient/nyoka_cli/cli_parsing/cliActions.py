from .cliParseException import CLIParseException
from ..constants import PROGRAM_NAME
from .resourceNameParse import parse_resource_name, ResourceNameParseException

class ActionDescription:
    def __init__(self, action_name, action_summary):
        self.name = action_name
        self.summary = action_summary

class InitAction:
    @staticmethod
    def get_description():
        return ActionDescription(
            "init",
            "Initialize code, data and model folders."
        )

    def __init__(self, action_args):
        if len(action_args) != 0:
            raise CLIParseException(PROGRAM_NAME + " " + InitAction.get_description().name + " takes no parameters.")

class AddAction:
    @staticmethod
    def get_description():
        return ActionDescription(
            "add",
            "Download and add a resource to local files."
        )
    
    def __init__(self, action_args):
        if len(action_args) != 1:
            raise CLIParseException(PROGRAM_NAME + " " + AddAction.get_description().name + " takes one parameter: Resource name.")
        
        try:
            self.resource_description = parse_resource_name(action_args[0])
        except ResourceNameParseException as ex:
            raise CLIParseException("Error parsing resource name: " + str(ex))

class ListAction:
    @staticmethod
    def get_description():
        return ActionDescription(
            "list",
            "List resources in local files."
        )
    
    def __init__(self, action_args):
        if len(action_args) != 0:
            raise CLIParseException(PROGRAM_NAME + " " + ListAction.get_description().name + " takes no parameters.")
