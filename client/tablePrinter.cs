using System.Collections.Generic;
using System.Linq;

namespace TablePrinterNS
{
    public class TablePrinter : System.Collections.IEnumerable
    {
        public class ColumnCountException : System.Exception
        {
            public ColumnCountException()
            : base()
            {
            }
        }

        private List<string> columnNames = new List<string>();
        private List<int> columnWidths = new List<int>();
        
        private List<List<string>> rows = new List<List<string>>();
        
        public TablePrinter()
        {
        }
        
        public void Add(string columnName, int columnWidth)
        {
            columnNames.Add(columnName);
            columnWidths.Add(columnWidth);
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            yield break;
        }

        public void addLine(params string[] row)
        {
            if (row.Length != this.columnWidths.Count) throw new ColumnCountException();
            
            this.rows.Add(row.ToList());
            
            // expand table if necessary
            for (int i = 0; i < columnWidths.Count; i++) {
                if (row[i].Length > columnWidths[i]) {
                    columnWidths[i] = row[i].Length;
                }
            }
        }

        public void print()
        {
            string header = "";
            string horizontalLine = "";
            string headerSeparator = "";
            string horizontalLineSeparator = "";
            for (int col = 0; col < columnNames.Count; col++) {
                header += headerSeparator;
                horizontalLine += horizontalLineSeparator;
                
                header += columnNames[col].PadRight(columnWidths[col]);
                horizontalLine += new string('=', columnWidths[col]);
                
                headerSeparator = "|";
                horizontalLineSeparator = "+";
            }
            System.Console.WriteLine(header);
            System.Console.WriteLine(horizontalLine);
            // int sumWidth = columnWidths.Select(p => p.Value).Aggregate((val1, val2) => val1 + val2) + columnWidths.Count - 1;

            foreach (List<string> row in rows) {
                string line = "";
                string rowSeparator = "";
                
                for (int col = 0; col < columnWidths.Count; col++) {
                    line += rowSeparator;
                    rowSeparator = "|";
                    line += row[col].PadRight(columnWidths[col]);
                }
                
                System.Console.WriteLine(line);
            }
        }
    }
}