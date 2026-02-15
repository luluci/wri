using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wri.Interface.Office
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ClosedXMLIf
    {
        public ClosedXMLIf()
        {

        }

        public XLWorkbookIf Open(string path)
        {
            try
            {
                if (!System.IO.File.Exists(path))
                {
                    return null;
                }
                var wb = new ClosedXML.Excel.XLWorkbook(path);
                return new XLWorkbookIf(wb);

            }
            catch (Exception ex)
            {
                GlobalData.ErrorMessage = ex.Message;
                return null;
            }
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class XLWorkbookIf
    {
        public ClosedXML.Excel.XLWorkbook workbook { get; set; }

        public XLWorkbookIf(ClosedXML.Excel.XLWorkbook wb)
        {
            workbook = wb;
        }

        [System.Runtime.CompilerServices.IndexerName("Worksheets")]
        public XLSheetIf this[string sheetName]
        {
            get
            {
                try
                {
                    var ws = workbook.Worksheet(sheetName);
                    return new XLSheetIf(ws);
                }
                catch (Exception ex)
                {
                    GlobalData.ErrorMessage = ex.Message;
                    return null;
                }
            }
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class XLSheetIf
    {
        public ClosedXML.Excel.IXLWorksheet worksheet { get; set; }

        public XLSheetIf(ClosedXML.Excel.IXLWorksheet ws)
        {
            worksheet = ws;
        }

        public XLRangeIf RangeUsed
        {
            get
            {
                try
                {
                    var range = worksheet.RangeUsed();
                    if (range is null)
                    {
                        return null;
                    }
                    return new XLRangeIf(range);
                }
                catch (Exception ex)
                {
                    GlobalData.ErrorMessage = ex.Message;
                    return null;
                }
            }
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class XLRangeIf
    {
        public ClosedXML.Excel.IXLRange range { get; set; }

        public XLRangeIf(ClosedXML.Excel.IXLRange r)
        {
            range = r;
        }

        public int Row
        {
            get { return range.FirstRow().RowNumber(); }
        }
        public int Column
        {
            get { return range.FirstColumn().ColumnNumber(); }
        }
        public int RowCount
        {
            get { return range.RowCount(); }
        }
        public int ColumnCount
        {
            get { return range.ColumnCount(); }
        }

        public object[] Get()
        {
            try
            {
                var result = range.Cells().Select(cell =>
                {
                    switch (cell.DataType)
                    {
                        case ClosedXML.Excel.XLDataType.Blank:
                            return (object)null;
                        case ClosedXML.Excel.XLDataType.Boolean:
                            return (object)cell.GetBoolean();
                        case ClosedXML.Excel.XLDataType.Number:
                            return (object)cell.GetDouble();
                        case ClosedXML.Excel.XLDataType.Text:
                            return (object)cell.GetText();
                        case ClosedXML.Excel.XLDataType.Error:
                            return (object)cell.GetError().ToString();
                        case ClosedXML.Excel.XLDataType.DateTime:
                            return (object)cell.GetDateTime().ToString();
                        case ClosedXML.Excel.XLDataType.TimeSpan:
                            return (object)cell.GetTimeSpan().ToString();
                        default:
                            return (object)cell.ToString();
                    }
                }).ToArray();
                return result;
            }
            catch (Exception ex)
            {
                GlobalData.ErrorMessage = ex.Message;
                return null;
            }
        }

        // Ramgeの値を2次元配列で取得する
        public object[,] Get2DArray()
        {
            try
            {
                int rows = range.RowCount();
                int cols = range.ColumnCount();
                object[,] result = new object[rows, cols];
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var cell = range.Cell(r + 1, c + 1);
                        result[r, c] = cell.Value;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                GlobalData.ErrorMessage = ex.Message;
                return null;
            }
        }
    }
}
