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
    class methods
    {
        public ArrayList getTodaysInvoices()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=======================================");
            Console.WriteLine("=   GATHERING TODAY'S INVOICED JOBS   =");
            Console.WriteLine("=======================================");

            ArrayList al = new ArrayList();
            al.Clear();

            string queryGetInvoices = @"SELECT *
                                        FROM printable.dbo.vw_PTI_invoice
                                        WHERE InvoiceDate > GETDATE()-4
                                        ORDER BY InvoiceDate";

            try
            {
                using (SqlConnection conn = new SqlConnection(globals.get_printableConnString))
                {
                    SqlCommand command = new SqlCommand(queryGetInvoices, conn);
                    try
                    {
                        command.Connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            object[] values = new object[reader.FieldCount];
                            reader.GetValues(values);
                            al.Add(values);
                        }

                        reader.Close();
                        conn.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return al;
        }

        public ArrayList getTodaysFGInvoices()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("");
            Console.WriteLine("========================================");
            Console.WriteLine("= GATHERING TODAY'S INVOICED FG ORDERS =");
            Console.WriteLine("========================================");

            ArrayList al = new ArrayList();
            al.Clear();

            string queryGetFGInvoices = @"select *
                                          from vw_PTI_invoiceFG
                                          where InvoiceDate > getdate()-4";

            try
            {
                using (SqlConnection conn = new SqlConnection(globals.get_printableConnString))
                {
                    SqlCommand command = new SqlCommand(queryGetFGInvoices, conn);
                    try
                    {
                        command.Connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            object[] values = new object[reader.FieldCount];
                            reader.GetValues(values);
                            al.Add(values);
                        }

                        reader.Close();
                        conn.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return al;
        }

        public ArrayList getDetails(string pWiKey)
        {
            string wiKey = pWiKey;

            ArrayList al = new ArrayList();
            al.Clear();

            string queryGetTheDeets = @"select i.InvoiceN, ij.Job, ijd.Description, j.Order_ID, j.OrderDetail_ID, j.PrintableID
                                        from pLogic.dbo.WinInvoice i
                                            join pLogic.dbo.WinInvJobs ij
                                                on i.WIKey = ij.WIKey
                                            join pLogic.dbo.WinInvJobDetail ijd
                                                on ij.WIKey = ijd.WIKey
                                            join pLogic.dbo.CT_Job j
                                                on ij.Job = j.JobN
                                        where i.WIKey = " + wiKey;

            try
            {
                using (SqlConnection conn = new SqlConnection(globals.get_logicConnString))
                {
                    SqlCommand command = new SqlCommand(queryGetTheDeets, conn);
                    try
                    {
                        command.Connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            object[] values = new object[reader.FieldCount];
                            reader.GetValues(values);
                            al.Add(values);
                        }

                        reader.Close();
                        conn.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return al;
        }

        public XmlDocument createInvoiceByLineItemRequest(string pInvoiceNum, string pLineItemID)
        {
            string invoiceNum = pInvoiceNum;
            string lineItemID = pLineItemID;
            XmlDocument outDoc = new XmlDocument();

            try
            {
                //PREFIX DECLARATIONS
                string soapPrefix = "soapenv";
                string ptiPrefix = "inv";
                string soapNamespace = @"http://schemas.xmlsoap.org/soap/envelope/";
                string ptiNamespace = @"http://www.printable.com/WebService/Invoice";

                //SOAP ENVELOPE CREATION
                XmlElement root = outDoc.CreateElement(soapPrefix, "Envelope", soapNamespace);
                root.SetAttribute("xmlns:soapenv", soapNamespace);
                root.SetAttribute("xmlns:inv", ptiNamespace);
                outDoc.AppendChild(root);

                //SOAP EMPTY HEADER CREATION
                XmlElement header = outDoc.CreateElement(soapPrefix, "Header", soapNamespace);
                root.AppendChild(header);

                //SOAP BODY CREATION
                #region START SOAP BODY CREATION
                XmlElement body = outDoc.CreateElement(soapPrefix, "Body", soapNamespace);

                XmlElement CreateInvoiceByLineItem = outDoc.CreateElement(ptiPrefix, "CreateInvoiceByLineItem", ptiNamespace);
                body.AppendChild(CreateInvoiceByLineItem);

                XmlElement pRequest = outDoc.CreateElement(ptiPrefix, "pRequest", ptiNamespace);
                CreateInvoiceByLineItem.AppendChild(pRequest);

                #region START PARTNER CREDENTIALS BLOCK
                XmlElement partnerCredentials = outDoc.CreateElement("PartnerCredentials");
                pRequest.AppendChild(partnerCredentials);

                XmlElement tokenTag = outDoc.CreateElement("Token");
                XmlText txt = outDoc.CreateTextNode(globals.get_printableToken);
                tokenTag.AppendChild(txt);
                partnerCredentials.AppendChild(tokenTag);
                #endregion END PARTNER CREDENTIALS BLOCK

                #region START INVOICE NODE BLOCK
                XmlElement invoiceNode = outDoc.CreateElement("InvoiceNode");
                pRequest.AppendChild(invoiceNode);

                XmlElement invoiceNumberTag = outDoc.CreateElement("InvoiceNumber");
                txt = outDoc.CreateTextNode(invoiceNum);
                invoiceNumberTag.AppendChild(txt);
                invoiceNode.AppendChild(invoiceNumberTag);

                #endregion END PACKING SLIP NODE BLOCK

                #region START LINE ITEMS BLOCK

                XmlElement lineItems = outDoc.CreateElement("LineItems");
                pRequest.AppendChild(lineItems);

                XmlElement lineItem = outDoc.CreateElement("LineItem");
                lineItems.AppendChild(lineItem);

                XmlElement idTag = outDoc.CreateElement("ID");
                idTag.SetAttribute("type", "Printable");
                txt = outDoc.CreateTextNode(pLineItemID);
                idTag.AppendChild(txt);
                lineItem.AppendChild(idTag);

                //Quantity Block for each line items

                #endregion END ORDERS BLOCK
                #endregion

                root.AppendChild(body);
                outDoc.Save(Console.Out);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return outDoc;
        }

        public XmlDocument sendXmlRequest(XmlDocument pDoc)
        {
            XmlDocument docResp = null;
            XmlDocument docReq = pDoc;

            HttpWebRequest objHttpWebRequest;
            HttpWebResponse objHttpWebResponse = null;

            Stream objRequestStream = null;
            Stream objResponseStream = null;

            XmlTextReader objXMLReader;

            objHttpWebRequest = (HttpWebRequest)WebRequest.Create(globals.get_printableURI_invoice);

            try
            {
                byte[] bytes;
                bytes = System.Text.Encoding.ASCII.GetBytes(docReq.InnerXml);
                objHttpWebRequest.Method = "POST";
                objHttpWebRequest.ContentLength = bytes.Length;
                objHttpWebRequest.ContentType = "text/xml; encoding='utf-8'";

                objRequestStream = objHttpWebRequest.GetRequestStream();

                objRequestStream.Write(bytes, 0, bytes.Length);

                objRequestStream.Close();

                objHttpWebResponse = (HttpWebResponse)objHttpWebRequest.GetResponse();

                if (objHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    objResponseStream = objHttpWebResponse.GetResponseStream();

                    objXMLReader = new XmlTextReader(objResponseStream);

                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(objXMLReader);

                    docResp = xmldoc;

                    objXMLReader.Close();
                }
                objHttpWebResponse.Close();
            }
            catch (WebException we)
            {
                Console.WriteLine(we.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                objRequestStream.Close();
                objResponseStream.Close();
                objHttpWebResponse.Close();

                objXMLReader = null;
                objRequestStream = null;
                objResponseStream = null;
                objHttpWebRequest = null;
                objHttpWebResponse = null;
            }

            docResp.Save(Console.Out);
            return docResp;
        }

        public void parseResponse(XmlDocument pDoc)
        {
            XmlDocument doc = pDoc;

            string errNum = "";
            string errDesc = "";
            string errStatus = "";
            string invIDnum = "";
            string lineItemIDnum = "";

            XmlNodeList respStatus = doc.GetElementsByTagName("Status");

            try
            {
                XmlNodeList invID = doc.GetElementsByTagName("ID");
                invIDnum = invID[0].Value;
                XmlNodeList lineItemID = doc.GetElementsByTagName("LineItemID");
                lineItemIDnum = lineItemID[0].Value;
            }
            catch { }

            if ((lineItemIDnum == "") || (lineItemIDnum == null))
            {
                lineItemIDnum = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            }

            try
            {
                doc.Save("../../XML/" + lineItemIDnum + "_resp.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            try
            {
                errNum = respStatus[0].Attributes["Code"].Value;
                errDesc = respStatus[0].Attributes["Message"].Value;
                errStatus = respStatus[0].Attributes["Status"].Value;
                
                


                //if (false) //DON'T LOG AN ERROR IF THE PROCESS COMPLETES SUCCESSFULLY
                //{
                //    responseError(errNum, errStatus, errDesc, "SUCCESS");
                //}
                //else
                //{
                    responseError(errNum, errDesc, errStatus, lineItemIDnum);
                //}

            }
            catch (Exception e)
            {
                //pDoc.Save(DateTime.Now.ToString() + "_error.xml");
                responseError("9999", "Parse Error", e.ToString(), "");
            }
        }

        public void responseError(string pErrNum, string pErr, string pErrDesc, string pInvoiceNum)
        {
            string errNum = "INV-" + pErrNum;
            string errDesc = pErrDesc;
            string err = pErr;
            string errDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            string invoiceNum = pInvoiceNum;
            string printableID = "0";

            string queryString = @"INSERT INTO CT_PTI_errorLog (errNum, errDesc, errMsg, errDate, printableID, packingSlipNum )
                                   VALUES ('" + errNum + "','" + errDesc + "','" + err + "','" + errDate + "','" + printableID + "','" + invoiceNum + "')";

            try
            {
                using (SqlConnection conn = new SqlConnection(globals.get_printableConnString))
                {
                    SqlCommand command = new SqlCommand(queryString, conn);
                    try
                    {
                        int rowsInserted = 0;
                        command.Connection.Open();
                        rowsInserted = command.ExecuteNonQuery();
                        command.Dispose();
                        command = null;
                        Console.WriteLine("");
                        Console.WriteLine(rowsInserted.ToString() + " row inserted into error log");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public string finGoodSubstring(string pField)
        {
            string result = pField.ToUpper();

            try { result = result.Remove(0, result.LastIndexOf("ORDER") + 6); }
            catch { }
            
            try { result = result.Remove(result.LastIndexOf("WAYBILL:")); }
            catch { }

            try { result = result.Remove(result.LastIndexOf(":")); }
            catch { }

            result = result.Trim();
            Console.WriteLine("Trimmed Order#: " + result);
            return result;
        }

        public string finGoodWebOrderID(string pOrderNum)
        {
            string orderNum = pOrderNum;
            string webOrderID = "";

            string queryGetWebOrderID = @"select isnull(Order_ID, 0)
                                          from printable.dbo.OrderDetails
                                          where OrderDetail_ID = " + orderNum;

            try
            {
                using (SqlConnection conn = new SqlConnection(globals.get_logicConnString))
                {
                    SqlCommand command = new SqlCommand(queryGetWebOrderID, conn);
                    try
                    {
                        command.Connection.Open();
                        string temp = (string)command.ExecuteScalar();

                        webOrderID = temp.ToString();
                    
                        conn.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return webOrderID;
        }
    
        //CREDIT CARD SETTLEMENT ADDITIONS
        public bool needsSettlement(string pOrderID)
        {
            //CHECK THE DATABASE TO SEE IF THE ORDER NEEDS SETTLED OR NOT
            string orderID = pOrderID;
            string paymentMethod = "";
            bool result;

            string queryCreditCardOrder = @"select PaymentMethod
                                            from printable.dbo.Orders
                                            where Order_ID = '" + orderID + "'";

            try
            {
                using (SqlConnection conn = new SqlConnection(globals.get_printableConnString))
                {
                    SqlCommand command = new SqlCommand(queryCreditCardOrder, conn);
                    try
                    {
                        command.Connection.Open();
                        paymentMethod = (string)command.ExecuteScalar();

                        conn.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            if (paymentMethod == "CreditCard") //SETTLEMENT NEEDED TEST
            {
                result = true;
                Console.WriteLine("Settlement needed");
            }
            else
            {
                result = false;
                Console.WriteLine("No settlement needed");
            }

            return result;
        }

        public string getOrderID(string pLineItemID)
        {
            string lineItemID = pLineItemID;
            string orderID = "";
            string queryOrderID = @"select Order_ID
                                    from printable.dbo.OrderDetails
                                    where OrderDetail_ID = " + lineItemID;

            try
            {
                using (SqlConnection conn = new SqlConnection(globals.get_printableConnString))
                {
                    SqlCommand command = new SqlCommand(queryOrderID, conn);
                    try
                    {
                        command.Connection.Open();
                        orderID = (string) command.ExecuteScalar();
                        
                        conn.Close();
                    }
                    catch (Exception e)
                    {
                        orderID = "";
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return orderID;
        }

        public XmlDocument createSettlementByOrderRequest(string pOrderID)
        {

            string orderID = pOrderID;
            XmlDocument outDoc = new XmlDocument();

            try
            {
                //PREFIX DECLARATIONS
                string soapPrefix = "soapenv";
                string ptiPrefix = "set";
                string soapNamespace = @"http://schemas.xmlsoap.org/soap/envelope/";
                string ptiNamespace = @"http://www.printable.com/WebService/Settlement";

                //SOAP ENVELOPE CREATION
                XmlElement root = outDoc.CreateElement(soapPrefix, "Envelope", soapNamespace);
                root.SetAttribute("xmlns:soapenv", soapNamespace);
                root.SetAttribute("xmlns:set", ptiNamespace);
                outDoc.AppendChild(root);

                //SOAP EMPTY HEADER CREATION
                XmlElement header = outDoc.CreateElement(soapPrefix, "Header", soapNamespace);
                root.AppendChild(header);

                //SOAP BODY CREATION
                #region START SOAP BODY CREATION
                XmlElement body = outDoc.CreateElement(soapPrefix, "Body", soapNamespace);

                XmlElement CreateSettlement = outDoc.CreateElement(ptiPrefix, "SettleByOrder", ptiNamespace);
                body.AppendChild(CreateSettlement);

                XmlElement pRequest = outDoc.CreateElement(ptiPrefix, "pRequest", ptiNamespace);
                CreateSettlement.AppendChild(pRequest);

                XmlElement settle = outDoc.CreateElement("SettleOnline");
                XmlText txt = outDoc.CreateTextNode("true");
                settle.AppendChild(txt);
                pRequest.AppendChild(settle);

                #region START PARTNER CREDENTIALS BLOCK
                XmlElement partnerCredentials = outDoc.CreateElement("PartnerCredentials");
                pRequest.AppendChild(partnerCredentials);

                XmlElement tokenTag = outDoc.CreateElement("Token");
                txt = outDoc.CreateTextNode(globals.get_printableToken);
                tokenTag.AppendChild(txt);
                partnerCredentials.AppendChild(tokenTag);
                #endregion END PARTNER CREDENTIALS BLOCK

                XmlElement idTag = outDoc.CreateElement("OrderID");
                idTag.SetAttribute("type", "Printable");
                txt = outDoc.CreateTextNode(orderID);
                idTag.AppendChild(txt);
                pRequest.AppendChild(idTag);
                #endregion

                root.AppendChild(body);
                Console.WriteLine("");
                outDoc.Save(Console.Out);
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return outDoc;
        }

        public void responseErrorSettlement(string pErrNum, string pErr, string pErrDesc)
        {
            string errNum = "SET-" + pErrNum;
            string errDesc = pErrDesc;
            string err = pErr;
            string errDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            string nullNum = "0";
            string printableID = "0";

            string queryString = @"INSERT INTO CT_PTI_errorLog (errNum, errDesc, errMsg, errDate, printableID, packingSlipNum )
                                   VALUES ('" + errNum + "','" + errDesc + "','" + err + "','" + errDate + "','" + printableID + "','" + nullNum + "')";

            try
            {
                using (SqlConnection conn = new SqlConnection(globals.get_printableConnString))
                {
                    SqlCommand command = new SqlCommand(queryString, conn);
                    try
                    {
                        int rowsInserted = 0;
                        command.Connection.Open();
                        rowsInserted = command.ExecuteNonQuery();
                        command.Dispose();
                        command = null;
                        Console.WriteLine("");
                        Console.WriteLine(rowsInserted.ToString() + " row inserted into error log");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void parseResponse_settlement(XmlDocument pDoc)
        {
            XmlDocument doc = pDoc;

            string errNum = "";
            string errDesc = "";
            string errStatus = "";

            XmlNodeList respStatus = doc.GetElementsByTagName("Status");

            try
            {
                for (int i = 0; i < respStatus.Count; i++)
                {
                    errNum = respStatus[i].Attributes["Code"].Value;
                    errDesc = respStatus[i].Attributes["Message"].Value;
                    errStatus = respStatus[i].Attributes["Status"].Value;
                }

                int num = Convert.ToInt32(errNum);

                responseErrorSettlement(errNum, errStatus, errDesc);
            }
            catch
            {
                responseErrorSettlement("9999", "Parse Error", "Invoice response parsing error, check methods.cs");
            }
        }

        //SHOULD UPDATE THE MATCH SIMILAR METHOD ABOVE WITH ADDITIONAL ARGUMENT
        public XmlDocument sendXmlRequest_Settlement(XmlDocument pDoc)
        {
            XmlDocument docResp = null;
            XmlDocument docReq = pDoc;

            HttpWebRequest objHttpWebRequest;
            HttpWebResponse objHttpWebResponse = null;

            Stream objRequestStream = null;
            Stream objResponseStream = null;

            XmlTextReader objXMLReader;

            objHttpWebRequest = (HttpWebRequest)WebRequest.Create(globals.get_printableURI_settlement);

            try
            {
                byte[] bytes;
                bytes = System.Text.Encoding.ASCII.GetBytes(docReq.InnerXml);
                objHttpWebRequest.Method = "POST";
                objHttpWebRequest.ContentLength = bytes.Length;
                objHttpWebRequest.ContentType = "text/xml; encoding='utf-8'";

                objRequestStream = objHttpWebRequest.GetRequestStream();

                objRequestStream.Write(bytes, 0, bytes.Length);

                objRequestStream.Close();

                objHttpWebResponse = (HttpWebResponse)objHttpWebRequest.GetResponse();

                if (objHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    objResponseStream = objHttpWebResponse.GetResponseStream();

                    objXMLReader = new XmlTextReader(objResponseStream);

                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(objXMLReader);

                    docResp = xmldoc;

                    objXMLReader.Close();
                }
                objHttpWebResponse.Close();
            }
            catch (WebException we)
            {
                Console.WriteLine(we.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if (objRequestStream != null)
                    objRequestStream.Close();
                if (objResponseStream != null)
                    objResponseStream.Close();
                if (objHttpWebResponse != null)
                    objHttpWebResponse.Close();

                objXMLReader = null;
                objRequestStream = null;
                objResponseStream = null;
                objHttpWebRequest = null;
                objHttpWebResponse = null;
            }
            Console.WriteLine("");
            if (docResp != null)
                docResp.Save(Console.Out);
            Console.WriteLine("");
            return docResp;
        }
    }
}
