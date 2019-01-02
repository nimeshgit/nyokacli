
using CommandLine;

namespace ny_cli {
    [Verb("add", HelpText = "Add file contents to the index.")]
    class AddOptions {
        //normal options here
    }
    [Verb("commit", HelpText = "Record changes to the repository.")]
    class CommitOptions {
        //commit options here
    }
    [Verb("clone", HelpText = "Clone a repository into a new directory.")]
    class CloneOptions {
        //clone options here
    }
    
    class Program {
        // internal class NyokaOptions {
        //     [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        //     public bool Verbose { get; set; }
        // }
        
        static void Main(string[] args) {
            Parser.Default.ParseArguments<AddOptions, CommitOptions, CloneOptions>(args)
                .MapResult(
                    (AddOptions opts) => {
                        System.Console.WriteLine("Add");
                        return 1;
                    },
                    (CommitOptions opts) => {
                        System.Console.WriteLine("Commit");
                        return 1;
                    },
                    (CloneOptions opts) => {
                        System.Console.WriteLine("Clone");
                        return 1;
                    },
                    errs => 1
                );
        }
    }
}
