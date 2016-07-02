using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VimeoPlayerDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Call with input URL and target file name as arguments");
                return;
            }

            string baseUrl = args[0];
            string targetFileName = args[1];

            Console.WriteLine("Downloading index");

            var content = DownloadString(baseUrl);

            var match = Regex.Match(content, "https://[^\"]*master.json[^\"]*");

            string masterUrl = match.Value;

            content = DownloadString(masterUrl);

            var jsonContent = (JObject)JToken.Parse(content);

            var videos = (JArray)jsonContent["video"];
            long largestBitrate = -1;
            JObject largest = null;

            foreach (JObject video in videos)
            {
                long bitrate = (long)video["bitrate"];
                if (largestBitrate == -1 || bitrate > largestBitrate)
                {
                    largestBitrate = bitrate;
                    largest = video;
                }
            }

            Console.WriteLine($"Found highest bitrate stream with bitrate {largestBitrate}");

            string downloadBaseUrl = JoinUrl(masterUrl, (string)jsonContent["base_url"]);
            string downloadUrl = JoinUrl(downloadBaseUrl, (string)largest["base_url"]);

            using (var target = File.Create(targetFileName))
            {
                var initSegment = Convert.FromBase64String((string)largest["init_segment"]);

                target.Write(initSegment, 0, initSegment.Length);

                foreach (JObject segment in (JArray)largest["segments"])
                {
                    Console.WriteLine($"Downloading segment {(string)segment["url"]}");

                    string segmentUrl = downloadUrl + (string)segment["url"];

                    DownloadBytes(segmentUrl, target);
                }
            }
        }

        private static string JoinUrl(string a, string b)
        {
            int pos = a.LastIndexOf('/');
            a = a.Substring(0, pos);

            foreach (string part in b.Split('/'))
            {
                if (part == "..")
                {
                    pos = a.LastIndexOf('/');
                    a = a.Substring(0, pos);
                }
                else
                {
                    a = a + "/" + part;
                }
            }

            return a;
        }

        private static string DownloadString(string baseUrl)
        {
            var request = (HttpWebRequest)WebRequest.Create(baseUrl);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static void DownloadBytes(string baseUrl, Stream target)
        {
            var request = (HttpWebRequest)WebRequest.Create(baseUrl);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    stream.CopyTo(target);
                }
            }
        }
    }
}
