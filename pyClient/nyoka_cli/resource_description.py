from .constants import RESOURCE_TYPES

class ResourceDescription:
    # version is optional
    def __init__(self, resource_type, resource_name, version=None):
        assert resource_type in RESOURCE_TYPES, "Assert valid resource type"
        
        self.resource_type = resource_type
        self.resource_name = resource_name
        self.version = version