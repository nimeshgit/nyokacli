from ..logging import Logger, LogTable
from .cliActions import InitAction, AddAction
from ..constants import PROGRAM_NAME
from .cliParseException import CLIParseException

class CLIParser:
    _possible_actions = [
        InitAction,
        AddAction,
    ]

    @staticmethod
    def _list_available_actions():
        table = LogTable() \
            .add_header("Action Name") \
            .add_header("Action Description")
        
        for possible_action in CLIParser._possible_actions:
            action_description = possible_action.get_description()
            
            table.add_row(
                action_description.name,
                action_description.summary
            )
        
        Logger.log_table(table, show_frame=False)
    
    def __init__(self, args):
        self.parsed_action = None
        
        if len(args) == 0:
            Logger.log_error("ERROR: " + PROGRAM_NAME + " must be called with an action name. Available action names:")
            CLIParser._list_available_actions()
            return
        
        action_name = args[0]

        parse_class = None

        for possible_action in CLIParser._possible_actions:
            if action_name == possible_action.get_description().name:
                parse_class = possible_action

        
        if parse_class == None:
            Logger.log_error("Invalid action name " + action_name + ". Available action names:")
            CLIParser._list_available_actions()
            return
        
        try:
            self.parsed_action = parse_class(args[1:])
        except CLIParseException as err:
            Logger.log_error("Error: " + str(err))
    
    def with_action(self, action_class, call_func):
        if isinstance(self.parsed_action, action_class):
            call_func(self.parsed_action)
        
        return self
