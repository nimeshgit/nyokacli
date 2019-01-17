from ..resource_description import ResourceDescription

class ResourceNameParseException(Exception):
    """Exception for parsing resource name"""

def parse_resource_name(resource_name_str):
    split_by_version_separator = resource_name_str.split("@")
    
    if len(split_by_version_separator) > 2:
        raise ResourceNameParseException("Only one @ symbol is allowed in resource name, to separate resource name and version number")
    
    resource_name = split_by_version_separator[0]
    resource_type = infer_resource_type(resource_name)

    # include version in description if it's present
    if len(split_by_version_separator) == 1:
        return ResourceDescription(resource_type, resource_name)
    else:
        version = split_by_version_separator[1]
        return ResourceDescription(resource_type, resource_name, version)

def infer_resource_type(resource_name):
    if len(resource_name.split(".")) == 1:
        raise ResourceNameParseException("Could not infer resource type from \"" + resource_name + "\" since it's missing a file extension")
    
    extension = resource_name.split(".")[-1].lower().strip()

    if extension in ["py", "ipynb"]:
        return "code"
    elif extension in ["pmml"]:
        return "model"
    elif extension in ["json", "csv", "png", "jpg", "jpeg", "zip"]:
        return "data"
    else:
        # special case where extension is empty
        if extension == "":
            raise ResourceNameParseException("Could not infer resource type from \"" + resource_name + "\" since file extension is empty")
        else:        
            raise ResourceNameParseException("Could not infer resource type from \"" + resource_name + "\"'s file extension \"" + extension + "\".")
