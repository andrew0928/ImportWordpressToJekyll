using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace ConsoleApplication4
{
    class Program
    {

        static string wpxml = @"d:\HomeDisk\BlogWork\wordpress.2016-09-01.xml";
        static string output = @"D:\HomeDisk\BlogWork\columns\_posts";

        static void Main(string[] args)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(wpxml);

            XmlNamespaceManager xnm = new XmlNamespaceManager(xmldoc.NameTable);
            xnm.AddNamespace("excerpt", "http://wordpress.org/export/1.2/excerpt/");
            xnm.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
            xnm.AddNamespace("wfw", "http://wellformedweb.org/CommentAPI/");
            xnm.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            xnm.AddNamespace("wp", "http://wordpress.org/export/1.2/");

            if (Directory.Exists(output)) Directory.Delete(output, true);
            Directory.CreateDirectory(output);

            int count = 0;
            foreach (XmlElement item in xmldoc.DocumentElement["channel"].SelectNodes("item"))
            {
                DateTime pubdate;
                if (DateTime.TryParse(item["pubDate"].InnerText, out pubdate) == false)
                {
                    Console.WriteLine($"error: {item["pubDate"].InnerText}");
                }

                string link = item["link"].InnerText;
                string title = item["title"].InnerText;
                string post_type = item.SelectSingleNode("wp:post_type", xnm).InnerText;
                string post_name = item.SelectSingleNode("wp:post_name", xnm).InnerText;
                string status = item.SelectSingleNode("wp:status", xnm).InnerText;
                string content = item.SelectSingleNode("content:encoded", xnm).InnerText;

                string tags = null;
                string[] categories = null;

                {
                    List<string> temp = new List<string>();
                    foreach (XmlElement cat in item.SelectNodes("category"))
                    {
                        if (temp.Count > 0) tags += ",";
                        tags += JString(cat.InnerText);
                        temp.Add(cat.InnerText);
                    }
                    categories = temp.ToArray();
                    temp = null;
                }



                string permalink = //$"/{pubdate:yyyy}/{pubdate:mm}/{pubdate:dd}/{post_name}/";
                    FixUrlText(new Uri(link).PathAndQuery);
                string filename = $"{pubdate:yyyy}-{pubdate:MM}-{pubdate:dd}-{FixUrlText(post_name)}.html";

                if (post_type != "post") continue;
                if (status != "publish") continue;

                Console.WriteLine($"post:        {title}");
                Console.WriteLine($"- filename:  {filename}");
                Console.WriteLine($"--------------------------------------------------------------");

                count++;

                // generate _/posts/*.html
                /*
                ---
                layout: post
                title: Battlestar Galactica
                categories:
                -"有的沒的"
                tags: []
                published: true
                comments: true
                ---
                <!-- html content here -->
                */
                TextWriter tw = new StreamWriter(
                    Path.Combine(output, filename),
                    false,
                    new UTF8Encoding(false));
                tw.WriteLine($"---");
                tw.WriteLine($"layout: post");
                tw.WriteLine($"title: {JString(title)}");
                tw.WriteLine($"categories:");
                foreach (string cat in categories)
                {
                    tw.WriteLine($"- {JString(cat)}");
                }
                tw.WriteLine($"tags: [{tags}]");
                tw.WriteLine($"published: true");
                tw.WriteLine($"comments: true");
                tw.WriteLine($"permalink: {JString(permalink)}");
                tw.WriteLine($"---");
                tw.Write(content);
                tw.Close();
            }

            Console.WriteLine($"Total Post: {count}");
        }

        /// <summary>
        /// convert string content to JScript string (source code)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static string JString(string text)
        {
            return
                '"' +
                $"{text}"
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"") +
                '"';
        }

        /// <summary>
        /// do URL decode %11%11 parts of text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static string FixUrlText(string text)
        {
            return HttpUtility.UrlDecode(text);
        }
    }
}
