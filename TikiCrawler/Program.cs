using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using SeleniumExtras.WaitHelpers;

namespace TikiCrawler
{
    class Product
    {
        public string Title { get; set; }
        public string Brand { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }
        public string DetailInformation { get; set; }
        public List<string> ImgUrl { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //Define total number of product needed to get
            int totalProductCount = 100;

            //Create an instance of Chrome driver
            IWebDriver browser = new ChromeDriver();

            //Navigate to website Tiki.vn > Laptop category
            browser.Navigate().GoToUrl("https://tiki.vn/laptop/c8095");
            //page index
            int currentPage = 1;

            

            //store product crawled
            var productsData = new List<Product>();

            while (productsData.Count<totalProductCount)
            {
                Console.WriteLine("Current page: " + currentPage);

                // Wait for the page to load
                //browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                //WebDriverWait wait = new WebDriverWait(browser, TimeSpan.FromSeconds(20));
                //wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("a.product-item")));
                System.Threading.Thread.Sleep(3000);

                //Select all product items by CSS Selector
                var products = browser.FindElements(By.CssSelector(".product-item"));
                Console.WriteLine("Number of products in page:" + products.Count);

                //if there's no product left
                if (products.Count == 0)
                {
                    break;
                }
                //test
                //list store all product links
                List<string> listProductLink = new List<string>();
                int i = 0;
                foreach (var product in products)
                {
                    //string outerHtml = product.GetAttribute("outerHTML");
                    //string productLink = Regex.Match(outerHtml, "href=\"(.*?)\"").Groups[1].Value;
                    //productLink = "https://" + productLink;
                    try
                    {
                        string productLink = product.GetAttribute("href");
                        listProductLink.Add(productLink);
                    }
                    catch
                    {
                        Console.WriteLine(product.GetAttribute("outerHTML"));
                        Console.WriteLine($"Product number {i+1} at page {currentPage} have href not found");
                    }
                    i++;
                }

                //Go to each product link
                //for (int i = 0; i < 5; i++)
                foreach (var productLink in listProductLink)
                {
                    if (productsData.Count >= totalProductCount)
                        break;
                    //var productLink = listProductLink[i];
                    Console.WriteLine("Going to: " + productLink.ToString());

                    //Go to product link
                    try
                    {
                        browser.Navigate().GoToUrl(productLink);
                    }
                    catch
                    {
                        try
                        {
                            browser.Navigate().GoToUrl("tiki.vn" + productLink);
                        }
                        catch
                        {
                            Console.WriteLine("Link error");
                            continue;
                        }

                    }

                    //Declare product information variables
                    string productTitle;
                    string productBrand;
                    List<string> productImgs = new List<string>();
                    string productDetails;
                    string productPrice;
                    string productDescription;

                    // Wait for the page to load
                    //browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                    System.Threading.Thread.Sleep(1000);

                    //Extract product information by CSS Selector
                    try
                    {

                        productTitle = browser.FindElement(By.CssSelector("h1.title")).Text;
                        Console.WriteLine("Product title: " + productTitle);
                    }
                    catch
                    {
                        Console.WriteLine("Title not found");
                        continue;
                    }

                    try
                    {
                        //Extract product brand by CSS Selector then remove redundant data by Regular Expression
                        //string productBrand = browser.FindElements(By.CssSelector(".brand-and-author"))[0].GetAttribute("outerHTML");
                        productBrand = browser.FindElement(By.XPath("//a[@data-view-id='pdp_details_view_brand']")).Text;
                        //productBrand = Regex.Match(productBrand, "brand\">(.*?)</a>").Groups[1].Value;
                        Console.WriteLine("Product brand: " + productBrand);
                    }
                    catch
                    {
                        Console.WriteLine("Brand not found");
                        continue;
                    }

                    //Extract product images
                    try
                    {
                        var groupImgs = browser.FindElements(By.CssSelector(".review-images img"));
                        foreach (var img in groupImgs)
                        {
                            string imgSrc = img.GetAttribute("src");
                            //Get bigger img from img cdn
                            imgSrc.Replace("100x100", "600x600");
                            productImgs.Add(imgSrc);
                            Console.WriteLine("Image source: " + imgSrc);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Image not found");
                        continue;
                    }



                    //Extract product price
                    try
                    {
                        var price = browser.FindElement(By.CssSelector(".product-price__current-price")).Text;
                        //string price = browser.FindElements(By.CssSelector(".product-price__current-price"));
                        price = Regex.Match(price, "^[\\d|\\.|\\,]+").Value;
                        productPrice = price.Replace(".", string.Empty);
                        Console.WriteLine("Product price: " + price);
                    }
                    catch
                    {
                        Console.WriteLine("Price not found");
                        continue;
                    }

                    //Extract product details
                    try
                    {
                        productDetails = browser.FindElement(By.CssSelector(".content.has-table table")).GetAttribute("outerHTML");

                    }
                    catch
                    {
                        Console.WriteLine("Details not found");
                        continue;
                    }

                    //Extract product description
                    try
                    {
                        productDescription = browser.FindElement(By.CssSelector(".ToggleContent__View-sc-1dbmfaw-0.wyACs")).GetAttribute("innerHTML");
                        //var productDescription = browser.FindElement(By.CssSelector(".content.has-table table"));
                        //string details = productDescription.GetAttribute("outerHTML");
                    }
                    catch
                    {
                        Console.WriteLine("Description not found");
                        continue;
                    }

                    //Create product object from product informations collected
                    var product = new Product { Title = productTitle, Brand = productBrand, ImgUrl = productImgs, Description = productDescription, DetailInformation = productDetails, Price = productPrice };
                    //Add to list
                    productsData.Add(product);
                    System.Threading.Thread.Sleep(3000);
                }
                
                
                //Navigate to next page
                browser.Navigate().GoToUrl("https://tiki.vn/laptop/c8095?page="+ (++currentPage));

            }
            //Config delimiter
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "@"
            };
            using (var writer = new StreamWriter("products.csv"))
            using (var csv = new CsvWriter(writer, config))
            {
                //Write data header
                csv.WriteField("Name");
                csv.WriteField("Categories");
                csv.WriteField("Regular Price");
                csv.WriteField("Images");
                csv.WriteField("Description");
                csv.WriteField("Short Description");
                csv.NextRecord();

                // Write the data rows
                foreach (var product in productsData)
                {
                    csv.WriteField(product.Title);
                    csv.WriteField(product.Brand);
                    csv.WriteField(product.Price);
                    csv.WriteField(string.Join(",", product.ImgUrl));
                    csv.WriteField(product.Description);
                    csv.WriteField(product.DetailInformation);
                    csv.NextRecord();
                }

            }
        }
    }
}