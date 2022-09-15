using System;
using AngleSharp.Html.Parser;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using PageParser.Models;
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
        public List<CarEntity> CarsData;
        public List<string> HrefList;
        #endregion Properties

        public CustomParser()
        {
            Config = new Config();
            CarsData = new List<CarEntity>();
            HrefList = new List<string>();
        }

        #region Methods
        /// <summary>
        /// Получает cтроковый контент из html страницы используя её адрес.
        /// </summary>
        public string GetPageStrContent(string url)
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

        //public List<string> GetFirstLayerCarsData(IDocument document)
        //{
        //    List<CarModel> carModelList = new List<CarModel>();
        //    List<string> hyperlinkList = new List<string>();

        //    var elements = document.All.Where(m => m.LocalName == "div" &&
        //                             m.HasAttribute("class") &&
        //                             m.GetAttribute("class").StartsWith("id")).ToList();

        //    foreach (IElement el in elements)
        //    {
        //        var carModel = new CarModel();

        //        carModel.SecondLayerDataUrl = el.InnerHtml;

        //        hyperlinkList.Add(el.InnerHtml);

        //        carModel.Id = el.TextContent;
        //    }

        //    return hyperlinkList;
        //}

        /// <summary>
        /// Получает все родительские html элементы с домашней страницы, которые содержат данные и ссылку на варианты модели машины.
        /// </summary>
        public List<IElement> GetParentCarDivElementsList(IDocument document)
        {
            var elements = document.All.Where(el => el.LocalName == "div" &&
                                                el.HasAttribute("class") &&
                                                el.GetAttribute("class").StartsWith("List") &&
                                                el.Children.Any(chEl => chEl.LocalName == "div" &&
                                                chEl.HasAttribute("class") &&
                                                chEl.GetAttribute("class").StartsWith("Header")));
            return elements.ToList();
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
        public async Task<List<CarEntity>> CreateCarEntitysFromParentNode(IElement parentElement)
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

                carEntity = await GetModelDataToCarEntity(carEntity, el); // get model data



                carEntities.Add(carEntity);
            }
            return carEntities;
        }

        //public string GetSecondLayerDataUrl(IElement childElement)
        //{
        //    string temp = childElement.InnerHtml;
        //    int start = 0;
        //    int finish = 0;
        //    for (int i = 0; i < temp.Length; i++)
        //    {
        //        var start = "href=";
        //        for(int j = 0; j < temp.Length; j++)
        //        {
        //            if()
        //        }
        //    }

        //    return null;
        //}


        /// <summary>
        /// Возвращаем имя из родительского елемента
        /// </summary>
        public async Task<string> GetModelId(IElement childElement)
        {
            var childNode = await GetDocumentFromElement(childElement);
            var nodeWithName = childNode.All.Where(el => el.LocalName == "div" &&
                                el.HasAttribute("class") &&
                                el.GetAttribute("class").StartsWith("id")).FirstOrDefault();

            var result = nodeWithName.InnerHtml;

            return result;
        }

        /// <summary>
        /// Возвращаем имя из родительского елемента
        /// </summary>
        public async Task<string> GetModelName(IElement parentElement)
        {
            var childNode = await GetDocumentFromElement(parentElement);
            var nodeWithName = childNode.All.Where(el => el.LocalName == "div" &&
                                el.HasAttribute("class") &&
                                el.GetAttribute("class").StartsWith("name")).FirstOrDefault();

            var result = nodeWithName.InnerHtml;

            return result;
        }

        /// <summary>
        /// Возвращает обьект carEntity с отредатированной датой старта и финиша
        /// </summary>
        public async Task<CarEntity> GetModelDataToCarEntity(CarEntity car, IElement childElement)
        {
            var childNode = await GetDocumentFromElement(childElement);
            var nodeWithData = childNode.All.Where(el => el.LocalName == "div" &&
                                el.HasAttribute("class") &&
                                el.GetAttribute("class").StartsWith("dateRange")).FirstOrDefault();

            var strData = nodeWithData.InnerHtml;
            strData = strData.Replace(",", "/");
            var dataStrList = strData.Split("&nbsp;-&nbsp;");
            List<DateTime?> dataTimeList = new List<DateTime?>();
            foreach(string data in dataStrList)
            {
                if (data != "...")
                    dataTimeList.Add(Convert.ToDateTime(data));
                else
                    dataTimeList.Add(null);
            }

            car.StartDate = dataTimeList[0];
            car.FinishDate = dataTimeList[1];

            return car;
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
        /// Возвращает тип документ из типа елемент
        /// </summary>
        private async Task<IDocument> GetDocumentFromElement(IElement element)
        {
            var strContent = element.ToHtml();

            return await CreateDataDocument(strContent);
        }

        #endregion Methods
    }
}
