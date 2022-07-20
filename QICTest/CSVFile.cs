using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QICTest
{
    public class CSVFile
    {
        private char _separator;

        private string[] _columnHeaders;
        public string[] Colums => _columnHeaders;

        private List<string[]> _data = new List<string[]>();

        public long Length => _data.Count;
        

        public CSVFile(char separator)
        {
            _separator = separator;
        }
        


        public void Load(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                int lineNumber = 0;
                try
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        lineNumber++;
                        if (line.Length == 0)
                        {
                            continue; //skip the empty line
                        }
                        ParseLine(line);
                    }
                }
                catch (Exception exeption)
                {
                    throw new ApplicationException($"Parse CSV from stream failed. error detected in line {lineNumber}.", exeption);
                }
            }
        }

        
        private void ParseLine(string line)
        {
            // the first line is always treated as column head line. initialize this information first
            if (_columnHeaders == null)
            {
                _columnHeaders = line.Split(_separator).Select(t => t.Trim()).ToArray();
            }
            else
            {
                var data = line.Split(_separator);
                if (data.Length != _columnHeaders.Length)
                {
                    throw new ApplicationException("Invalid CSV data. The number of data in this row is diffrent from the colum definition. ");
                }
                _data.Add(data);
            }
        }

        public void WriteToStream(Stream fs)
        {
            using(StreamWriter sw = new StreamWriter(fs))
            {
                var first = true;
                foreach(var c in _columnHeaders)
                {
                    if (!first)
                    {
                        sw.Write(_separator);
                    }
                    else
                    {
                        first = false;
                    }
                    sw.Write(c);                    
                }

                foreach(var row in _data)
                {
                    first = true;
                    foreach(var d in row)
                    {
                        if (!first)
                        {
                            sw.Write(_separator);
                        }
                        else
                        {
                            first = false;
                        }
                        sw.Write(d);
                    }

                    sw.WriteLine();
                }
                
            }
        }


        public void SetColums(string[] colums)
        {
            _columnHeaders = colums;
        }
        public void AddData(string[] data)
        {
            if(_columnHeaders == null)
            {
                throw new ApplicationException("Please specify the columns before adding data to the CSV.");
            }

            if(data.Length != _columnHeaders.Length)
            {
                throw new ApplicationException("Invalid CSV data. The number of data in this row is diffrent from the colum definition. ");
            }
            _data.Add(data);
        }


        public T GetDataInCell<T>(int rowNumber, string columName)
        {
            int colNumber = Array.IndexOf(_columnHeaders, columName);
            if(colNumber < 0)
            {
                throw new ApplicationException($"Invalid colum name '{colNumber}'");
            }
            return GetDataInCell<T>(rowNumber, colNumber);
        }
        
        public T GetDataInCell<T>(int rowNumber, int columNumber)
        {
            if (rowNumber >= _data.Count || rowNumber < 0)
            {
                throw new ApplicationException("Row number is not in the valid range.");
            }
            if(columNumber >= _columnHeaders.Length || columNumber < 0)
            {
                throw new ApplicationException("Column number is not in the valid range.");
            }

            var v = _data[rowNumber][columNumber];
            return (T) Convert.ChangeType(v, typeof(T));
        }     
        
        
    }
}
