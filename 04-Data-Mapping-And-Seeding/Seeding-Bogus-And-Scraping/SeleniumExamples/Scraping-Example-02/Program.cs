using CsvHelper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;

class Program
{

    // define a custom type to store
    // the product objects to scrape
    public class Product
    {
        public string? Name { get; set; }
        public string? Price { get; set; }
    }
    static void Main(string[] args)
    {
        // to store the scraped data
        var products = new List<Product>();

        // instantiate a driver instance to control
        // Chrome in headless mode
        var chromeOptions = new ChromeOptions();
        //chromeOptions.AddArguments("--headless=new"); // comment out for testing
        var driver = new ChromeDriver(chromeOptions);

        // open the target page in Chrome
        driver.Navigate().GoToUrl("https://scrapingclub.com/exercise/list_infinite_scroll/");

        // deal with infinite scrolling
        // deal with infinite scrolling
        var jsScrollScript = @"
            const scrolls = 10
            let scrollCount = 0

            // scroll down and then wait for 0.5s
            const scrollInterval = setInterval(() => {
            window.scrollTo(0, document.body.scrollHeight)
            scrollCount++

              if (scrollCount === numScrolls) {
                  clearInterval(scrollInterval)
              }
            }, 500)
            ";

        driver.ExecuteScript(jsScrollScript);

        // wait 10 seconds for the DOM to be updated
        // by waiting for the 60-th product element to be on the page
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        //wait.Until(c => c.FindElement(By.CssSelector(".post:nth-child(20)")));
        wait.Until(c => c.FindElements(By.CssSelector(".post")).Count >= 50);


        // select the product HTML elements on the page
        // with a CSS Selector
        var productElements = driver.FindElements(By.CssSelector(".post"));

        // iterate over each product and extract data from them
        foreach (var productElement in productElements)
        {
            // select the name and price elements
            var nameElement = productElement.FindElement(By.CssSelector("h4"));
            var priceElement = productElement.FindElement(By.CssSelector("h5"));

            // extract their data
            var name = nameElement.Text;
            var price = priceElement.Text;

            // create a new product object and add it to the list
            var product = new Product { Name = name, Price = price };
            products.Add(product);
        }

        // create the CSV output file
        using (var writer = new StreamWriter("C:\\CALCULATION\\products.csv"))
        // instantiate the CSV writer
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            // populate the CSV file
            csv.WriteRecords(products);
        }

        // close the browser and release its resources
        driver.Quit();
        Console.ReadLine();
    }
}