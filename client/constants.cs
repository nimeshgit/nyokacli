
namespace Constants
{
    public enum ResourceType { Code, Data, Model }
    public static class ConstStrings
    {
        internal const string RESOURCE_TYPE_HINT = "Resource Type: \"code\", \"data\" or \"model\"";
        internal const string APPLICATION_ALIAS = "nyoka";
    }
    public class ResourceIdentifier
    {
        public string resourceName;
        public string version;
        public ResourceType resourceType;
        public ResourceIdentifier(string resourceName, ResourceType resourceType)
        {
            this.resourceName = resourceName;
            this.resourceType = resourceType;
            this.version = null;
        }
        public ResourceIdentifier(string resourceName, ResourceType resourceType, string version)
        {
            this.resourceName = resourceName;
            this.resourceType = resourceType;
            this.version = version;
        }
    }
}