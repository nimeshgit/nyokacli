using CLIInterfaceNS;
using System.Collections.Generic;
using Constants;
using FileTypeInferenceNS;
using System.Linq;
using PackageManagerNS;

namespace CLIParserNS
{
    internal class ParseUtils
    {
        public class ArgumentProcessException : System.Exception
        {
            public ArgumentProcessException(string mssg)
            : base (mssg)
            {
            }
        }

        public static ResourceType parseResourceType(string type)
        {
            if (type.ToLower() == "model") return ResourceType.Model;
            else if (type.ToLower() == "data") return ResourceType.Data;
            else if (type.ToLower() == "code") return ResourceType.Code;
            else throw new ArgumentProcessException($"Invalid resource type \"{type}\"");
        }
        
        private static ResourceType inferResourceTypeFromResourceName(string resourceName)
        {
            try{
                if (FileTypeInference.isCodeFileName(resourceName)) return ResourceType.Code;
                if (FileTypeInference.isDataFileName(resourceName)) return ResourceType.Data;
                if (FileTypeInference.isModelFileName(resourceName)) return ResourceType.Model;
                throw new System.Exception();
            }
            catch (System.Exception)
            {
                throw new ArgumentProcessException($"Could not infer resource type from extension of {resourceName}");
            }
        }

        private static void validateVersionString(string resourceStr, string version)
        {
            string[] versionSections = version.Split('.');
            foreach (string section in versionSections)
            {
                if (section.Trim() != section)
                {
                    throw new ArgumentProcessException("Version cannot contain spaces");
                }
                // if this is section empty
                if (section.Length == 0)
                {
                    // if this is also the only section
                    if (versionSections.Length == 1)
                    {
                        throw new ArgumentProcessException($"\"{resourceStr}\" is missing version");
                    }
                    else
                    {
                        throw new ArgumentProcessException(
                            $"Invalid version \"{version}\" in \"{resourceStr}\": Version should be " +
                            "series of numbers separated by periods, like 1.2.3 or 333.3.20"
                        );
                    }
                }
                foreach (char ch in section)
                {
                    if (!"1234567890".Contains(ch))
                    {
                        throw new ArgumentProcessException($"Invalid version character \"{ch}\" in {resourceStr}");
                    }
                }
            }
        }

        private static void validateFileName(string resourceName)
        {
            foreach (char ch in resourceName)
            {
                // @TODO use regex?
                if (!"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890_-.".Contains(ch))
                {
                    throw new ArgumentProcessException($"Invalid character in file name: \"{ch}\"");
                }
            }
        }

        public static PackageManager.ResourceIdentifier generateResourceIdentifier(string resourceStr)
        {
            string[] splitByAt = resourceStr.Split('@');

            string version;
            string resourceName;

            // If there is no @ symbol in the string to separate name from version
            if (splitByAt.Length == 1)
            {
                version = null; // redundant?
                resourceName = splitByAt[0];
            }
            // If there is one @ symbol in the string to separate name from version
            else if (splitByAt.Length == 2)
            {
                version = splitByAt[1];
                resourceName = splitByAt[0];
            }
            // If there is more than one @ symbol in the string
            else
            {
                throw new ArgumentProcessException(
                    $"Could not process \"{resourceStr}\": Only one @ symbol is permitted in a resource name"
                );
            }

            ResourceType resourceType = inferResourceTypeFromResourceName(resourceName);

            if (version != null)
            {
                // validate version string
                validateVersionString(resourceStr, version);
            }
            
            validateFileName(resourceName);

            return new PackageManager.ResourceIdentifier(resourceName, resourceType, version);
        }
    }
    
    public class InitOptions
    {
        public static OptionDescription description = new OptionDescription(
            "init",
            "Initialize code, data and model folders."
        );

        public InitOptions(List<string> actionArgs, out bool successful)
        {
            if (actionArgs.Count != 0)
            {
                CLIInterface.logError($"{description.name} action takes no arguments. Usage:");
                logUsage();
                successful = false;
                return;
            }

            successful = true;
        }

        private void logUsage()
        {
            CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                {"Example Action", 0},
                {"Explanation", 0},
            };

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name}",
                $"Initialize the resource directories that {ConstStrings.APPLICATION_ALIAS} uses in the current directory"
            );

            CLIInterface.logTable(table, visibleLines: false);
        }
    }

    public class AddOptions
    {
        public static OptionDescription description = new OptionDescription(
            "add",
            "Download and add a resource to local files."
        );

        public PackageManager.ResourceIdentifier resourceIdentifier;

        public AddOptions(List<string> actionArgs, out bool successful)
        {
            if (actionArgs.Count != 1)
            {
                CLIInterface.logError($"{description.name} action takes one argument: resource name. Usage:");
                logUsage();
                successful = false;
                return;
            }

            string resourceStr = actionArgs[0];
            try
            {
                resourceIdentifier = ParseUtils.generateResourceIdentifier(resourceStr);
            }
            catch (ParseUtils.ArgumentProcessException ex)
            {
                CLIInterface.logError($"Error: {ex.Message}");
                successful = false;
                return;
            }

            successful = true;
        }

        private void logUsage()
        {
            CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                {"Example Action", 0},
                {"Explanation", 0},
            };

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name} programName.py",
                "Adds the latest version a code resource named programName.py to the code folder in the current directory"
            );

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name} model1.pmml@1.2.3",
                "Adds version 1.2.3 of a model resource named model1.pmml to the model folder in the current directory"
            );

            CLIInterface.logTable(table, visibleLines: false);
        }
    }

    public class RemoveOptions
    {
        public static OptionDescription description = new OptionDescription(
            "remove",
            "Remove resource from local files."
        );

        public PackageManager.ResourceIdentifier resourceIdentifier;

        public RemoveOptions(List<string> actionArgs, out bool successful)
        {
            if (actionArgs.Count != 1)
            {
                CLIInterface.logError($"{description.name} action takes one argument: resource name. Usage:");
                logUsage();
                successful = false;
                return;
            }

            string resourceStr = actionArgs[0];
            try
            {
                resourceIdentifier = ParseUtils.generateResourceIdentifier(resourceStr);
            }
            catch (ParseUtils.ArgumentProcessException ex)
            {
                CLIInterface.logError($"Error: {ex.Message}");
                successful = false;
                return;
            }

            successful = true;
        }

        private void logUsage()
        {
            CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                {"Example Action", 0},
                {"Explanation", 0},
            };

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name} programName.py",
                "Removes a code resource named programName.py from the code folder in the current directory"
            );

            CLIInterface.logTable(table, visibleLines: false);
        }
    }

    public class ListOptions
    {
        public static OptionDescription description = new OptionDescription(
            "list",
            "List resources in local files."
        );

        public ResourceType? resourceType;

        public ListOptions(List<string> actionArgs, out bool successful)
        {
            if (actionArgs.Count > 1)
            {
                CLIInterface.logError($"{description.name} action takes one optional argument: resource type. Usage:");
                logUsage();
                successful = false;
                return;
            }
            
            if (actionArgs.Count == 1)
            {
                string resourceTypeStr = actionArgs[0];
                
                try
                {
                    resourceType = ParseUtils.parseResourceType(resourceTypeStr);
                }
                catch (ParseUtils.ArgumentProcessException ex)
                {
                    CLIInterface.logError($"Error: {ex.Message}");
                    successful = false;
                    return;
                }
            }

            successful = true;
        }

        private void logUsage()
        {
            CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                {"Example Action", 0},
                {"Explanation", 0},
            };

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name} model",
                "Lists all model resources present locally"
            );

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name}",
                "Lists all resources present locally"
            );

            CLIInterface.logTable(table, visibleLines: false);
        }
    }

    public class AvailableOptions
    {
        public static OptionDescription description = new OptionDescription(
            "available",
            "List available resources."
        );

        public ResourceType? resourceType;

        public AvailableOptions(List<string> actionArgs, out bool successful)
        {
            if (actionArgs.Count > 1)
            {
                CLIInterface.logError($"{description.name} action takes one optional argument: resource type. Usage:");
                logUsage();
                successful = false;
                return;
            }
            
            if (actionArgs.Count == 1)
            {
                string resourceTypeStr = actionArgs[0];
                
                try
                {
                    resourceType = ParseUtils.parseResourceType(resourceTypeStr);
                }
                catch (ParseUtils.ArgumentProcessException ex)
                {
                    CLIInterface.logError($"Error: {ex.Message}");
                    successful = false;
                    return;
                }
            }

            successful = true;
            return;
        }

        private void logUsage()
        {
            CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                {"Example Action", 0},
                {"Explanation", 0},
            };

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name} model",
                "Lists all model resources available on server"
            );

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name}",
                "Lists all resources available on server"
            );

            CLIInterface.logTable(table, visibleLines: false);
        }
    }

    public class DependenciesOptions
    {
        public static OptionDescription description = new OptionDescription(
            "dependencies",
            "List dependencies of resource."
        );

        public PackageManager.ResourceIdentifier resourceIdentifier;

        public DependenciesOptions(List<string> actionArgs, out bool successful)
        {
            if (actionArgs.Count != 1)
            {
                CLIInterface.logError($"{description.name} action takes one argument: resource name. Usage:");
                logUsage();
                successful = false;
                return;
            }

            string resourceStr = actionArgs[0];
            try
            {
                resourceIdentifier = ParseUtils.generateResourceIdentifier(resourceStr);
            }
            catch (ParseUtils.ArgumentProcessException ex)
            {
                CLIInterface.logError($"Error: {ex.Message}");
                successful = false;
                return;
            }

            successful = true;
        }

        private void logUsage()
        {
            CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                {"Example Action", 0},
                {"Explanation", 0},
            };

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name} programName.py",
                "Lists dependencies of a code resource named programName.py. If the resource is present locally," +
                " this will list the dependencies of the local version. Otherwise, it'll list the dependencies of " +
                "the latest version available on the server."
            );

            CLIInterface.logTable(table, visibleLines: false);
        }
    }

    public class PublishOptions
    {
        public static OptionDescription description = new OptionDescription(
            "publish",
            "Publish a resource in local files to the server."
        );

        public PackageManager.ResourceIdentifier resourceIdentifier;
        public List<PackageManager.ResourceIdentifier> deps = new List<PackageManager.ResourceIdentifier>();

        public PublishOptions(List<string> actionArgs, out bool successful)
        {
            if (actionArgs.Count < 1)
            {
                CLIInterface.logError($"{description.name} action takes one required argument, resource name, with an option --deps to add dependencies. Usage:");
                logUsage();
            }

            string resourceStr = actionArgs[0];
            try
            {
                resourceIdentifier = ParseUtils.generateResourceIdentifier(resourceStr);
            }
            catch (ParseUtils.ArgumentProcessException ex)
            {
                CLIInterface.logError($"Error: {ex.Message}");
            }

            if (actionArgs.Count >= 2)
            {
                string shouldBeDepsOption = actionArgs[1];
                if (shouldBeDepsOption != "--deps")
                {
                    CLIInterface.logError($"Error: {description.name} has one possible option, --deps. Usage:");
                    logUsage();
                    successful = false;
                    return;
                }
                
                IEnumerable<string> depStrings = actionArgs.Skip(2);

                foreach (string depString in depStrings)
                {
                    try
                    {
                        deps.Add(ParseUtils.generateResourceIdentifier(depString));
                    }
                    catch (ParseUtils.ArgumentProcessException ex)
                    {
                        CLIInterface.logError($"Error: {ex.Message}");
                        successful = false;
                        return;
                    }
                }
            }

            successful = true;
        }

        private void logUsage()
        {
            CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                {"Example Action", 0},
                {"Explanation", 0},
            };

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name} programName.py",
                "Publish local code resource called programName.py. Since version is not provided, version number defaults to 1.0."
            );

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name} dataFile.csv@10.2.2",
                "Publish local data resource called dataFile.csv with version number 10.2.2"
            );

            table.addRow(
                $"{ConstStrings.APPLICATION_ALIAS} {description.name} program1.py@2.2.3 --deps program2.py@1.2.3 program2@9.1",
                "Publish local code resource program1.py at version 2.2.3, with the dependencies program2.py, version 1.2.3 and program2, version 9.1"
            );

            CLIInterface.logTable(table, visibleLines: false);
        }
    }

    public class OptionDescription
    {
        public readonly string name;
        public readonly string summary;

        public OptionDescription(
            string name,
            string summary)
        {
            this.name = name;
            this.summary = summary;
        }
    }
    public class CLIParser
    {
        private InitOptions parsedInitOptions = null;
        private AddOptions parsedAddOptions = null;
        private RemoveOptions parsedRemoveOptions = null;
        private ListOptions parsedListOptions = null;
        private AvailableOptions parsedAvailableOptions = null;
        private DependenciesOptions parsedDependenciesOptions = null;
        private PublishOptions parsedPublishOptions = null;

        private static Dictionary<string, OptionDescription> optionDescriptions = new Dictionary<string, OptionDescription> {
            { InitOptions.description.name, InitOptions.description },
            { AddOptions.description.name, AddOptions.description },
            { RemoveOptions.description.name, RemoveOptions.description },
            { ListOptions.description.name, ListOptions.description },
            { AvailableOptions.description.name, AvailableOptions.description },
            { DependenciesOptions.description.name, DependenciesOptions.description },
            { PublishOptions.description.name, PublishOptions.description },
        };
        
        public CLIParser(List<string> args)
        {
            if (args.Count == 0)
            {
                CLIInterface.logError($"{ConstStrings.APPLICATION_ALIAS} must be called with an action name. Available actions:");
                listActions();
                return;
            }

            string actionName = args[0];
            
            // arguments without action name
            List<string> actionArgs = args.Skip(1).ToList();

            if (!optionDescriptions.ContainsKey(actionName))
            {
                CLIInterface.logError($"Invalid action name {actionName}. Available actions:");
                listActions();
                return;
            }

            bool optionParseSuccessful;
            if (actionName == InitOptions.description.name)
            {
                var opts = new InitOptions(actionArgs, out optionParseSuccessful);
                
                if (optionParseSuccessful) parsedInitOptions = opts;
            }
            if (actionName == AddOptions.description.name)
            {
                var opts = new AddOptions(actionArgs, out optionParseSuccessful);
                
                if (optionParseSuccessful) parsedAddOptions = opts;
            }
            if (actionName == RemoveOptions.description.name)
            {
                var opts = new RemoveOptions(actionArgs, out optionParseSuccessful);
                
                if (optionParseSuccessful) parsedRemoveOptions = opts;
            }
            if (actionName == ListOptions.description.name)
            {
                var opts = new ListOptions(actionArgs, out optionParseSuccessful);
                
                if (optionParseSuccessful) parsedListOptions = opts;
            }
            if (actionName == AvailableOptions.description.name)
            {
                var opts = new AvailableOptions(actionArgs, out optionParseSuccessful);
                
                if (optionParseSuccessful) parsedAvailableOptions = opts;
            }
            if (actionName == DependenciesOptions.description.name)
            {
                var opts = new DependenciesOptions(actionArgs, out optionParseSuccessful);
                
                if (optionParseSuccessful) parsedDependenciesOptions = opts;
            }
            if (actionName == PublishOptions.description.name)
            {
                var opts = new PublishOptions(actionArgs, out optionParseSuccessful);
                
                if (optionParseSuccessful) parsedPublishOptions = opts;
            }
        }

        private void listActions()
        {
            CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                {"Action Name", 0},
                {"Action Description", 0},
            };
            
            foreach (KeyValuePair<string, OptionDescription> descriptionKVPair in optionDescriptions)
            {
                string actionName = descriptionKVPair.Key;
                OptionDescription description = descriptionKVPair.Value;

                table.addRow(
                    actionName,
                    description.summary
                );
            }

            CLIInterface.logTable(table, visibleLines: false);
        }

        public CLIParser withInit(System.Action<InitOptions> callFunc)
        {
            if (parsedInitOptions != null) callFunc(parsedInitOptions);

            return this;
        }
        
        public CLIParser withAdd(System.Action<AddOptions> callFunc)
        {
            if (parsedAddOptions != null) callFunc(parsedAddOptions);

            return this;
        }
        
        public CLIParser withRemove(System.Action<RemoveOptions> callFunc)
        {
            if (parsedRemoveOptions != null) callFunc(parsedRemoveOptions);

            return this;
        }
        
        public CLIParser withList(System.Action<ListOptions> callFunc)
        {
            if (parsedListOptions != null) callFunc(parsedListOptions);

            return this;
        }
        
        public CLIParser withAvailable(System.Action<AvailableOptions> callFunc)
        {
            if (parsedAvailableOptions != null) callFunc(parsedAvailableOptions);

            return this;
        }
        
        public CLIParser withDependencies(System.Action<DependenciesOptions> callFunc)
        {
            if (parsedDependenciesOptions != null) callFunc(parsedDependenciesOptions);

            return this;
        }
        
        public CLIParser withPublish(System.Action<PublishOptions> callFunc)
        {
            if (parsedPublishOptions != null) callFunc(parsedPublishOptions);

            return this;
        }
    }
}