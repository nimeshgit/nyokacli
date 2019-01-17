import os
from ..logging import Logger

class FileUtilsException(Exception):
    """Exception from file utils"""

class FileUtils():
    _resource_dirs = {
        "code": "Code",
        "model": "Models",
        "data": "Data",
    }
    
    @staticmethod
    def _get_required_dir_paths():
        return [
            FileUtils._resource_dirs["code"],
            FileUtils._get_nyoka_sub_dir_path(FileUtils._resource_dirs["code"]),
            FileUtils._resource_dirs["model"],
            FileUtils._get_nyoka_sub_dir_path(FileUtils._resource_dirs["model"]),
            FileUtils._resource_dirs["data"],
            FileUtils._get_nyoka_sub_dir_path(FileUtils._resource_dirs["data"]),
        ]

    @staticmethod
    def _get_nyoka_sub_dir_path(parentDirPath):
        return os.path.join(parentDirPath, ".nyoka")
    
    @staticmethod
    def check_required_dirs_exist():
        try:
            for required_dir_path in FileUtils._get_required_dir_paths():
                if not os.path.exists(required_dir_path):
                    return False
            
            return True
        except:
            raise FileUtilsException("Failed to check current directory for required directories")
    
    @staticmethod
    def create_missing_required_dirs():
        try:
            for required_dir_path in FileUtils._get_required_dir_paths():
                if not os.path.exists(required_dir_path):
                    os.makedirs(required_dir_path)
                    Logger.log_line("Created directory " + required_dir_path)
                else:
                    Logger.log_line("Directory " + required_dir_path + " already exists")
        except FileUtilsException:
            raise
        except:
            raise FileUtilsException("Failed to create missing required directories in current directory")
    
    @staticmethod
    def get_resources_present(resource_type):
        resources_path = FileUtils._resource_dirs[resource_type]
        
        try:
            pass
            # unimplemented
            # print(onlyfiles)
        except:
            raise FileUtilsException("Unable to read files from  " + resources_path + " directory")
