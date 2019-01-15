using CommandLine;
using FSOpsNS;
using System.Collections.Generic;
using PackageManagerNS;
using CommandLine.Text;
using Constants;
using CLIInterfaceNS;
using System.Linq;
using FileTypeInferenceNS;
using CLIParserNS;

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
        
        static void Main(string[] args)
        {
            new CLIParser(args.ToList())
                .withInit(opts => {
                    PackageManager.initDirectories();
                })
                .withList(opts => {
                    PackageManager.listResources(opts.resourceType);
                })
                .withAdd(opts => {
                    PackageManager.addPackage(opts.resourceIdentifier);
                })
                .withRemove(opts => {
                    PackageManager.removePackage(opts.resourceIdentifier);
                })
                .withAvailable(opts => {
                    PackageManager.listAvailableResources(opts.resourceType);
                })
                .withDependencies(opts => {
                    PackageManager.listDependencies(opts.resourceIdentifier);
                })
                .withPublish(opts => {
                    PackageManager.publishResource(opts.resourceIdentifier, opts.deps);
                });
        }
    }
}
