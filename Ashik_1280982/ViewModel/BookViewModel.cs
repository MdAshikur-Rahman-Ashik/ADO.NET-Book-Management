using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ashik_1280982.ViewModel
{
    public class BookViewModel
    {
        public int BookId { get; set; }
        public string BookCode { get; set; }
        public string BookName { get; set; }
        public DateTime PublishDate { get; set; }
        public string BookStatus { get; set; }
        public bool IsMultiLanguage { get; set; }
        public int TotalCategoryWisePrice { get; set; }
        public string CategoryTitle { get; set; }
        public string ImagePath { get; set; }
    }
}
