//Copied from mal unity step 1.
//Modified by James Vanderhyde, 9 October 2021
//  Implemented Step 2.

using System;
using System.IO;
using Mal;

namespace Mal {
    class step1_read_print
    {
        // read
        static types.MalVal READ(string str) {
            return reader.read_str(str);
        }

        // eval
        static types.MalVal EVAL(types.MalVal ast, evaluator.Environment env) {
            return evaluator.eval_ast(ast,env);
        }

        // print
        static string PRINT(types.MalVal exp) {
            return printer.pr_str(exp);
        }

        static void Main(string[] args) {
            if (args.Length > 0 && args[0] == "--raw") {
                Mal.readline.mode = Mal.readline.Mode.Raw;
            }

            // repl loop
            while (true) {
                string line;
                try {
                    line = Mal.readline.Readline("user> ");
                    if (line == null) { break; }
                    if (line == "") { continue; }
                } catch (IOException e) {
                    Console.WriteLine("IOException: " + e.Message);
                    break;
                }
                try
                {
                    Console.WriteLine(PRINT(EVAL(READ(line),evaluator.baseEnvironment)));
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
