using Stimulsoft.Report;
using Stimulsoft.Report.Export;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace GetPdfApi
{
    public partial class Index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnChama_Click(object sender, EventArgs e)
        {
            string strTokken = Autentica("87e6754d-9e2a-4ee1-8b93-574360100a0f");
            byte[] byArquivos = ObterFaa(strTokken, "http://localhost:10961/api/atendimentos/GetFaa?", "37978", "8185");
            CarregaPdf(byArquivos);
        }

        #region Metedos
        protected string Autentica(string strCode)
        {
            try
            {
                string strUrlAut = "http://localhost:10961/token";

                if (string.IsNullOrEmpty(strUrlAut))
                {
                    return "error Settings : UrlApiToken não esta configurada no Web.Config";
                }

                var request = (HttpWebRequest)WebRequest.Create(strUrlAut);

                var postData = "grant_type=password";
                postData += "&username=" + strCode;
                postData += "&Scope=6";
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                var json_serializer = new JavaScriptSerializer();
                var strToken_list = (IDictionary<string, object>)json_serializer.DeserializeObject(responseString);
                return strToken_list["access_token"].ToString();
            }
            catch (Exception ex)
            {
                return "error na Autenticação :" + ex.Message;
            }
        }

        protected byte[] ObterFaa(string strTokken, string strUrl, string strPac, string strEorg)
        {
            try
            {
                byte[] arquivo = null;
                string strRequest = strUrl + "intPac=" + strPac + "&intEorg=" + strEorg;

                HttpWebRequest requestGet = WebRequest.Create(strRequest) as HttpWebRequest;

                requestGet.Method = "GET";
                requestGet.ContentType = "application/json; charset=utf-8";
                requestGet.Headers.Add("Authorization", "Bearer " + HttpUtility.UrlEncode(strTokken));

                HttpWebResponse responseGet;
                using (responseGet = requestGet.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(responseGet.GetResponseStream());
                    string results = reader.ReadToEnd();

                    var json_serializer = new JavaScriptSerializer();
                    var str = (IDictionary<string, object>)json_serializer.DeserializeObject(results);
                    arquivo = Encoding.ASCII.GetBytes(str["arquivo"].ToString());
                    return arquivo;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected void CarregaPdf(byte[] byArquivos)
        {
            try
            {
                StiReport stiRelatorio = new StiReport();
                StiPdfExportService service = new StiPdfExportService();
                MemoryStream stream = new MemoryStream();
                BinaryWriter Writer = null;
                string Name = @"C:\Users\wcfeitosa\Desktop\GetPdfApi\GetPdfApi\Relatorio\R1300130029.mrt";

                DataTable dtRelatorio = new DataTable();
                dtRelatorio.Columns.Add("dado", typeof(System.Byte[]));

                DataRow dtRow = dtRelatorio.NewRow();

                dtRow["dado"] = byArquivos;
                dtRelatorio.Rows.Add(dtRow);

                stiRelatorio.Load(Name);
                stiRelatorio.RegData(dtRelatorio);
                stiRelatorio.Compile();
                stiRelatorio.Render();

                service.ExportPdf(stiRelatorio, stream);
                service.Export(stiRelatorio, "MyReportTEste.pdf");



            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
    }
}