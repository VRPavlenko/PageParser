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

            var temp = new CarEntity();

            //if (carsEntities[50] != null && carsEntities.Count > 0)
            //{
            //    temp = await parser.GetAllComplictationsForOneCar(carsEntities[50]);
            //}

            var allDataCars = parser.GetCarComplictationsIntoAllCars();



            Console.ReadKey();
        }
    }
}