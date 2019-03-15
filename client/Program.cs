using FSOpsNS;
using System.Collections.Generic;
using PackageManagerNS;
using Constants;
using CLIInterfaceNS;
using System.Linq;
using FileTypeInferenceNS;
using CLIParserNS;

namespace nyoka
{
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
                    PackageManager.addPackage(opts.prefix,opts.resourceIdentifier);
                })
                .withRemove(opts => {
                    PackageManager.removePackage(opts.resourceIdentifier);
                })
                .withAvailable(opts => {
                    PackageManager.listAvailableResources(opts.prefix,opts.resourceType);
                })
                .withDependencies(opts => {
                    PackageManager.listDependencies(opts.prefix,opts.resourceIdentifier);
                })
                .withPublish(opts => {
                    PackageManager.publishResource(opts.prefix,opts.resourceIdentifier, opts.deps);
                })
                .withRemote(opts => {
                    PackageManager.setRemoteServerAddress(opts.prefix,opts.webAddress);
                });

            // To return console color to normal if necessary and to add some space at the bottom of the output
            CLIInterface.logLine("");
        }
    }
}
