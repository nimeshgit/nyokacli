using System.Collections.Generic;
using System.Linq;

// @TODO implement progress bars?
namespace CLIInterfaceNS
{
    public static class CLIInterface
    {
        private static System.ConsoleColor warningColor = System.ConsoleColor.Yellow;
        private static System.ConsoleColor errorBackroundColor = System.ConsoleColor.Red;
        private static System.ConsoleColor errorForegroundColor = System.ConsoleColor.Black;
        private static System.ConsoleColor tableHeaderColor = System.ConsoleColor.Yellow;
        private static System.ConsoleColor tableFrameColor = System.ConsoleColor.White;

        // Inherits from IEnumerable to enable object initializer
        public class PrintTable : System.Collections.IEnumerable
        {
            public class ColumnCountException : System.Exception
            {
                public ColumnCountException()
                : base()
                {
                }
            }

            internal List<string> columnNames = new List<string>();
            internal List<int> columnWidths = new List<int>();

            internal List<List<string>> rows = new List<List<string>>();

            public PrintTable()
            {
                // empty
            }

            // To enable object initializer
            public void Add(string columnName, int minWidth)
            {
                columnNames.Add(columnName);
                int maxWidth = columnName.Length > minWidth ? columnName.Length : minWidth;
                columnWidths.Add(maxWidth);
            }

            // To enable object initializer
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                yield break;
            }

            public void addRow(params string[] row)
            {
                if (row.Length != this.columnWidths.Count)
                {
                    throw new ColumnCountException();
                }

                this.rows.Add(row.ToList());

                // expand table if necessary
                for (int i = 0; i < columnWidths.Count; i++) {
                    if (row[i].Length > columnWidths[i]) {
                        columnWidths[i] = row[i].Length;
                    }
                }
            }
        }

        public static void logWarning(string message)
        {
            System.Console.ForegroundColor = warningColor;
            System.Console.Write($"WARNING: {message}");
            System.Console.ResetColor();
            System.Console.Write("\n");
        }

        public static void logError(string message)
        {
            System.Console.BackgroundColor = errorBackroundColor;
            System.Console.ForegroundColor = errorForegroundColor;
            System.Console.Write($"ERROR: {message}");
            System.Console.ResetColor();
            System.Console.Write("\n");
        }

        public static bool askYesOrNo(string question)
        {
            bool? response = null;

            while (!response.HasValue)
            {
                System.Console.Write(question + " [y/n] ");
                string input = System.Console.ReadLine();

                if (input.Trim().ToLower() == "y")
                {
                    response = true;
                }
                else if (input.Trim().ToLower() == "n")
                {
                    response = false;
                }
                else
                {
                    System.Console.WriteLine($"Invalid response \"{input.Trim()}\": type in \"y\" or \"n\"");
                }
            }

            return response.Value;
        }

        public static void logLine(string str)
        {
            System.Console.WriteLine(str);
        }

        public static void logTable(PrintTable table)
        {
            string horizontalLine = "";

            for (int col = 0; col < table.columnNames.Count; col++)
            {
                System.Console.ForegroundColor = tableFrameColor;

                if (col != 0)
                {
                    System.Console.Write("|");
                }

                System.Console.ForegroundColor = tableHeaderColor;

                System.Console.Write(table.columnNames[col].PadRight(table.columnWidths[col]));
            }

            System.Console.Write("\n");

            for (int col = 0; col < table.columnNames.Count; col++)
            {
                if (col != 0) horizontalLine += "+";
                horizontalLine += new string('=', table.columnWidths[col]);
            }

            System.Console.ForegroundColor = tableFrameColor;
            System.Console.WriteLine(horizontalLine);
            System.Console.ResetColor();

            foreach (List<string> row in table.rows)
            {
                for (int col = 0; col < table.columnWidths.Count; col++)
                {
                    System.Console.ForegroundColor = tableFrameColor;

                    if (col != 0) System.Console.Write("|");

                    System.Console.ResetColor();
                    System.Console.Write(row[col].PadRight(table.columnWidths[col]));
                }

                System.Console.Write("\n");
            }
        }
    }
}
