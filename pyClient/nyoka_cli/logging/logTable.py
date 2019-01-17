
class LogTable:
    def __init__(self, *args):
        self._headers = []
        self._rows = []
        pass
    
    def add_header(self, header_name, header_min_width = 0):
        header_min_width = max(len(header_name), header_min_width)
        self._headers.append( (header_name, header_min_width) )
        
        return self
    
    def add_row(self, *row_vals):
        self._rows.append(row_vals)
        return self
    
    def get_col_widths(self):
        col_widths = []
        for header_entry in self._headers:
            col_widths.append(header_entry[1])

        for row_values in self._rows:
            for i, row_string in enumerate(row_values):
                row_string_len = len(row_string)

                if len(col_widths) < i + 1:
                    col_widths.append(row_string_len)
                else:
                    if col_widths[i] < row_string_len:
                        col_widths[i] = row_string_len
        
        return col_widths
    
    def get_col_count(self):
        col_count = len(self._headers)

        for row in self._rows:
            if len(row) > col_count:
                col_count = len(row)
        
        return col_count
    
    def get_headers(self):
        headers = list(map(lambda header_val: header_val[0], self._headers))
        
        col_count = self.get_col_count()
        
        while len(headers) < col_count:
            headers.append("")
        
        return headers
    
    def get_rows(self):
        col_count = self.get_col_count()

        rows = self._rows.copy()
        for row in rows:
            while len(row) < col_count:
                row.append("")
        
        return rows