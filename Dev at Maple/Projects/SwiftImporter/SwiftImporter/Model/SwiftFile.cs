using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SwiftImporterUI.Model
{
    public class SwiftFile
    {
        public SwiftFile(string filePath)
        {
            ReadInFile(filePath);
            FileName = filePath;
        }

        #region Methods

        private void ReadInFile(string fileName)
        {
            string allLines = "";
            FileArray = null;

            if (!File.Exists(fileName)) //file not found
            {
                throw new FileNotFoundException("The filename, " + fileName + ", does not exist. \r\n");
            }

            using (StreamReader stream = new StreamReader(fileName, true))
            {
                allLines = stream.ReadToEnd();
            }

            string newLineTagMarker = "\n:";
            string statementEndMarker = "-}";

            FileArray = allLines.Split(new string[] { newLineTagMarker, statementEndMarker }, StringSplitOptions.RemoveEmptyEntries);  // split on each new tag that appears on a new line
        }


        
        public int CurrentLine {
            get { return currentLine; }
        }

        int currentLine = 0;
        public string NextStatementLine()
        {
            //Extract one line of text at a time from an input stream
            //Assumption: each line ends in a single \n character          

            string line = "";
            if (FileArray.Length > currentLine)  // as long as we are within the bounds, return the next line
            {
                line = FileArray[currentLine].ToString().Replace("\n", "");   // sometimes each statement line goes over more than 1 line (i.e. wraps)
                if ((currentLine + 1 < FileArray.Length && FileArray[currentLine].StartsWith("61:")) // Not at last line
                    && (FileArray[currentLine + 1].StartsWith("86:")))  // tag 86 contains additional description (assume here that it always comes after the 61 tag
                {
                    line = line + FileArray[currentLine + 1].Replace("86:", ".  ");  //take just the description part and append it
                    currentLine++;  // so we don't pick it up next time
                }

                // now remove any new lines
                line = line.Replace("\n", "");
                line = line.Replace("\r", "");
                line = line.Trim();
                line = ":" + line; // temporarily added colon as split function removes it and everything else relies on it being the first index
            }

            currentLine++;  //keep track of the index so we know which line we are on

            return line;
        }


        /// <summary> 
        /// Waits until a file can be opened with write permission 
        /// </summary> 
        public static void WaitReady(string fileName)
        {
            DateTime waitStartTime = DateTime.Now;
            while (true)
            {
                try
                {
                    using (Stream stream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (stream != null)
                        {
                            Trace.WriteLine(string.Format("Output file {0} ready.", fileName));
                            break;
                        }
                    }
                }
                catch (FileNotFoundException ex)
                {
                    Trace.WriteLine(string.Format("Output file {0} not yet ready ({1})", fileName, ex.Message));
                }
                catch (IOException ex)
                {
                    Trace.WriteLine(string.Format("Output file {0} not yet ready ({1})", fileName, ex.Message));
                }
                catch (UnauthorizedAccessException ex)
                {
                    Trace.WriteLine(string.Format("Output file {0} not yet ready ({1})", fileName, ex.Message));
                }

                Thread.Sleep(500);

                if (waitStartTime < DateTime.Now.AddMinutes(-15))
                    throw new Exception("File was not accessible within the time allocated.  Check whats up with the file:  " + fileName);
            }
        }


        #endregion


        #region Properties

        string fileName;
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }


        string[] fileArray;
        public string[] FileArray
        {
            get { return fileArray; }
            set { fileArray = value; }
        }

        #endregion
    }
}
