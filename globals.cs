using System;
using System.Collections.Generic;
using System.Text;

namespace PTI_Integration_Invoice
{
    class globals
    {
        //Printable Data Feed Declarations
        private static Uri printableURI_invoice = new Uri(@"https://services.printable.com/TRANS/0.9/Invoice.asmx");
        private static Uri printableURI_settlement = new Uri(@"https://services.printable.com/TRANS/0.9/Settlement.asmx");
        private static string printableToken = "4F3515138E8BAA6A8D968BAE7F486C34";

        //Connection String Declarations
        //private static string logicConnString = "Data Source=PLM;Initial Catalog=devLogic;User ID=FPGwebservice;Password=kissmygrits"; //DEV CONN STRING
        //private static string printableConnString = "Data Source=PLM;Initial Catalog=printable;User ID=FPGwebservice;Password=kissmygrits"; //DEV CONN STRING
        private static string logicConnString = "Data Source=SQL1;Initial Catalog=pLogic;User ID=FPGwebservice;Password=kissmygrits";
        private static string printableConnString = "Data Source=SQL1;Initial Catalog=printable;User ID=FPGwebservice;Password=kissmygrits";

        //Accessor Methods
        //Use these for access "private" global variables for data protection
        public static string get_logicConnString
        {
            get { return globals.logicConnString; }
        }

        public static string get_printableConnString
        {
            get { return globals.printableConnString; }
        }

        public static Uri get_printableURI_invoice
        {
            get { return globals.printableURI_invoice; }
        }

        public static Uri get_printableURI_settlement
        {
            get { return globals.printableURI_settlement; }
        }

        public static string get_printableToken
        {
            get { return globals.printableToken; }
        }
    }
}
