using CommandLine;
using FSOpsNS;
using System.Collections.Generic;
using PackageManagerNS;
using CommandLine.Text;
using Constants;

namespace ny_cli
{
    [Verb("init", HelpText = "Initialize code, data and model folders.")]
    class InitOptions 
    {
    }

    [Verb("add", HelpText = "Add resource")]
    class AddOptions
    {
        [Value(0, Required = true, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public ResourceType resourceType {get;set;}

        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example(
                        "Add a data resource",
                        new AddOptions
                        {
                            resourceType = ResourceType.data,
                            resourceName = "example_data_resource_name"
                        }
                    )
                };
            }
        }
    }

    [Verb("remove", HelpText = "Remove resource")]
    class RemoveOptions
    {
        [Value(0, Required = true, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public ResourceType resourceType {get;set;}

        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example(
                        "Add a data resource",
                        new RemoveOptions
                        {
                            resourceType = ResourceType.model,
                            resourceName = "example_model_resource_name"
                        }
                    )
                };
            }
        }
    }

    [Verb("list", HelpText = "List packages")]
    class ListOptions
    {
        [Value(0, Required = false, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public string resourceType {get;set;}

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
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
        }
    }

    [Verb("available", HelpText = "List available packages")]
    class AvailableOptions
    {
        [Value(0, Required = false, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public string resourceType {get;set;}

        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
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
        }
    }

    [Verb("dependencies", HelpText = "List dependencies of resource")]
    class DependenciesOptions
    {
        [Value(0, Required = true, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public string resourceType {get;set;}

        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}

        [Value(2, Required = true, HelpText = "Resource version")]
        public string version {get;set;}

        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example(
                        "List the dependencies of a locally downloaded code resource",
                        new DependenciesOptions {
                            resourceType  = "code",
                            resourceName = "some_local_resource_name.py"
                        }
                    ),
                    new Example(
                        "List the dependencies of a model resource on the server",
                        new DependenciesOptions {
                            resourceType = "model",
                            resourceName = "name_of_server_model.pmml"
                        }
                    )
                };
            }
        }
    }

    class Program
    {
        internal class InvalidResourceTypeException : System.Exception
        {
            public InvalidResourceTypeException(string mssg)
            : base (mssg)
            {
            }
        }
        
        private static ResourceType parseResourceType(string type)
        {
            if (type.ToLower() == "model") return ResourceType.model;
            else if (type.ToLower() == "data") return ResourceType.data;
            else if (type.ToLower() == "code") return ResourceType.code;
            else throw new InvalidResourceTypeException(type);
        }
        
        static void Main(string[] args)
        {
            Parser parser = new Parser(settings => {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = System.Console.Error;
            });

            parser.ParseArguments<InitOptions, AddOptions, RemoveOptions, ListOptions, AvailableOptions, DependenciesOptions>(args)
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
                        catch (InvalidResourceTypeException)
                        {
                            System.Console.WriteLine($"Invalid resource type \"{opts.resourceType}\"");
                        }
                    }
                })
                .WithParsed<AddOptions>(opts => {
                    PackageManager.addPackage(
                        opts.resourceType,
                        opts.resourceName
                    );
                })
                .WithParsed<RemoveOptions>(opts => {
                    PackageManager.removePackage(
                        opts.resourceType,
                        opts.resourceName
                    );
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
                        catch (InvalidResourceTypeException)
                        {
                            System.Console.WriteLine($"Invalid resource type \"{opts.resourceType}\"");
                        }
                    }
                })
                .WithParsed<DependenciesOptions>(opts => {
                    try {
                        ResourceType resourceType = parseResourceType(opts.resourceType);
                        PackageManager.listDependencies(
                            resourceType,
                            opts.resourceName,
                            opts.version
                        );
                    }
                    catch (InvalidResourceTypeException)
                    {
                        System.Console.WriteLine($"Invalid resource type \"{opts.resourceType}\"");
                    }
                });
        }
    }
}
