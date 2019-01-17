import os

class FileUtils():
    _code_dir_name = "Code"
    _models_dir_name = "Models"
    _data_dir_name = "Data"
    
    _required_dir_paths = [
        _code_dir_name,
        _get_nyoka_sub_dir_path(_code_dir_name),
        _models_dir_name,
        _get_nyoka_sub_dir_path(_models_dir_name),
        _data_dir_name,
        _get_nyoka_sub_dir_path(_data_dir_name),
    ]

    @staticmethod
    def _get_nyoka_sub_dir_path(parentDirPath):
        return os.path.join(parentDirPath, "/.nyoka")
    
    @staticmethod
    def check_required_dirs_exist():
        for required_dir_path in _required_dir_paths:
            if not os.path.exists(required_dir_path):
                return False
        
        return True
    
    @staticmethod
    def create_missing_required_dirs():
        for required_dir_path in _required_dir_paths:
            if not os.path.exists(required_dir_path):
                # todo: use pathlib?
