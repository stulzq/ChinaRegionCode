using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ChinaRegionCode
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new HttpClient();
            var html = await client.GetStringAsync("http://www.mca.gov.cn/article/sj/xzqh/2020/2020/2020112010001.html");

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var title = doc.DocumentNode.SelectSingleNode("//tr[1]").InnerText.Trim().Replace("\n", "").Replace("\r", "");
            Console.WriteLine($"正在抓取 {title} ...");

            var nodes = doc.DocumentNode.SelectNodes("//tr[@height='19']");

            var result = new List<AreaModel>();
            //解析数据 xpath
            foreach (var node in nodes)
            {
                var code = node.SelectSingleNode("td[2]").InnerText.Trim();
                var name = node.SelectSingleNode("td[3]").InnerText.Replace(((char)160).ToString(), " ");

                if (name[0] != ' ')
                {
                    result.Add(new AreaModel() { Code = code, Name = name.TrimStart(), ParentCode = "100000" });
                }
                else
                {
                    var step = 10;
                    AreaModel parent = null;
                    var codeInt = int.Parse(code);
                    while (step < 10000 & parent == null)
                    {
                        step *= 10;
                        var parentCode = codeInt / step * step;
                        parent = result.FirstOrDefault(a => a.Code == parentCode.ToString());
                    }

                    if (parent == null)
                    {
                        throw new Exception($"未能找到父节点 {code}");
                    }

                    result.Add(new AreaModel() { Code = code, Name = name.TrimStart(), ParentCode = parent.Code });
                }
            }

            //写入数据
            await File.WriteAllTextAsync("region_three.json", JsonConvert.SerializeObject(result, Formatting.Indented));

            //生成 ant design 级联选择格式 https://ant.design/components/cascader-cn/s
            var rootAntNode=new AntAreaModel(){Value = "100000"};
            ResolveAntArea(rootAntNode, result);

            await File.WriteAllTextAsync("region_antd.json", JsonConvert.SerializeObject(rootAntNode.Children, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

            Console.WriteLine("完成");
        }

        /// <summary>
        /// 递归解析生成 Tree
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="data"></param>
        static void ResolveAntArea(AntAreaModel parentNode, List<AreaModel> data)
        {
            foreach (var areaModel in data.Where(a => a.ParentCode == parentNode.Value))
            {
                var item = new AntAreaModel() { Label = areaModel.Name, Value = areaModel.Code };
                ResolveAntArea(item, data);
                parentNode.Children.Add(item);
            }
        }

    }

    class AreaModel
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string ParentCode { get; set; }
    }

    class AntAreaModel
    {
        public AntAreaModel()
        {
            Children = new List<AntAreaModel>();
        }
        public string Value { get; set; }
        public string Label { get; set; }

        public List<AntAreaModel> Children { get; set; }
    }
}
