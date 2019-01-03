using CommandLine;
using FSOpsNS;
using System.Collections.Generic;
using PackageManagerNS;
using CommandLine.Text;
using Constants;

namespace ny_cli {
    [Verb("init", HelpText = "Initialize code, data and model folders.")]
    class InitOptions {}

    [Verb("add", HelpText = "Add resource")]
    class AddOptions {
        [Value(0, Required = true, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public ResourceType resourceType {get;set;}

        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples {get {return new List<Example>() {
            new Example("Add a data resource", new AddOptions { resourceType = ResourceType.data, resourceName = "example_data_resource_name" })
        };}}
    }

    [Verb("remove", HelpText = "Remove resource")]
    class RemoveOptions {
        [Value(0, Required = true, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public ResourceType resourceType {get;set;}

        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples {get {return new List<Example>() {
            new Example("Add a data resource", new RemoveOptions { resourceType = ResourceType.model, resourceName = "example_model_resource_name" })
        };}}
    }

    [Verb("list", HelpText = "List packages")]
    class ListOptions {
        public ResourceType? nullableResourceType = null;
        
        [Value(0, Required = false, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public ResourceType resourceType {
            get {
                return nullableResourceType.Value;
            }
            set {
                nullableResourceType = value;
            }
        }

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples {get { return new List<Example>() {
            new Example("List model resources", new ListOptions { resourceType = ResourceType.model}),
        };}}
    }

    class Program {
        static void Main(string[] args) {
            Parser parser = new Parser(settings => {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = System.Console.Error;
            });

            parser.ParseArguments<InitOptions, AddOptions, RemoveOptions, ListOptions>(args)
                .WithParsed<InitOptions>(opts => {
                    PackageManager.initDirectories();
                })
                .WithParsed<ListOptions>(opts => {
                    PackageManager.listPackages(
                        opts.nullableResourceType
                    );
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
                });
        }
    }
}
