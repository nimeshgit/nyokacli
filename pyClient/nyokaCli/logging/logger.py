
class Colors:
    CEND      = '\33[0m'
    CBOLD     = '\33[1m'
    CITALIC   = '\33[3m'
    CURL      = '\33[4m'
    CBLINK    = '\33[5m'
    CBLINK2   = '\33[6m'
    CSELECTED = '\33[7m'
    CBLACK  = '\33[30m'
    CRED    = '\33[31m'
    CGREEN  = '\33[32m'
    CYELLOW = '\33[33m'
    CBLUE   = '\33[34m'
    CVIOLET = '\33[35m'
    CBEIGE  = '\33[36m'
    CWHITE  = '\33[37m'
    CBLACKBG  = '\33[40m'
    CREDBG    = '\33[41m'
    CGREENBG  = '\33[42m'
    CYELLOWBG = '\33[43m'
    CBLUEBG   = '\33[44m'
    CVIOLETBG = '\33[45m'
    CBEIGEBG  = '\33[46m'
    CWHITEBG  = '\33[47m'
    CGREY    = '\33[90m'
    CRED2    = '\33[91m'
    CGREEN2  = '\33[92m'
    CYELLOW2 = '\33[93m'
    CBLUE2   = '\33[94m'
    CVIOLET2 = '\33[95m'
    CBEIGE2  = '\33[96m'
    CWHITE2  = '\33[97m'
    CGREYBG    = '\33[100m'
    CREDBG2    = '\33[101m'
    CGREENBG2  = '\33[102m'
    CYELLOWBG2 = '\33[103m'
    CBLUEBG2   = '\33[104m'
    CVIOLETBG2 = '\33[105m'
    CBEIGEBG2  = '\33[106m'
    CWHITEBG2  = '\33[107m'

class Logger:
    @staticmethod
    def log_line(lineString):
        print(Colors.CEND + lineString)
    
    @staticmethod
    def log_error(lineString):
        print(Colors.CRED2 + lineString + Colors.CEND)
    
    @staticmethod
    def _write_frame(frameStr):
        print(Colors.CWHITE2 + frameStr, end="")
    
    @staticmethod
    def _write_header(headerStr):
        print(Colors.CYELLOW2 + headerStr, end="")
    
    @staticmethod
    def _write(writeStr):
        print(Colors.CEND + writeStr, end="")
    
    @staticmethod
    def log_table(table, show_frame=True):
        frame_vertical = "|" if show_frame else " "
        frame_horizontal = "=" if show_frame else " "
        frame_cross = "+" if show_frame else " "
        
        col_widths = table.get_col_widths()
        headers = table.get_headers()

        # print header row
        for col_idx, header_str in enumerate(headers):
            Logger._write_frame(frame_vertical)
            Logger._write_header(header_str.ljust(col_widths[col_idx]))
        
        Logger._write_frame(frame_vertical)
        Logger.log_line("")

        # print horizontal line separating header from values
        for col_width in col_widths:
            Logger._write_frame(frame_cross)
            Logger._write_frame(frame_horizontal * col_width)
        
        Logger._write_frame(frame_cross)
        Logger.log_line("")

        # print table values
        for row_vals in table.get_rows():
            # print value row
            for col_idx, row_string in enumerate(row_vals):
                Logger._write_frame(frame_vertical)
                Logger._write( row_string.ljust(col_widths[col_idx]) )

            Logger._write_frame(frame_vertical)
            Logger.log_line("")
        
        Logger.log_line("")
