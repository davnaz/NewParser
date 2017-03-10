using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using System.Net;
using System.Web;

namespace NewParser
{

    class Program
    {
        private static string mainLink = "https://www.gumtree.com.au/s-property-for-rent/forrentby-ownr/c18364"; //start link for parsing
        private static string gumtreeBaselink = "https://www.gumtree.com.au"; //gumtree.com.au
        private static string dbConnectionString = "Data Source=NOTEBOOK;Initial Catalog=Offers;Integrated Security=True"; //string for database with Offers
        private static string logOnLink = "https://www.gumtree.com.au/t-login.html";
        private static string gumtreeLoginMail = "clfdynic5oep@mail.ru";  //логин и пароль менять тут
        private static string gumtreePassword = "megapass!";
        private static string authParams = "targetUrl=%2Ft-settings.html&likingAd=false&loginMail=" + HttpUtility.UrlPathEncode(gumtreeLoginMail) + "&password=" + HttpUtility.UrlPathEncode(gumtreePassword) + "&rememberMe=true&_rememberMe=on";  //POST Params
        private static Uri gumtreeUri = new Uri(gumtreeBaselink); //необходимое зло
        private static CookieContainer cookies;
        private static HtmlParser parser;
        private static SqlConnection dbConnection;
        static void Main(string[] args)
        {
            Init(out parser, out dbConnection);
            Authorizate();

        }

        private static void Authorizate()
        {
            //Заголовки запроса
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(logOnLink);
            req.Method = "POST";
            req.Timeout = 100000;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
            req.KeepAlive = true;
            req.Headers.Add(HttpRequestHeader.Cookie, "machId=8tjqFvgmDAzTpd7aZPQ6ko6Qkek23aNJuh6MQ0L01tSy8f416G_0VJPCZhLulwlCQA6Srh6YtwpbmZj8ZkxP_lYfWLBWVi9D6XJd; bs=%7B%22st%22%3A%7B%7D%7D; up=%7B%22ln%22%3A%22302987873%22%2C%22ls%22%3A%22l%3D0%26r%3D0%26sv%3DLIST%26sf%3Ddate%22%2C%22lsh%22%3A%22l%3D0%26c%3D18364%26r%3D0%26sv%3DLIST%26rentals.forrentby_s%3Downr%26sf%3Ddate%22%2C%22lbh%22%3A%22l%3D0%26c%3D18364%26r%3D0%26sv%3DLIST%26rentals.forrentby_s%3Downr%26sf%3Ddate%22%7D; __utmt_siteTracker=1; __gads=ID=5ae0f0d7c464bcf1:T=1489057452:S=ALNI_MaRx14q9pz3OlJD-agyLiPRPgJSQg; _gat=1; sc=k6fIThLZKoQTphDwQ4jV; __utma=160852194.1931519358.1489057448.1489057448.1489057448.1; __utmb=160852194.2.10.1489057448; __utmc=160852194; __utmz=160852194.1489057448.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); _ga=GA1.3.1931519358.1489057448; ki_t=1489057454836%3B1489057454836%3B1489057469432%3B1%3B2; ki_r=; _gali=btn-submit-login");
            //req.Connection = "keep-alive";
            //req.ContentLength = 110;
            req.Referer = "https://www.gumtree.com.au/t-login-form.html";
            req.Headers.Add("Cache-Control", "max-age=0");
            req.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            req.Host = "www.gumtree.com.au";
            req.Headers.Add("Origin","www.gumtree.com.au");
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.AllowAutoRedirect = false;

            byte[] sentData = Encoding.GetEncoding(1251).GetBytes(authParams);
            req.ContentLength = sentData.Length;
            System.IO.Stream sendStream = req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);
            sendStream.Close();
            System.Net.HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            
            string cookiesString = res.Headers["Set-Cookie"]; //получаем строку с печеньками
            List<string> values =new List<string>(cookiesString.Split(new[] { ';'}, StringSplitOptions.RemoveEmptyEntries)); //делаем массив из ключей и их значений по очереди

            Cookie c = new Cookie();
            for (int i = 0; i< values.Count;i++)
            {
                try
                {
                    if (String.Compare(values[i].Split('=')[0], "Domain", true) == 0)
                    {
                        c.Domain = values[i].Split('=')[1];
                    }
                    if (String.Compare(values[i].Split('=')[0], "sid2", true) == 0)
                    {
                        c.Name = "sid2";
                        c.Value = values[i].Split('=')[1];
                    }
                    if (String.Compare(values[i].Split('=')[0], "Path", true) == 0)
                    {
                        c.Path = values[i].Split('=')[1];
                    }
                    if (String.Compare(values[i].Split('=')[0], "Expires", true) == 0)
                    {
                        c.Expires = DateTime.Parse(values[i].Split('=')[1]);
                    }
                }
                catch
                {
                    continue;
                }
            }
            cookies.Add(gumtreeUri,c); //получили нашу куки, теперь у нас есть ИД сессии
        }//используем данные, которые указаны в начале класса для авторизации на сайте

        private static string GetHtml(string link) //получаем страницу с 
        {
            //а Теперь повторим запрос, но теперь вложим печеньку внутрь запроса с идентификатором сессии
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(link);
            req.Method = "GET";
            req.Timeout = 100000;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
            req.KeepAlive = true;
            req.Headers.Add(HttpRequestHeader.Cookie, "machId=8tjqFvgmDAzTpd7aZPQ6ko6Qkek23aNJuh6MQ0L01tSy8f416G_0VJPCZhLulwlCQA6Srh6YtwpbmZj8ZkxP_lYfWLBWVi9D6XJd; bs=%7B%22st%22%3A%7B%7D%7D; up=%7B%22ln%22%3A%22302987873%22%2C%22ls%22%3A%22l%3D0%26r%3D0%26sv%3DLIST%26sf%3Ddate%22%2C%22lsh%22%3A%22l%3D0%26c%3D18364%26r%3D0%26sv%3DLIST%26rentals.forrentby_s%3Downr%26sf%3Ddate%22%2C%22lbh%22%3A%22l%3D0%26c%3D18364%26r%3D0%26sv%3DLIST%26rentals.forrentby_s%3Downr%26sf%3Ddate%22%7D; __utmt_siteTracker=1; __gads=ID=5ae0f0d7c464bcf1:T=1489057452:S=ALNI_MaRx14q9pz3OlJD-agyLiPRPgJSQg; _gat=1; sc=k6fIThLZKoQTphDwQ4jV; __utma=160852194.1931519358.1489057448.1489057448.1489057448.1; __utmb=160852194.2.10.1489057448; __utmc=160852194; __utmz=160852194.1489057448.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); _ga=GA1.3.1931519358.1489057448; ki_t=1489057454836%3B1489057454836%3B1489057469432%3B1%3B2; ki_r=; _gali=btn-submit-login");
            req.Host = "www.gumtree.com.au";
            if(cookies != null)
            {
                req.CookieContainer = cookies; //самый важный пункт: сюда добавляем печеньку с идентификатором авторизации
            }            
            req.AllowAutoRedirect = true;
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            System.IO.Stream ReceiveStream = res.GetResponseStream();
            System.IO.StreamReader sr2 = new System.IO.StreamReader(ReceiveStream, Encoding.UTF8);
            //Кодировка указывается в зависимости от кодировки ответа сервера
            Char[] read = new Char[256];
            int count = sr2.Read(read, 0, 256);
            string htmlString = String.Empty;
            while (count > 0)
            {
                String str = new String(read, 0, count);
                htmlString += str;
                count = sr2.Read(read, 0, 256);
            }
            return htmlString;
        }

        private static void Init(out HtmlParser parser, out SqlConnection dbConnection)
        {
            parser = new HtmlParser(); //создание экземпляра парсера, он можнт быть использован несколько раз, т.е. для всей программы
            dbConnection = new SqlConnection(dbConnectionString); //bинициализация подключения к БД
            cookies = new CookieContainer();
        }

        private static bool OpenSqlConnection(SqlConnection conn) //Функцйия открытия соединения к БД
        {
            try
            {
                if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                conn.Open(); // Открыть
                return true;
            }
            catch (Exception ex) // Исключение
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
