using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FermaTelegram
{
    class ParserHtml
    {
        LoadHtml load;
        IHtmlDocument documentHtml = null;
        HtmlParser domParser;

        string url = "";

        public delegate void SendErrorLog(string message);
        SendErrorLog _del;

        public void RegisterHandler(SendErrorLog del)
        {
            _del = del;
        }

        #region Properties
        public string URL
        {
            get
            {
                return url;
            }

            set
            {
                if (value != "")
                {
                    url = value;
                }
            }
        }
        #endregion

        public ParserHtml()
        {
            load = new LoadHtml();
            domParser = new HtmlParser();
        }

        public async Task MakeDocumentHtmlAsync()
        {
            if (url != "")
            {
                try
                {
                    var source = await load.GetContentHtml(url);
                    //Console.WriteLine(source);
                    documentHtml = await domParser.ParseAsync(source);
                    //Console.WriteLine(documentHtml.ToString());
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(DateTime.Now.ToString() + " :" + " Error in MakeDocumentHtmlAsync : " + ex.Message);
                    if (_del != null)
                        _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                }
            }
        }

        public string ParseBySelector(string selector)
        {
            string parsString = "";

            if (documentHtml != null)
            {
                var parsing = documentHtml.QuerySelector(selector);

                if (parsing != null)
                {
                    parsString = parsing.TextContent;
                }                
            }
            //Console.WriteLine(parsString);
            return parsString;
        }
    }
}
