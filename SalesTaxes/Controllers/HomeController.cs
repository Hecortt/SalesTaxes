using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SalesTaxes.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SalesTaxes.Controllers
{
    #region Sessions

    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }

    #endregion

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        #region Items



        public IActionResult ListItems()
        {
            string path = @"File\product.json";
            try
            {
                using (StreamReader jsonStream = System.IO.File.OpenText(path))
                {
                    var json = jsonStream.ReadToEnd();
                    List<Item> litems = JsonConvert.DeserializeObject<List<Item>>(json);
                    List<Item> lfiltered = new List<Item>();

                    var itemsagrouped = litems.GroupBy(m => new { m.Imported, m.Name, m.Price, m.TaxGeneral, m.TaxImported }).
                                       Select(s => new
                                       {
                                           Name = s.Key.Name,
                                           Imported = s.Key.Imported,
                                           TaxGeneral = s.Key.TaxGeneral,
                                           TaxImported = s.Key.TaxImported,
                                           Price = s.Sum(s => s.Price),
                                           Qty = s.Sum(s => s.Qty)

                                       });

                    foreach (var entry in itemsagrouped)
                    {
                        Item item = new Item
                        {

                            Name = entry.Name,
                            Qty = entry.Qty,
                            Imported = entry.Imported,
                            TaxGeneral = entry.TaxGeneral,
                            TaxImported = entry.TaxImported,
                            Price = entry.Price
                        };
                        lfiltered.Add(item);
                    }

                    if (litems != null)
                        return View("Items/ListItems", lfiltered);
                }
            }
            catch (Exception ex) { }

            return View("Items/ListItems", null);
        }

        public IActionResult NewItem()
        {
            Item item = new Item();
            item.TypeInput = true;

            return View("Items/NewItem", item);
        }

        public IActionResult NewItemSingleLine()
        {
            Item item = new Item();
            item.TypeInput = false;

            return View("Items/NewItem", item);
        }

        public IActionResult ClearRecipe()
        {
            string path = @"File\product.json";
            System.IO.File.Delete(path);

            if (!System.IO.File.Exists(path))
                ViewData["Message"] = "Recipe Deleted";

            return View("Items/ListItems", null);
        }

        public IActionResult PrintRecipe()
        {

            string path = @"File\product.json";
            try
            {
                using (StreamReader jsonStream = System.IO.File.OpenText(path))
                {
                    var json = jsonStream.ReadToEnd();
                    List<Item> litems = JsonConvert.DeserializeObject<List<Item>>(json);
                    List<Item> lfiltered = new List<Item>();

                    var itemsagrouped = litems.GroupBy(m => new { m.Imported, m.Name, m.Price, m.TaxGeneral, m.TaxImported }).
                                       Select(s => new
                                       {
                                           Name = s.Key.Name,
                                           Imported = s.Key.Imported,
                                           TaxGeneral = s.Key.TaxGeneral,
                                           TaxImported = s.Key.TaxImported,
                                           Price = s.Sum(s => s.Price),
                                           Qty = s.Sum(s => s.Qty)

                                       });

                    foreach (var entry in itemsagrouped)
                    {
                        Item item = new Item
                        {

                            Name = entry.Name,
                            Qty = entry.Qty,
                            Imported = entry.Imported,
                            TaxGeneral = entry.TaxGeneral,
                            TaxImported = entry.TaxImported,
                            Price = entry.Price
                        };
                        lfiltered.Add(item);
                    }

                    if (litems != null)
                        return View("Recipes/ListRecipes", lfiltered);
                }
            }
            catch (Exception ex) { }

            return View("Items/ListItems", null);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _NewItemSingleLine(Item item)
        {
            if (item.CompletedName == null)
                item.CompletedName = item.Qty.ToString() + " " + (item.Imported ? "Imported " : "") + item.Name + " at " + item.Price.ToString();

            if (ValidateString(item.CompletedName))
            {
                return View("Items/Error");
            }
            else
            {
                string[] initialStructure = ValidateStructure(item.CompletedName);
                int countArray = initialStructure.Length;
                switch (countArray)
                {
                    case < 4:
                        return View("Items/Error");
                        break;
                    case 4:
                        item.Id = Guid.NewGuid();
                        item.Qty = int.Parse(initialStructure[0]);
                        item.Name = initialStructure[1];
                        item.TaxGeneral = ValidateExceptionsTax(item.Name) ? 0 : .10;
                        item.Price = double.Parse(initialStructure[countArray - 1]);
                        break;

                    case > 4:
                        ///  Would exist an imported item
                        ///  An import duty (import tax) applies to all imported items at a rate of 5% of the shelf price, with no exceptions.
                        if (ValidateImported(initialStructure))
                        {
                            item.Id = Guid.NewGuid();
                            item.Imported = true;
                            item.TaxImported = .05;
                            item.Qty = int.Parse(initialStructure[0]);

                            for (int i = 2; i < countArray - 2; i++)
                                item.Name += initialStructure[i] + " ";

                            item.TaxGeneral = ValidateExceptionsTax(item.Name) ? 0 : .10;
                            item.Price = double.Parse(initialStructure[countArray - 1]);

                        }
                        else
                        {
                            item.Id = Guid.NewGuid();
                            item.Imported = false;
                            item.Qty = int.Parse(initialStructure[0]);

                            for (int i = 1; i < countArray - 2; i++)
                                item.Name += initialStructure[i] + " ";

                            item.TaxGeneral = ValidateExceptionsTax(item.Name) ? 0 : .10;
                            item.Price = double.Parse(initialStructure[countArray - 1]);
                        }
                        break;
                }

                string path = @"File\product.json";
                List<Item> litems = new List<Item>();
                try
                {
                    using (StreamReader jsonStream = System.IO.File.OpenText(path))
                    {
                        var jsonexist = jsonStream.ReadToEnd();
                        litems = JsonConvert.DeserializeObject<List<Item>>(jsonexist);
                    }
                }
                catch (Exception ex) { }

                litems.Add(item);

                string json = JsonConvert.SerializeObject(litems);
                System.IO.File.WriteAllText(path, json);

                return RedirectToAction("ListItems", "Home");
                //return View("Items/ListItems", litems);
            }
        }

        #endregion

        #region Validations

        /// <summary>
        ///  An import duty (import tax) applies to all imported items at a rate of 5% of the shelf price, with no exceptions.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool ValidateImported(string[] str)
        {
            bool imported = str.Contains("Imported") ? true : false;
            return imported;
        }

        /// <summary>
        /// exception of books, food, and medical products
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool ValidateExceptionsTax(string str)
        {
            str = str.ToUpper();
            bool exception = str.Contains("CHOCOLATES") || str.Contains("CHOCOLATE") || str.Contains("PILLS") || str.Contains("BOOK") ? true : false;
            return exception;
        }


        /// <summary>
        /// return qty of elements
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string[] ValidateStructure(string str)
        {
            string[] stru = str.Split(' ');
            return stru;
        }

        public bool ValidateString(string str)
        {
            bool ex1 = false;
            bool ex2 = false;
            bool ex3 = false;
            string[] stru = str.Split(' ');
            int cstr = stru.Length;

            try
            {
                if (char.IsNumber(char.Parse(stru[0])))
                    ex1 = false;
                else
                    ex1 = true;
            }
            catch (Exception ex)
            {
                ex1 = true;
            }

            try
            {
                if (double.Parse(stru[cstr - 1].ToString()) > 0.1)
                    ex2 = false;
                else
                    ex2 = true;
            }
            catch (Exception ex)
            {
                ex2 = true;
            }

            try
            {
                if (stru[cstr - 2].ToString().ToUpper() == "AT")
                    ex3 = false;
                else
                    ex3 = true;
            }
            catch (Exception ex)
            {
                ex3 = true;
            }

            if (!ex1 && !ex2 && !ex3)
                return false;
            else return true;

        }

        #endregion



    }
}
