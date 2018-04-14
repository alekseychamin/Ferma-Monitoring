using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FermaTelegram
{
    class LoadHtml
    {
        HttpClient client;

        public LoadHtml()
        {
            client = new HttpClient();
        }

        public async Task<string> GetContentHtml(string url)
        {
            string result = null;
            var response = await client.GetAsync(url);
            if ((response != null) && (response.StatusCode == HttpStatusCode.OK))
            {
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
    }
}
