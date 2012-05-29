using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace PTI_Integration_Invoice
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("= " + DateTime.Now.AddDays(-1).ToString("MMMM dd, yyyy").ToUpper().PadLeft(17) + " - " + DateTime.Now.ToString("MMMM dd, yyyy").ToUpper().PadRight(17) + " =");
            Console.WriteLine("=========================================");

            ArrayList al = new ArrayList();     //NON-FG
            ArrayList al2 = new ArrayList();    //FG
            string invoiceNum = "";
            XmlDocument doc = new XmlDocument();
            methods me = new methods();
            string lineItemNum = "";
            
            //NON-FINISHED GOODS THAT WERE INVOICED
            al = me.getTodaysInvoices();

            foreach (object[] row in al)
            {
                Console.WriteLine("WIKey:" + row[0].ToString());
                for (int i = 1; i < row.Length; i++)
                {
                    Console.WriteLine("   -"+ row[i] as string);
                }
                
                invoiceNum = row[1].ToString();   //2nd column in vw_PTI_invoice
                lineItemNum = row[4].ToString();  //5th column in vw_PTI_invoice
                Console.WriteLine("Line Item ID: " + lineItemNum);
                
                doc = me.createInvoiceByLineItemRequest(invoiceNum, lineItemNum);
                me.parseResponse(me.sendXmlRequest(doc));
                
                try
                {
                    doc.Save("../../XML/" + invoiceNum + " - " + lineItemNum + "_req.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                doc.RemoveAll();

                int settlementNeeded = Convert.ToInt32(row[7]);

                if (settlementNeeded == 1)
                {
                    Console.WriteLine("CREDIT CARD SETTLEMENT FOR " + lineItemNum +"\n");
                    string orderID = me.getOrderID(lineItemNum);
                    XmlDocument d = me.createSettlementByOrderRequest(orderID);
                    me.parseResponse_settlement(me.sendXmlRequest_Settlement(d));
                    d.RemoveAll();
                }

                Console.WriteLine("========================================");
            }


            //FINISHED GOODS ORDERS THAT WERE INVOICED
            al2 = me.getTodaysFGInvoices();

            foreach (object[] row in al2)
            {
                Console.WriteLine("WIKey:" + row[0].ToString());
                for (int i = 0; i < row.Length; i++)
                {
                    Console.WriteLine("   -" + row[i] as string);
                }

                invoiceNum = row[2].ToString(); //3rd column in vw_PTI_invoiceFG
                string hold = row[6] as string; //8th column in vw_PTI_invoiceFG
                //string fgOrderNum = me.finGoodSubstring(hold);
                string FGwebOrderID = hold;
                string s = me.getOrderID(FGwebOrderID);
                Console.WriteLine("Web Order ID: " + hold /*FGwebOrderID*/ + "\n");

                doc = me.createInvoiceByLineItemRequest(invoiceNum, hold/*FGwebOrderID*/);
                me.parseResponse(me.sendXmlRequest(doc));
                doc.RemoveAll();

                if (me.needsSettlement(s))
                {
                    Console.WriteLine("CREDIT CARD SETTLEMENT FOR FG" + FGwebOrderID + "Ord:" + s + "\n");
                    XmlDocument d = me.createSettlementByOrderRequest(s);
                    me.parseResponse_settlement(me.sendXmlRequest_Settlement(d));
                    d.RemoveAll();
                }

                Console.WriteLine("========================================");
            }

            //                                                                                                  
            // MANUAL FG ORDER INVOICE                                                                          
            //                                                                                                  

            /*al2 = me.getTodaysFGInvoices();

            foreach (object[] row in al2)
            {
                Console.WriteLine("WIKey:" + row[0].ToString());
                for (int i = 0; i < row.Length; i++)
                {
                    Console.WriteLine("   -" + row[i] as string);
                }

                invoiceNum = "134428";
                string hold = row[6] as string; //8th column in vw_PTI_invoiceFG
                //string fgOrderNum = me.finGoodSubstring(hold);
                string FGwebOrderID = hold;
                string s = me.getOrderID(FGwebOrderID);
                Console.WriteLine("Web Order ID: " + hold  + "\n");

                doc = me.createInvoiceByLineItemRequest(invoiceNum, hold);
                me.parseResponse(me.sendXmlRequest(doc));
                doc.RemoveAll();

                if (me.needsSettlement(s))
                {
                    Console.WriteLine("CREDIT CARD SETTLEMENT FOR FG" + FGwebOrderID + "Ord:" + s + "\n");
                    XmlDocument d = me.createSettlementByOrderRequest(s);
                    me.parseResponse_settlement(me.sendXmlRequest_Settlement(d));
                    d.RemoveAll();
                }

                Console.WriteLine("========================================");
            }
            */
            //                                                                                                  
            //                                                                                                  
            //                                                                                                  
        }
    }
}
