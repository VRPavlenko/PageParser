using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Parser;
using PageParser.Models;
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

            var allParentDivs = customParser.GetParentCarDivElementsList(document);

            List<CarEntity> allCars = new List<CarEntity>();
            
            foreach(var pn in allParentDivs)
            {
                allCars.AddRange(await customParser.CreateCarEntitysFromParentNode(pn));
            }

            

            Console.ReadKey();
        }
    }
}