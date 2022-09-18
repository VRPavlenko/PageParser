using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Parser;
using PageParser.Entity;
using PageParser.SiteParser;

namespace PageParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            CustomParser parser = new CustomParser();

            var carsEntities = await parser.GetCarEntities();

            

            Console.ReadKey();
        }
    }
}