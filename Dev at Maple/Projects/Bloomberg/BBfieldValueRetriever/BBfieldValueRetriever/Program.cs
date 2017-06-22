using System;
using System.Windows.Forms;

namespace BBfieldValueRetriever
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1(args));
        }
    }

    /// <summary>
    /// A custom EventArgs class to hold any extra data that we require
    /// The name of this class can be anything but should end in "EventArgs" for clarity
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        public string Message;

        public MessageEventArgs(string Message)
        {
            // When this EventArgs class is created it is created with the required arguments
            // and we stored them so that the class recieving the event gets the data
            this.Message = Message;
        }
    }

    /// <summary>
    /// A delegate with the event signature
    /// The name of this delegate can be anything but should end in "EventDelegate" for clarity
    /// It's signature is the object of the sender and our own custom event args class
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void MessageEventDelegate(object sender, MessageEventArgs e);
}