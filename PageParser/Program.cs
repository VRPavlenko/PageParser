using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Parser;
using PageParser.SiteParser;

namespace PageParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string strContent = "";

            CustomParser customParser = new CustomParser();

            strContent = customParser.GetPageStrContent(customParser.Config.HomePage);

            var document = await customParser.CreateDataDocument(strContent);

            //customParser.GetFirstLayerCarsData(document);

            var allParentDivs = customParser.GetParentCarDivElementsList(document);
            var childNodes = await customParser.GetChildCarNodes(allParentDivs[1]);

            await customParser.CreateCarEntitysFromParentNode(childNodes[0]);

            

            Console.ReadKey();
        }
    }
}