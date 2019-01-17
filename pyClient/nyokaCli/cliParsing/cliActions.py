from .cliParseException import CLIParseException
from ..constants import PROGRAM_NAME

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
            raise CLIParseException(PROGRAM_NAME + " " + self.get_description().name + " takes no parameters.")

class AddAction:
    @staticmethod
    def get_description():
        return ActionDescription(
            "add",
            "Download and add a resource to local files."
        )
    
    def __init__(self, action_args):
        if len(action_args) != 1:
            raise CLIParseException(PROGRAM_NAME + " " + self.get_description().name + " takes one parameter: Resource name.")