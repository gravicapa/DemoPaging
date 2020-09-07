using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DemoPaging.Models
{
    internal class NewsApiResponse
    {
        public string Status { get; set; }
        public int? Code { get; set; }
        public string Message { get; set; }
        public List<News> Articles { get; set; }
        public int TotalResults { get; set; }
    }
}