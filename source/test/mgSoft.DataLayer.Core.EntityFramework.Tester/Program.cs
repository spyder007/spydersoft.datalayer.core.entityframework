using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mgSoft.DataLayer.Core.EntityFramework.Tester
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder();
            options.UseSqlServer("Server=envy-mg\\SQL2014;Database=OneDev;Trusted_Connection=True;MultipleActiveResultSets=true");
            var dbProvider = new DataContextProvider(new DataContext(options.Options));

            var dataStore = new DataStore(dbProvider);

            TestGetItems(dataStore);

            TestSaveItems(dataStore);

            TestDeleteItems(dataStore);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        private static void TestGetItems(DataStore dataStore)
        {
            TestAndLogAction("GetItem with Primary Key", () => {
                var error = string.Empty;
                var bev = dataStore.GetItem<Beverage>(1);

                if (bev == null)
                {
                    error = "Error: Unable to locate Beverage with ID = 1";
                }

                return error;
            });


            TestAndLogAction("GetItems", () =>
            {
                var error = string.Empty;
                var bevs = dataStore.GetItems<Beverage>();

                if (bevs == null || !bevs.Any())
                {
                    error = "Error: No Beverage records found";
                }

                return error;
            });

            TestAndLogAction("GetItems With Predicate", () =>
            {
                var error = string.Empty;
                var bevs = dataStore.GetItems<Beverage>(myBev => myBev.BeverageKey == "GUINNESS");

                if (bevs == null || !bevs.Any())
                {
                    error = "Error: No Beverage record found for GUINNESS";
                }

                return error;
            });

            TestAndLogAction("GetItems With Predicate and Skip/Take", () =>
            {
                var error = string.Empty;
                var bevs = dataStore.GetItems<Beverage>(myBev => myBev.BeverageKey == "GUINNESS", 0, 10);

                if (bevs == null || !bevs.Any())
                {
                    error = "Error: No Beverage record found for GUINNESS";
                }

                return error;
            });


        }

        private static void TestSaveItems(DataStore dataStore)
        {
            TestAndLogAction("Save New Item", () => {
                var error = string.Empty;
                var bevSave = new Beverage()
                {
                    BeverageKey = "STRONGBOW-HONEY",
                    Description = "Strongbow Hard Cider - Honey",
                    ExternalInfo = null,
                    Name = "Strongbow Honey"
                };

                bevSave = dataStore.SaveItem(bevSave);

                if (bevSave.Id == 0)
                {
                    error = "Error: Id not populated for new item";
                }

                return error;
            });


            TestAndLogAction("Update Existing Item", () =>
            {
                var error = string.Empty;

                Beverage existingBeverage = dataStore.GetItems<Beverage>(myBev => myBev.BeverageKey == "STRONGBOW-HONEY").FirstOrDefault();
                if (existingBeverage == null)
                {
                    return "Error: Unable to load existing record for test";
                }

                existingBeverage.Description += "-Test";
                existingBeverage = dataStore.SaveItem(existingBeverage);

                Beverage reloadBeverage = dataStore.GetItems<Beverage>(myBev => myBev.BeverageKey == "STRONGBOW-HONEY").FirstOrDefault();
                if (reloadBeverage == null)
                {
                    return "Error: Unable to reload existing record for test";
                }

                if (reloadBeverage.Description != existingBeverage.Description)
                {
                    return "Error: Changes did not save correctly";
                }

                return error;
            });

        }

        private static void TestDeleteItems(DataStore dataStore)
        {
            TestAndLogAction("Delete Item", () => {
                var error = string.Empty;
                Beverage existingBeverage = dataStore.GetItems<Beverage>(myBev => myBev.BeverageKey == "STRONGBOW-HONEY").FirstOrDefault();
                if (existingBeverage == null)
                {
                    return "Error: Unable to load existing record for test";
                }

                dataStore.DeleteItem(existingBeverage);

                var reloadBeverage = dataStore.GetItems<Beverage>(myBev => myBev.BeverageKey == "STRONGBOW-HONEY");
                if (reloadBeverage != null && reloadBeverage.Any())
                {
                    return "Error: Record not deleted";
                }

                return error;
            });
        }

        private static void TestAndLogAction(string testName, Func<string> functionToTest)
        {
            Console.Write($"Running {testName}...");

            var error = string.Empty;
            try
            {
                error = functionToTest.Invoke();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            Console.WriteLine((string.IsNullOrEmpty(error) ? "Success" : "Failure"));
            Console.WriteLine(error);
        }
    }
}
