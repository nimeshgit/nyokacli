
using CommandLine;
using FSOpsNS;
using System.Collections.Generic;
using PackageManagerNS;

namespace ny_cli {
    internal static class ConstStrings {
        internal const string RESOURCE_TYPE_HINT = "Resource Type: \"code\", \"data\" or \"model\"";
    }
    [Verb("init", HelpText = "Initialize code, data and model folders.")]
    class InitOptions {
    }

    [Verb("add", HelpText = "Add resource")]
    class AddOptions {
        [Value(0, Required = true, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public ResourceType resourceType {get;set;}

        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}
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
    }
    
    class Program {
        static void Main(string[] args) {
            Parser parser = new Parser(settings => {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = System.Console.Error;
            });

            parser.ParseArguments<InitOptions, AddOptions, ListOptions>(args)
                .WithParsed<InitOptions>(opts => {
                    bool successful = FSOps.createCodeDataModelDirs(logExisting: true, logCreated: true, logError: true);
                })
                .WithParsed<AddOptions>(opts => {
                    PackageManager.addPackage(
                        opts.resourceType,
                        opts.resourceName
                    );
                })
                .WithParsed<ListOptions>(opts => {
                    PackageManager.listPackages(
                        opts.nullableResourceType
                    );
                });
        }
    }
}
