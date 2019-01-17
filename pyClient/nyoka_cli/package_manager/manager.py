from ..file_utils import FileUtils, FileUtilsException
from ..logging import Logger
from ..constants import RESOURCE_TYPES

class PackageManager():
    @staticmethod
    def init_dirs():
        try:
            FileUtils.create_missing_required_dirs()
        except FileUtilsException as ex:
            Logger.log_error("File Error: " + str(ex))
    
    @staticmethod
    def add_package(resource_description):
        pass
    
    @staticmethod
    def list_local_resources():
        try:
            if not FileUtils.check_required_dirs_exist():
                create_dirs = Logger.ask_yes_no("Required local resource directories do not exist. Create them now?")
                
                if create_dirs:
                    FileUtils.create_missing_required_dirs()

            for resource_type in RESOURCE_TYPES:
                FileUtils.get_resources_present(resource_type)
        
        except FileUtilsException as ex:
            Logger.log_error("File Error: " + str(ex))
