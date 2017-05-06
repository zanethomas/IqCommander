using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Mono.Options;

using IqFeed.Model;
using IqFeed.Commanders;

namespace IqFeed {
	class MainClass {
		static CommanderBase commander;
		private static Admin admin;
		private static string infile;
		private static string ignored;
		private static bool help;

		public static void Main(string[] args) {
			ProcessArgs(args);
			admin = new Admin();
			Task<bool> t = Task.Run(() => admin.Launch());

			t.Wait();

			if (t.Result == true) {
				if (infile != null) {
					commander = new FileCommander(infile);
				} else {
					commander = new ReadExecPrint();
					commander.Responses.Subscribe(
						(line) => Console.WriteLine($"data: {line}"),
						() => Console.WriteLine("Sequence Completed.")
					);
				}
				Task.Run(() => commander.Run(admin)).Wait();

			} else {
				Console.WriteLine("bad admin");
			}
		}

		private static void ProcessArgs(string[] args) {
			List<string> options;

			OptionSet option_set = new OptionSet()
				.Add("?|help|h", "Prints out the options.", option => help = option != null)
				.Add("i=|input-file=",
	   				"File with list of commands to execute.",
	   				option => infile = option)
				.Add("s=|server=|servername=|instance=|instancename=",
					 option => ignored = option);

			try {
				options = option_set.Parse(args);
				if (options.Contains("--infile") && infile == null) {
					show_help("no file specified for infile", option_set);
				}
				if (help) {
					show_help("help yourself", option_set);
				}
			} catch (OptionException e) {
				show_help(e.Message, option_set);
			}
		}
		public static void show_help(string message, OptionSet option_set) {
			Console.Error.WriteLine(message);
			option_set.WriteOptionDescriptions(Console.Error);
			Environment.Exit(-1);
		}
	}
}
