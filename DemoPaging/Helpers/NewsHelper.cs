using DemoPaging.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace DemoPaging.Helpers
{
    public class NewsHelper
    {
        PageInfo _pageInfo;
        string _url;

        public NewsHelper()
        {
            _url = "https://newsapi.org/v2/top-headlines?q=trump&apiKey=b09d05e603a9495d99d9eb1f8294f862";
            _pageInfo = new PageInfo();
            _pageInfo.PageSize = 10;
            _pageInfo.TotalItems = 0;
            GetDataFromNewsApi(GetUrl(1)); //read first page to get total items
        }

        /**
         * @brief Get last 'size' news
         */
        public List<News> GetAllNews(int size = 50)
        {
            var context = new NewsDBEntities();
            if (context.News.Count() == _pageInfo.TotalItems)
                return context.News.OrderBy(x => x.Page).Skip(Math.Max(0, context.News.Count() - size)).ToList();
            else
            {
                LoadNews();
                return GetAllNews(size);
            }
        }

        /**
         * @brief Get news for specific page
         */
        public List<News> GetNewsByPage(int page = 1)
        {
            var context = new NewsDBEntities();
            var newsOnPage = context.News.Where(x => x.Page == page).ToList();
            if (newsOnPage.Count() == 0)
            {
                newsOnPage = LoadNews(page);
            }
            return newsOnPage;
        }

        /**
        * @brief Get total number of news at source
        */
        public int LoadNewsFromExternalApi()
        {
            return _pageInfo.TotalItems;
        }

        /**
        * @brief Get news by index
        */
        public News GetNewsByIndex(int idx)
        {
            int page = (int)Math.Ceiling((decimal)idx / _pageInfo.PageSize) + 1;
            int index = idx % _pageInfo.PageSize;
            try
            {
                return GetNewsByPage(page).ElementAt(index);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return new News();
            }
        }

        /**
         * @brief Delete news by index
         */
        public HttpResponseMessage DeleteNewsByIndex(int idx)
        {
            News news = GetNewsByIndex(idx);
            if (news == null)
                throw new System.ArgumentOutOfRangeException();
            else
            {
                using (var context = new NewsDBEntities())
                {
                    context.News.Remove(news);
                }
                var status = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.NoContent
                };
                return status;
            }
        }


        /**
        * @brief Get news by page to view representation
        */
        public IndexViewModel GetNewsByPageView(int page = 1)
        {
            IndexViewModel ivm = new
                IndexViewModel
            { PageInfo = _pageInfo, News = GetNewsByPage(page) };
            return ivm;
        }

        /**
         * @brief Load data from external api
         */
        private List<News> GetDataFromNewsApi(string url)
        {
            string json = "";
            try
            {
                json = new WebClient().DownloadString(url);
            }
            catch (System.Net.WebException ex)
            {
                Debug.WriteLine("The NEWS API returned an error: " + ex.Message);
            }
            if (!string.IsNullOrWhiteSpace(json))
            {
                var newsApiResponse = JsonConvert.DeserializeObject<NewsApiResponse>(json);
                if (newsApiResponse != null && newsApiResponse.Status == "ok")
                {
                    _pageInfo.TotalItems = newsApiResponse.TotalResults > 100 ? 100 : newsApiResponse.TotalResults; //без подписки не больше 100 записей, хотя в результате может быть больше
                    return newsApiResponse.Articles;
                }
                else throw new JsonException("Something happend while deserialize: " + json);
            }
            else throw new WebException("No data loaded from source: " + url);
        }

        /**
         * @brief Load news and save in database
         * @details if page = 0 => load all news
         *          if page > 0 => load news for one page
         */
        private List<News> LoadNews(int page=0)
        {
            var context = new NewsDBEntities();
            var storedPages = context.News.Select(x => x.Page).Distinct().ToHashSet();
            
            var news = new List<News>();
            int start = page == 0 ? 1 : page;
            int end = page == 0 ? _pageInfo.TotalPages : page;
            for (var i = start; i <= end; ++i)
            {
                if (!storedPages.Contains(i))
                {
                    var newsBunch = GetDataFromNewsApi(GetUrl(i));
                    newsBunch.ForEach(x => x.Page = i);
                    news.AddRange(newsBunch);
                }
            }
            context.News.AddRange(news);
            context.SaveChanges();

            return news;
        }

        private string GetUrl(int page)
        {
            return _url + "&pageSize=" + _pageInfo.PageSize + "&page=" + page;
        }
    }
}