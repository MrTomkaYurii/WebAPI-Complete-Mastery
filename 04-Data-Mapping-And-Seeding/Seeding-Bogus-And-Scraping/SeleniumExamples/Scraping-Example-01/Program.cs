using CsvHelper;
using HtmlAgilityPack;
using System.Globalization;

internal class Program
{
    class Book
    {
        public string? Title { get; set; }
        public string? Price { get; set; }
    }

    private static void Main(string[] args)
    {
        List<string> bookLinks = GetBookLinks("https://books.toscrape.com/catalogue/category/books/mystery_3/index.html");
        Console.WriteLine("Found {0} links", bookLinks.Count);
        List<Book> books = GetBookDetails(bookLinks);
        exportToCSV(books);
    }

    static  void exportToCSV(List<Book> books)
    {
        using (StreamWriter writer = new StreamWriter("C:\\CALCULATION\\books.csv"))
        using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(books);
        }
    }
    static List<Book> GetBookDetails(List<string> urls)
    {
        List<Book> books = new List<Book>();
        foreach (var url in urls)
        {
            HtmlDocument document = GetDocument(url);
            string titlexpath = "//h1";
            string pricexpath = "//div[contains(@class,\"product_main\")]/p[@class=\"price_color\"]";
            Book book = new Book();
            book.Title = document.DocumentNode.SelectSingleNode(titlexpath).InnerText;
            book.Price = document.DocumentNode.SelectSingleNode(pricexpath).InnerText;
            books.Add(book);
        }
        return books;
    }

    static HtmlDocument GetDocument(string url)
    {
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        return doc;
    }

    static List<string> GetBookLinks (string url)
    {
        List<string> booksLinks = new List<string>();
        HtmlDocument doc = GetDocument(url);
        HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//h3/a");
        Uri baseUri = new Uri(url);
        foreach (HtmlNode link in linkNodes)
        {
            string href = link.Attributes["href"].Value;
            booksLinks.Add(new Uri(baseUri, href).AbsoluteUri);
        }
        return booksLinks;
    }
}