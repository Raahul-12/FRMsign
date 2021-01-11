using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Image = iTextSharp.text.Image;

namespace FRMsign
{
    class Program
    {
        static void Main(string[] args)
        {
            PdfReader pdfReaderLocal = null;
            FileStream fout = null;
            float RectRight = 0; float RectLeft = 0; float RectTop = 0; float RectBottom = 0;

            try
            {
                bool isVisibleSignature = true;
                int noOfPage = 0;
                bool iscert = false;
                X509Certificate2 mcert = null;
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certificates = store.Certificates;
                if (certificates.Count == 0)
                {

                    
                    store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                }
                foreach (X509Certificate2 certs in certificates)
                {
                    if (certs.GetName().Contains("Exalca DS Ver2.0"))
                    {
                        iscert = true;
                        mcert = certs;
                        Console.WriteLine("found cert Exalca DS Ver2.0");
                    }
                }
                //string Internal_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Signed_Doc") + "\\" + InvoiceName;
                string inptfldr = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InputFldr");
                
                if (!Directory.Exists(inptfldr))
                {
                    Directory.CreateDirectory(inptfldr);
                }

                string outfldr = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OutputFldr");
                if (!Directory.Exists(outfldr))
                {
                    Directory.CreateDirectory(outfldr);
                }
                if (iscert)
                {
                    string Sign_Location = "Bengaluru";
                    string Sign_AllPages = "N";
                    X509Certificate2 cert = mcert; //get certificate based on thumb print

                    Org.BouncyCastle.X509.X509CertificateParser cp = new Org.BouncyCastle.X509.X509CertificateParser();
                    Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] {
                cp.ReadCertificate(cert.RawData)};

                    IExternalSignature externalSignature = new X509Certificate2Signature(cert, "SHA1");
                    //PdfReader pdfReader = new PdfReader(sourceDocument);
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    //ErrorLog.WriteHistoryLog("Open for write");
                    //signatureAppearance.SignatureGraphic = Image.GetInstance(pathToSignatureImage);
                    //signatureAppearance.Reason = reason; signpdf method
                    pdfReaderLocal = new PdfReader(inptfldr + "//s.pdf");
                    noOfPage = pdfReaderLocal.NumberOfPages;
                  
                    iTextSharp.text.Rectangle mediabox = pdfReaderLocal.GetPageSize(1);
                    fout = new FileStream(outfldr+"\\123.pdf", FileMode.Append, FileAccess.Write);
                    PdfStamper stamper = PdfStamper.CreateSignature(pdfReaderLocal, fout, '\0', null, true);
                    PdfSignatureAppearance signatureAppearance = stamper.SignatureAppearance;
                   
                    signatureAppearance.ReasonCaption = "";
                    signatureAppearance.Reason = "Exalca";
                    signatureAppearance.LocationCaption = "";
                    
                    signatureAppearance.Location = "Bengaluru";
                    signatureAppearance.Acro6Layers = false;
                    signatureAppearance.Layer4Text = PdfSignatureAppearance.questionMark;

                    BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    signatureAppearance.Layer2Font = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);
                    var rec1 = new iTextSharp.text.Rectangle(610, 75, 440, 150);
                    signatureAppearance.SetVisibleSignature(rec1, 1, "Signature" + 1); //i-->Page no,Signature1--->Field name

                    MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, null, 0, CryptoStandard.CMS);
                    signatureAppearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;



                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (pdfReaderLocal != null)
                {
                    pdfReaderLocal.Close();
                    pdfReaderLocal.Dispose();
                }
                if (fout != null)
                {
                    fout.Close();
                    fout.Dispose();
                }
            }
        }

       
    }
}
