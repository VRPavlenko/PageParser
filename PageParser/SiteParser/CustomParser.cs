using System;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using PageParser.Entity;
using AngleSharp.Dom;
using System.Linq;

namespace PageParser.SiteParser
{
    public class CustomParser
    {
        #region Fields
        private Config config;
        private string homePageStrContent;
        #endregion Fields

        #region Properties
        public Config Config { get => config; set => config = value; }

        public string HomePageStrContent { get => homePageStrContent; set => homePageStrContent = value; }
        public List<CarEntity> CarsEntities;
        public List<string> SecondLayerUrlList;

        #endregion Properties

        public CustomParser()
        {
            Config = new Config();
            CarsEntities = new List<CarEntity>();
            SecondLayerUrlList = new List<string>();
        }


        #region UtilityMethods
        /// <summary>
        /// Получает cтроковый контент из html страницы используя её адрес.
        /// </summary>
        private string GetPageStrContent(string url)
        {
            var resultStr = "";
            var webRequest = WebRequest.Create(url);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                resultStr = reader.ReadToEnd();
            }

            return resultStr;
        }

        /// <summary>
        /// Создает объект типа IDocument из контента html страницы.
        /// </summary>
        public async Task<IDocument> CreateDataDocument(string content)
        {
            IConfiguration config = Configuration.Default;

            IBrowsingContext context = BrowsingContext.New(config);

            IDocument document = await context.OpenAsync(req => req.Content(content));

            return document;
        }

        /// <summary>
        /// Возвращает тип документ из типа елемент
        /// </summary>
        private async Task<IDocument> GetDocumentFromElement(IElement element)
        {
            var strContent = element.ToHtml();

            return await CreateDataDocument(strContent);
        }
        #endregion UtilityMethods

        #region FirstLvlMethods
        /// <summary>
        /// Получает все родительские html элементы с домашней страницы, которые содержат данные и ссылку на варианты модели машины.
        /// </summary>
        public List<IElement> GetParentCarDivElementsList(IDocument document)
        {
            var parenNodes = document.All.Where(el => el.LocalName == "div" &&
                                                el.HasAttribute("class") &&
                                                el.GetAttribute("class").StartsWith("List") &&
                                                el.Children.Any(chEl => chEl.LocalName == "div" &&
                                                chEl.HasAttribute("class") &&
                                                chEl.GetAttribute("class").StartsWith("Header")));
            return parenNodes.ToList();
        }

        /// <summary>
        /// Возвращает список элементов с моделями машин содержащихся в одном родительском елементе.
        /// </summary>
        public async Task<List<IElement>> GetChildCarNodes(IElement element)
        {
            var parentNode = await GetDocumentFromElement(element);

            var childNodes = parentNode.All.Where(el => el.LocalName == "div" &&
                                                    el.HasAttribute("class") &&
                                                    el.GetAttribute("class").StartsWith("List") &&
                                                    el.Children.Any(chEl => chEl.LocalName == "div" &&
                                                    chEl.HasAttribute("class") &&
                                                    chEl.GetAttribute("class").StartsWith("id")));
            var result = childNodes.ToList();
            return result;
        }

        /// <summary>
        /// Создает список обьектов CarEntity из родительского елемента
        /// </summary>
        public async Task<List<CarEntity>> CreateCarEntitysFromSingleNode(IElement parentElement)
        {
            //var parentNode = await GetDocumentFromElement(parentElement);

            List<CarEntity> carEntities = new List<CarEntity>();

            List<IElement> childrenNodes = new List<IElement>();

            childrenNodes = await GetChildCarNodes(parentElement);


            foreach (IElement el in childrenNodes)
            {
                var carEntity = new CarEntity();

                carEntity.Name = await GetModelName(parentElement); // get model name

                carEntity.Codes = await GetModelCodes(el); //get modelCodes

                carEntity.Id = await GetModelId(el); //get model Id

                DateTime? start, finish;
                GetModelStartDateAndFinishDate(el, out start, out finish); // get model start date
                carEntity.StartDate = start;
                carEntity.FinishDate = finish;

                carEntity.SecondLayerDataUrl = await GetSecondLvlUrl(el); // get url second lvl

                carEntities.Add(carEntity);
            }
            return carEntities;
        }

        /// <summary>
        /// Возвращает URL из дочернего елемента
        /// </summary>
        public async Task<string> GetSecondLvlUrl(IElement childElement)
        {
            var childNode = await GetDocumentFromElement(childElement);
            var nodeWithURL = childNode.All.Where(el => el.LocalName == "div" &&
                                el.HasAttribute("class") &&
                                el.GetAttribute("class").StartsWith("id") &&
                                el.Children.Any(chEl => chEl.LocalName == "a" &&
                                chEl.HasAttribute("href"))).FirstOrDefault();

            var strNode = nodeWithURL.InnerHtml;

            strNode = strNode.Split("f=\"")[1];
            strNode = strNode.Split("\" t")[0];


            return "https://www.ilcats.ru/" + strNode;
        }

        /// <summary>
        /// Возвращаем имя из родительского елемента
        /// </summary>
        public async Task<string> GetModelId(IElement childElement)
        {
            var childNode = await GetDocumentFromElement(childElement);
            var nodeWithName = childNode.All.Where(el => el.LocalName == "div" &&
                                el.HasAttribute("class") &&
                                el.GetAttribute("class").StartsWith("id") &&
                                el.Children.Any(chEl => chEl.LocalName == "a" &&
                                chEl.HasAttribute("href"))).FirstOrDefault();

            var result = nodeWithName.TextContent;

            return result;
        }

        /// <summary>
        /// Возвращаем имя из родительского елемента
        /// </summary>
        public async Task<string> GetModelName(IElement parentElement)
        {
            var parentNode = await GetDocumentFromElement(parentElement);
            var nodeWithName = parentNode.All.Where(el => el.LocalName == "div" &&
                                                   el.HasAttribute("class") &&
                                                   el.GetAttribute("class").StartsWith("name")).FirstOrDefault();
            var result = nodeWithName.InnerHtml;

            return result;
        }

        /// <summary>
        /// возвращает начальную и конечную дату выпуска определенного автомобиля 
        /// </summary>
        public void GetModelStartDateAndFinishDate(IElement childElement, out DateTime? startDate, out DateTime? endDate)
        {
            var childNode = GetDocumentFromElement(childElement).Result;
            var nodeWithData = childNode.All.Where(el => el.LocalName == "div" &&
                                el.HasAttribute("class") &&
                                el.GetAttribute("class").StartsWith("dateRange")).FirstOrDefault();

            var strData = nodeWithData.InnerHtml;
            strData = strData.Replace("&nbsp;", " ");
            var dateStrList = strData.Split(" - ");
            List<DateTime?> dateTimeList = new List<DateTime?>();
            foreach (string data in dateStrList)
            {
                if (data != "   ...   ")
                {
                    var date = Convert.ToDateTime(data.Replace(".", "/"));
                    dateTimeList.Add(date);
                }
                else
                    dateTimeList.Add(null);
            }

            var result = dateTimeList.ToArray();
            startDate = result[0];
            endDate = result[1];
        }

        /// <summary>
        /// Возвращает коды моделей из одного дочернего элемента модели машины.
        /// </summary>
        public async Task<List<string>> GetModelCodes(IElement childElement)
        {
            var childNode = await GetDocumentFromElement(childElement);
            var nodeWithCodes = childNode.All.Where(el => el.LocalName == "div" &&
                                el.HasAttribute("class") &&
                                el.GetAttribute("class").StartsWith("modelCode")).FirstOrDefault();

            var codesStr = nodeWithCodes.InnerHtml;
            var codesList = codesStr.Split(',').ToList();

            return codesList;
        }

        /// <summary>
        /// Метод создает список машин с первого слоя сайта
        /// </summary>
        public async Task<List<CarEntity>> GetCarEntities()
        {
            var strContent = GetPageStrContent(Config.HomePage);

            var document = await CreateDataDocument(strContent);

            var allParentDivs = GetParentCarDivElementsList(document);

            List<CarEntity> allCars = new List<CarEntity>();

            foreach (var pn in allParentDivs)
            {
                allCars.AddRange(await CreateCarEntitysFromSingleNode(pn));
            }

            return allCars;
        }

        #endregion FirstLvlMethods

        #region SecondLvlMethods

        public async Task GetCarComplictationsIntoAllCars()
        {
            var allCars = await GetCarEntities();
            foreach (CarEntity car in allCars)
            {

                var tempComple = GetAllComplictationsForOneCar(car);

            }
        }

        public async Task<CarEntity> GetAllComplictationsForOneCar (CarEntity car)
        {
            var strContent = GetPageStrContent(car.SecondLayerDataUrl);
            var document = await CreateDataDocument(strContent);
            var parentNode = GetParentNodeInSecondLayer(document);
            return null;
        }

        private async Task<IElement> GetParentNodeInSecondLayer(IDocument document)
        {
            var parenNodes = document.All.Where(el => el.LocalName == "tbody" &&
                                                el.Children.Any(chEl => chEl.LocalName == "div" &&
                                                chEl.HasAttribute("class") &&
                                                chEl.GetAttribute("class").StartsWith("modelCode")));

            return null;
        }



        #endregion SecondLvlMethods
    }
}
