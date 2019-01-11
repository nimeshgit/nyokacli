using CommandLine;
using FSOpsNS;
using System.Collections.Generic;
using PackageManagerNS;
using CommandLine.Text;
using Constants;
using CLIInterfaceNS;
using System.Linq;
using FileTypeInferenceNS;

namespace nyoka_cli
{
    [Verb("init", HelpText = "Initialize code, data and model folders.")]
    class InitOptions 
    {
    }

    [Verb("add", HelpText = "Add resource")]
    class AddOptions
    {
        [Value(1, Required = true, HelpText = "Resource name, possibly with version specified (ex: resource.py or resource.py@1.2.3)")]
        public string resourceStr {get;set;}
        
        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples => new List<Example> {
            new Example(
                "Add a data resource",
                new AddOptions
                {
                    resourceStr = "example_data_resource_name.json"
                }
            )
        };
    }

    [Verb("remove", HelpText = "Remove resource")]
    class RemoveOptions
    {
        [Value(1, Required = true, HelpText = "Resource name, possibly with version specified (ex: resource.py or resource.py@1.2.3)")]
        public string resourceStr {get;set;}


        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples => new List<Example> {
            new Example(
                "Remove a data resource",
                new RemoveOptions
                {
                    resourceStr = "example_model_resource_name.csv"
                }
            )
        };
    }

    [Verb("list", HelpText = "List packages")]
    class ListOptions
    {
        [Value(0, Required = false, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public string resourceType {get;set;}

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples => new List<Example> {
            new Example(
                "List all resources",
                new ListOptions {}
            ),
            new Example(
                "List all model resources",
                new ListOptions
                {
                    resourceType = "model"
                }
            )
        };
    }

    [Verb("available", HelpText = "List available packages")]
    class AvailableOptions
    {
        [Value(0, Required = false, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public string resourceType {get;set;}

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples => new List<Example> {
            new Example(
                "List all available resources",
                new AvailableOptions {}
            ),
            new Example(
                "List all available model resources",
                new AvailableOptions {
                    resourceType = "model"
                }
            )
        };
    }

    [Verb("dependencies", HelpText = "List dependencies of resource")]
    class DependenciesOptions
    {
        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}

        [Value(2, Required = false, HelpText = "Resource version")]
        public string version {get;set;} = null;

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples => new List<Example> {
            new Example(
                "List the dependencies of a locally downloaded code resource",
                new DependenciesOptions {
                    resourceName = "some_local_resource_name.py"
                }
            ),
            new Example(
                "List the dependencies of a model resource",
                new DependenciesOptions {
                    resourceName = "name_of_server_model.pmml",
                    version = "1.2.3",
                }
            )
        };
    }

    [Verb("publish", HelpText = "Publish a resource to server")]
    class PublishOptions
    {
        [Value(1, Required = true, HelpText = "Resource name with version. (ex: dep.py@12.3.4 or data.json@3.33.2")]
        public string resourceStr {get;set;}

        [Option("deps", HelpText = "Dependencies of this package, separated by spaces. Example: code.py@1.2.3 data.csv@1.0.0")]
        public IEnumerable<string> deps {get;set;}

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples => new List<Example> {
            new Example("Publish code_file.ipynb version 1.2.3 with no dependencies", new PublishOptions {
                resourceStr = "code_file.ipynb@1.2.3",
                deps = new string[] {},
            }),
            new Example("Publish model1.pmml version 10.2.3 with a data dependency called dataset.json, version 1.2.3", new PublishOptions {
                resourceStr = "model1.pmml@10.2.3",
                deps = new string[] {"dataset.json@1.2.3"},
            }),
        };
    }

    class Program
    {
        internal class ArgumentProcessException : System.Exception
        {
            public ArgumentProcessException(string mssg)
            : base (mssg)
            {
            }
        }

        private static ResourceType parseResourceType(string type)
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

        private static PackageManager.ResourceIdentifier generateResourceIdentifier(string resourceStr)
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
        
        static void Main(string[] args)
        {
            Parser parser = new Parser(settings => {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = System.Console.Error;
            });

            parser.ParseArguments<InitOptions, AddOptions, RemoveOptions, ListOptions, AvailableOptions, DependenciesOptions, PublishOptions>(args)
                .WithParsed<InitOptions>(opts => {
                    PackageManager.initDirectories();
                })
                .WithParsed<ListOptions>(opts => {
                    if (opts.resourceType == null)
                    {
                        PackageManager.listResources(null);
                    }
                    else
                    {
                        try
                        {
                            ResourceType resourceType = parseResourceType(opts.resourceType);
                            PackageManager.listResources(resourceType);
                        }
                        catch (ArgumentProcessException ex)
                        {
                            CLIInterface.logError(ex.Message);
                        }
                    }
                })
                .WithParsed<AddOptions>(opts => {
                    try
                    {
                        PackageManager.addPackage(
                            generateResourceIdentifier(opts.resourceStr)
                        );
                    }
                    catch (ArgumentProcessException ex)
                    {
                        CLIInterface.logError(ex.Message);
                    }
                })
                .WithParsed<RemoveOptions>(opts => {
                    try
                    {
                        PackageManager.removePackage(
                            generateResourceIdentifier(opts.resourceStr)
                        );
                    }
                    catch (ArgumentProcessException ex)
                    {
                        CLIInterface.logError(ex.Message);
                    }
                })
                .WithParsed<AvailableOptions>(opts => {
                    if (opts.resourceType == null)
                    {
                        PackageManager.listAvailableResources(null);
                    }
                    else
                    {
                        try
                        {
                            ResourceType resourceType = parseResourceType(opts.resourceType);
                            PackageManager.listAvailableResources(resourceType);
                        }
                        catch (ArgumentProcessException ex)
                        {
                            CLIInterface.logError(ex.Message);
                        }
                    }
                })
                .WithParsed<DependenciesOptions>(opts => {
                    try
                    {
                        PackageManager.listDependencies(
                            inferResourceTypeFromResourceName(opts.resourceName),
                            opts.resourceName,
                            opts.version
                        );
                    }
                    catch (ArgumentProcessException ex)
                    {
                        CLIInterface.logError(ex.Message);
                    }
                })
                .WithParsed<PublishOptions>(opts => {
                    try
                    {
                        PackageManager.publishResource(
                            generateResourceIdentifier(opts.resourceStr),
                            opts.deps.Select(depStr => generateResourceIdentifier(depStr))
                        );
                    }
                    catch (ArgumentProcessException ex)
                    {
                        CLIInterface.logError(ex.Message);
                    }
                });
        }
    }
}
