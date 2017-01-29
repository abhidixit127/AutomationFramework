using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized; // Add reference System.Configuration
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Timers;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Interactions.Internal;
using OpenQA.Selenium.Interactions;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using Word = Microsoft.Office.Interop.Word;
using System.Runtime;
using System.Diagnostics;
using Novacode;
using System.Windows;
using System.Web;


/// <summary>
///  This program is for running the scripts maintained in TestCaseList excel file.
/// </summary>



namespace AutomationFramework
{
    public class NativeException : Exception   // To generate exceptions.
    {
        public NativeException()
        {

        }
    }
    internal class  MyReflectionClass { }
    class Program 
    {
        public static bool finalResult;
        private static Excel.Workbook MyBook = null;
        private static Excel.Application MyApp = null;
        private static Excel.Worksheet MySheet = null;
        public static Microsoft.Office.Interop.Word.Application winword = new Microsoft.Office.Interop.Word.Application();
        public static  Microsoft.Office.Interop.Word.Document document = winword.Documents.Add();
        public static string foldername;
        public static int SSCount;
        public static IWebDriver driver;
        //IWebDriver driver = new FirefoxDriver();
        public static String Url;

        public object Request { get; private set; }
        public Uri urlReferrer { get;}

        static void Main(string[] args)
       {
            int usedColumn, usedRows;
             String Id, Name, UserId, Passwd,MethodName;

            MyApp = new Excel.Application();
            
            MyBook = MyApp.Workbooks.Open(ConfigurationManager.AppSettings.Get("path"));             // Path where excel file is stored.
            MySheet = MyBook.Worksheets["TestCaseList"];                        // Sheet Name is Test Case List .

            usedColumn = MySheet.UsedRange.Columns.Count;
            usedRows = MySheet.UsedRange.Rows.Count;
            //

            Scripts  S = new Scripts();
            for (int i= 2; i <= usedRows; ++i)
            {
                object m = ((Microsoft.Office.Interop.Excel.Range)MySheet.Cells.Cells[i, 7]).Text;
                Char Execute = Convert.ToChar(m);

                if (Execute == 'Y')
                {
                    m = ((Microsoft.Office.Interop.Excel.Range)MySheet.Cells.Cells[i, 1]).Text;   // To pick Test Case id from the excel
                    Id = Convert.ToString(m);

                    m = ((Microsoft.Office.Interop.Excel.Range)MySheet.Cells.Cells[i, 2]).Text;  // To pick Test Case Name from the excel.
                    Name = Convert.ToString(m);
                    

                    m = ((Microsoft.Office.Interop.Excel.Range)MySheet.Cells.Cells[i, 4]).Text;  // To pick url from the excel
                    Url = Convert.ToString(m);

                    m = ((Microsoft.Office.Interop.Excel.Range)MySheet.Cells.Cells[i, 5]).Text;  // To pick the user id from the excel.
                    UserId = Convert.ToString(m);

                    m = ((Microsoft.Office.Interop.Excel.Range)MySheet.Cells.Cells[i, 6]).Text;   // To pick the password from the excel
                    Passwd = Convert.ToString(m);



                    S.Initialize(Id, Name);             //Call the initialize method for creating the folder.
                    S.LaunchApp(Url, UserId, Passwd);   // Call the method LaunchApp for launching the browser.
                    
                    m = ((Microsoft.Office.Interop.Excel.Range)MySheet.Cells.Cells[i, 8]).Text;   // To convert the value of method name provided in excel to a method name.
                    MethodName = Convert.ToString(m);

                    

                    Type mTypeObj = S.GetType();
                    MethodInfo MyMethodInfo = mTypeObj.GetMethod(MethodName);
                     
                    object[] mParam = new object[] { Url};
                    MyMethodInfo.Invoke(S, mParam);
                    

                    S.CloseApp();           // Close the browser.
                    
                }

                else
                    continue;
             }


            MyBook.Close(0);
            MyApp.Quit();


        }

      
        public static void TakeScreenshot()   // This method is for storing the screenshots in the TestSS folder.
        {
            string SSpath = foldername + "\\" + Convert.ToString(SSCount) + ".Jpeg";
            Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
            ss.SaveAsFile(SSpath, System.Drawing.Imaging.ImageFormat.Jpeg);
            SSCount++;
            winword.ActiveDocument.Characters.Last.Select();  // Line 1
            winword.Selection.Collapse();
            document.Application.Selection.InlineShapes.AddPicture(SSpath);
            System.IO.File.Delete(SSpath);
            document.Save();
            
        }


        public static void Sleep(int time)      // This method is for waiting the browser for the amount of seconds passed through parameter.
        {
            System.Threading.Thread.Sleep(time);
        }



        
        public static void PageVerify_Console(IWebElement element, string Display_Name, string Search)
        {
            string ele_text = element.Text.ToString().ToUpper();
            if (ele_text.Contains(Search.ToUpper()))
                Console.WriteLine(" Successfully Verified {0}", Display_Name);
            else
                Console.WriteLine(" Failed to Verify {0}", Display_Name);
        }

        public static void wait1(double time)           // This method is for waiting the browser for the amount of seconds passed through parameter.
        {
           // driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(50));
            DateTime now = DateTime.Now;
            string CurrentTime = Convert.ToString(now);
            DateTime NewT = now.AddSeconds(time);
            string NewTime = Convert.ToString(NewT);
            while (CurrentTime != NewTime)
            {
                CurrentTime = Convert.ToString(DateTime.Now);
            }
        }


        public void MethodReporting(String status, String reporting)
        {

            winword.ActiveDocument.Characters.Last.Select();  // Line 1
            winword.Selection.Collapse();

            if (status == "Pass")
                winword.Selection.Font.Color = Word.WdColor.wdColorSeaGreen;
            else
                winword.Selection.Font.Color = Word.WdColor.wdColorRed;

            winword.Selection.TypeText(reporting);

            document.Save();
            TakeScreenshot();
            document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);


        }

        public void Reporting(String status, String reporting)
        {

            winword.ActiveDocument.Characters.Last.Select();  // Line 1
            winword.Selection.Collapse();

            if (status == "Pass")
                winword.Selection.Font.Color = Word.WdColor.wdColorSeaGreen;
            else
                winword.Selection.Font.Color = Word.WdColor.wdColorDarkRed;


            winword.Selection.TypeText(reporting);

            document.Save();
            TakeScreenshot();

            document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);


        }



        /// <summary>
        /// Method Name : ElementClick
        /// For performing click operation on any element 
        /// </summary>
        /// <param name="LocatorMethod"></param> Denotes the locator method used for identifying the element
        /// <param name="Locator"></param>Denotes the string value for identifying the element
        /// <param name="ElementName"></param>denotes the reporting parameter.
        public void ElementClick(String LocatorMethod, String Locator, String ElementName)
        {
            String status, reporting;
            String text = "NULL";


            String method = Convert.ToString(LocatorMethod).ToUpper();
            try
            {
                switch (method)
                {
                    case "XPATH":
                        text = driver.FindElement(By.XPath(Locator)).Text;
                        driver.FindElement(By.XPath(Locator)).Click();
                        break;
                    case "Id":
                        text = driver.FindElement(By.Id(Locator)).Text;
                        driver.FindElement(By.Id(Locator)).Click();
                        break;
                    case "TAGNAME":
                        text = driver.FindElement(By.TagName(Locator)).Text;
                        driver.FindElement(By.TagName(Locator)).Click();
                        break;
                    case "LINKTEXT":
                        text = driver.FindElement(By.LinkText(Locator)).Text;
                        driver.FindElement(By.LinkText(Locator)).Click();
                        break;
                    case "CSS":
                        text = driver.FindElement(By.CssSelector(Locator)).Text;
                        driver.FindElement(By.CssSelector(Locator)).Click();
                        break;
                    case "CLASSNAME":
                        text = driver.FindElement(By.ClassName(Locator)).Text;
                        driver.FindElement(By.ClassName(Locator)).Click();
                        break;
                    case "NAME":
                        text = driver.FindElement(By.Name(Locator)).Text;
                        driver.FindElement(By.Name(Locator)).Click();
                        break;

                }
                status = "Pass";
                if (text == "" || text == "NULL")
                    reporting = "Successfully clicked on Element " + ElementName;
                else
                    reporting = "Successfully clicked on Element " + text;

            }
            catch
            {
                status = "Fail";
                if (text == "" || text == "NULL")
                    reporting = " Failed to click on Element " + ElementName;
                else
                    reporting = " Failed to click on Element " + text;


            }

            Reporting(status, reporting);

        }

        /// <summary>
        ///
        /// 
        /// </summary>
        /// <param name="Type"></param> denotes the locator method
        /// <param name="identifier"></param> identifier denotes the locator
        /// <param name="Value"></param>  Value denotes the value which needed to be set on a textbox
        /// <param name="ElementName"></param>ElementName denotes the reporting parameter.
        public void ElementTextSet(String LocatorMethod, String Locator, String Value, String ElementName)
        {
            String status, reporting;
            String method = Convert.ToString(LocatorMethod).ToUpper();
            try
            {
                switch (method)
                {
                    case "XPATH":
                        driver.FindElement(By.XPath(Locator)).SendKeys(Value);
                        break;
                    case "ID":
                        driver.FindElement(By.Id(Locator)).SendKeys(Value);
                        break;
                    case "TAGNAME":
                        driver.FindElement(By.TagName(Locator)).SendKeys(Value);
                        break;
                    case "LINKTEXT":
                        driver.FindElement(By.LinkText(Locator)).SendKeys(Value);
                        break;
                    case "CSS":
                        driver.FindElement(By.CssSelector(Locator)).SendKeys(Value);
                        break;
                    case "CLASSNAME":
                        driver.FindElement(By.ClassName(Locator)).SendKeys(Value);
                        break;
                    case "NAME":
                        driver.FindElement(By.Name(Locator)).SendKeys(Value);
                        break;

                }
                status = "Pass";
                reporting = "Successfully set " + Value + " in " + ElementName;
            }
            catch
            {
                status = "Fail";
                reporting = "Failed to  set " + Value + " in " + ElementName;
            }

            Reporting(status, reporting);

        }


        public void WaitUntil(String LocatorMethod, String Locator)
        {
            String status, reporting;

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));


            LocatorMethod = Convert.ToString(LocatorMethod).ToUpper();
            try
            {
                switch (LocatorMethod)
                {
                    case "XPATH":
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(Locator)));
                        break;
                    case "ID":
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(Locator)));
                        break;
                    case "TAGNAME":
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.TagName(Locator)));
                        break;
                    case "LINKTEXT":
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.LinkText(Locator)));
                        break;
                    case "CSS":
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(Locator)));
                        break;
                    case "CLASSNAME":
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName(Locator)));
                        break;
                    case "NAME":
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.Name(Locator)));
                        break;

                }
                status = "Pass";
                reporting = "Successfully Navigated to Page:" + driver.Title;

            }
            catch
            {
                status = "Fail";
                reporting = "Failed to Navigate to Page:" + driver.Title;
            }

            Reporting(status, reporting);

        }




    }


    class Scripts : Program
    {
        public object WdUnits { get; private set; }

        /// <summary>
        /// Inititialize method will create the folder in TestSS  with the TestCaseId and TestCaseNAme value provided in TestCaseLIst Excel file.Two parameters are holding the concerned values.
        /// Folder created in TestSS will append date and time in the created folder.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Name"></param>
        public void Initialize(String Id, String Name)
        {
            SSCount = 1;
            DateTime now = DateTime.Now;
            string time = Convert.ToString(now);
            time = time.Replace(':', '_');
            time = time.Replace('/', '_');
            if (time.Contains("AM"))
                time = time.Replace(" AM", "_");
            else
                time = time.Replace(" PM", "_");
            foldername = "D:\\TestingSuite\\Test_SS\\" + Id+"-" +Name + "-"+ time;
            foldername = foldername.Substring(0, foldername.Length - 1);
            System.IO.Directory.CreateDirectory(foldername);
            //String filename = foldername + "\\Screens";
            winword.Visible = false;
            document.SaveAs(@foldername + "\\ResultLog");
            
        }
        /// <summary>
        /// This method  will launch the browser with the url provided and will perofrm the login process.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="UserName"></param>
        /// <param name="Passwd"></param>
        public void LaunchApp(String url, String UserName, String Passwd)
        {
          // driver = new ChromeDriver();
           // driver = new FirefoxDriver();

            
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(url);
            
            
            driver.FindElement(By.Id("Email")).SendKeys(UserName);
            driver.FindElement(By.Id("Password")).SendKeys(Passwd);
            Reporting("Pass", "Welcome to Eshars Login Page");
            driver.FindElement(By.XPath("//input[@class='btn btn-success pull-right']")).Click();
            wait1(10);
        }
        /// <summary>
        /// This method will log out the active session and will close the browser. Will close the process in the task manager as well.
        
        /// </summary>
        public void CloseApp()
        {
            int c;
            driver.FindElement(By.XPath("//a[@class='dropdown-toggle']")).Click();
            wait1(3);
            Reporting("Pass", "Successfully clicked on LogoutDrop Down");
            driver.FindElement(By.XPath("//ul[@class='dropdown-menu']/li[4]/a")).Click();
            Reporting("Pass", "Successfully clicked on Logout");
            wait1(3);
            TakeScreenshot();
            driver.Close();
            driver.Quit();
            document.Close();
            document = null;
            winword.Quit();
            winword = null;
            

            foreach (var process in Process.GetProcessesByName("chromedriver"))
            {
                process.Kill();
            }
            foreach (var process in Process.GetProcessesByName("vshost32.exe"))
            {
                process.Kill();
            }

            wait1(5);
            if (finalResult == true)
            {
                string foldername2 = foldername + "_PASS";
                foldername2 = foldername2.Substring(0, foldername2.Length);
                try
                { 
                System.IO.Directory.Move(foldername, foldername2);
                }

                catch
                {
                    c = 1;
                }
            }
            
            else
            {
                string foldername2 = foldername + "_FAIL";
                foldername2 = foldername2.Substring(0, foldername2.Length);
                try
                {
                    System.IO.Directory.Move(foldername, foldername2);
                }
                catch
                {
                    c = 0;
                }
            }




        }

        public void ActivityTracking(String urls)  // For generating Activity Report
        {
            try {
                IJavaScriptExecutor js =  (IJavaScriptExecutor)driver;
                String title, ExecuteScript;
                if (Url.Contains("dev"))
            {
                driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                wait1(3);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                wait1(7);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

            }
            



            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
            IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
            element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));


            wait1(5);
            TakeScreenshot();
            driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();   // Click on Report Dashboard

            wait1(15);
            IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));
            /*           
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            string title = (string)js.ExecuteScript("$(\"#reportId\").data(\"kendoDropDownList\").select(2);");
            */
            TakeScreenshot();
            driver.FindElement(By.XPath("//*[@class='k-input']")).Click(); // Click on Report dropdown
            wait1(5);
            driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
            IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
            wait1(2);
            Actions actions = new Actions(driver);
            actions.MoveToElement(elements).Perform();

            // actions.Click();
            actions.SendKeys(Keys.ArrowDown);
            actions.Build().Perform();

            actions.SendKeys(Keys.Enter);
            actions.Build().Perform();
            wait1(10);
            TakeScreenshot();
            driver.FindElement(By.XPath("//tr[1]/td[2]/input[@class='chkBoxDistrict']")).Click(); // Click on District Checkbox
            wait1(5);
            driver.FindElement(By.XPath("//*//tr[3]/td[1]/input[@class='chkBoxCampus']")).Click();  // Click on Campus Checkbox
                wait1(5);
            driver.FindElement(By.XPath("//*//tr[1]/td[1]/input[@class='chkBoxClinician']")).Click();  // Click on Clinician Checkbox
                wait1(5);
            driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxService']")).Click();   // Click on Service Checkbox
                wait1(5);
            TakeScreenshot();

            driver.FindElement(By.XPath("//*[@id=\"runReport\"]")).Click();   // Click on Run Report button
                wait1(1);
                TakeScreenshot();
                wait1(15);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@id='newMessageBtn']")));    
            wait1(15);
            TakeScreenshot();
                driver.FindElement(By.XPath("//button[@id='newMessageBtn']")).Click();   // Click on Email Button
                wait1(1);
                TakeScreenshot();
                driver.FindElement(By.XPath("//button[@class='close']/span")).Click();
                wait1(1);
                TakeScreenshot();
               // ExecuteScript = "$(\"#ReportViewer_ctl05_ctl04_ctl00_ButtonImg\").click();";
                ExecuteScript = "$(\"[alt='Export drop down menu']\").click();";
                title = ( string)js.ExecuteScript(ExecuteScript);
                 wait1(3);
            
            ExecuteScript = "$(\"[title = 'Word']\").trigger(\"click\");";
            title = ( string)js.ExecuteScript(ExecuteScript);
            wait1(5);
               

            finalResult = true;
            }

            catch
            {
                finalResult = false;

            }
            
        }


        public void StudentMissingData(String urls)
        {
            try { 

            if (Url.Contains("dev"))
            {
                driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                wait1(3);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                wait1(7);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

            }
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
            IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
            element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
            wait1(5);
            TakeScreenshot();
            driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();
            wait1(15);
            IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));
            TakeScreenshot();
            driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
            wait1(5);
            driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
            IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
            wait1(2);
            Actions actions = new Actions(driver);
            actions.MoveToElement(elements).Perform();
            bool flag = false;
            int counter = 1;
            String title, ExecuteScript;
            while (flag !=true)
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                foreach(IWebElement link in links)
                if (link.Text == "Student Missing Data")
                    {
                        elements = driver.FindElement(By.XPath("//*[contains(text(),\"Student Missing Data\") and @class='k-input']"));
                        wait1(5);
                        actions.MoveToElement(elements).Perform();
                        actions.SendKeys(Keys.ArrowDown);
                        actions.Build().Perform();
                        wait1(3);
                        actions.SendKeys(Keys.ArrowUp);
                        actions.Build().Perform();

                        actions.SendKeys(Keys.Enter);
                        actions.Build().Perform();
                        flag = true;

                    }
                else
                    { 
                       counter += 1;
                       ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";
                       title = (string)js.ExecuteScript(ExecuteScript);
   
                    }
            }
            wait1(10);
            TakeScreenshot();
            title = (string)js.ExecuteScript("$(\"#parameters_DistrictId_\").data(\"kendoDropDownList\").select(1);");
            wait1(2);
            title = (string)js.ExecuteScript("$(\"#parameters_CampusId_\").data(\"kendoDropDownList\").select(1);");
            wait1(2);
            title = (string)js.ExecuteScript("$(\"#parameters_ServiceId_\").data(\"kendoDropDownList\").select(1);");
            wait1(1);
            driver.FindElement(By.XPath("//div[@class='form-group col-md-3'][4]/ul/label[@class='k-radio-label'][3]")).Click();
            wait1(1);
            driver.FindElement(By.XPath("//div[@class='form-group col-md-3'][5]/ul/label[@class='k-radio-label'][3]")).Click();
            wait1(1);
            driver.FindElement(By.XPath("//div[@class='form-group col-md-3'][6]/input")).SendKeys("November");
            wait1(2);
            elements = driver.FindElement(By.XPath("//div[@class='form-group col-md-3'][6]/input"));
            wait1(5);
            actions.MoveToElement(elements).Perform();
            actions.SendKeys(Keys.Tab);
            actions.Build().Perform();
            driver.FindElement(By.XPath("//div[@class='form-group col-md-3'][6]/input")).SendKeys("2016");
            wait1(2);
            title = (string)js.ExecuteScript("$(\"#parameters_SchoolYearId_\").data(\"kendoDropDownList\").select(1);");
            wait1(1);
            driver.FindElement(By.XPath("//input[@id='parameters_StartDate_']")).SendKeys("11/1/2016");
            driver.FindElement(By.XPath("//input[@id='parameters_EndDate_']")).SendKeys("11/20/2016");
            wait1(5);
            TakeScreenshot();
            driver.FindElement(By.XPath("//button[@id='runReport']")).Click();
            wait1(12);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@id='newMessageBtn']")));
            wait1(7);
            TakeScreenshot();
            driver.FindElement(By.XPath("//button[@id='newMessageBtn']")).Click();
            wait1(1);
            TakeScreenshot();
            driver.FindElement(By.XPath("//button[@class='close']/span")).Click();
            wait1(1);
            TakeScreenshot();
             ExecuteScript = "$(\"[alt='Export drop down menu']\").click();";
            title = (string)js.ExecuteScript(ExecuteScript);
            wait1(3);
            TakeScreenshot();
            ExecuteScript = "$(\"[title = 'Word']\").trigger(\"click\");";
            title = (string)js.ExecuteScript(ExecuteScript);
            wait1(5);
            TakeScreenshot();
            if (driver.PageSource.Contains("Students Missing Data"))
                finalResult = true;
            }

            catch
            {
                finalResult = false;

            }
        } //Function ends
        

        public void ActivityTrackingValidation(String urls)
        {
            try { 

            String district, campus, clinician, service;
            if (Url.Contains("dev"))
            {
                driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                wait1(3);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                wait1(7);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

            }




            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
            IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
            element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));


            wait1(5);
            TakeScreenshot();
            driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

            wait1(15);
            IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));
            /*           
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            string title = (string)js.ExecuteScript("$(\"#reportId\").data(\"kendoDropDownList\").select(2);");
            */
            TakeScreenshot();
            driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
            wait1(5);
            driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
            IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
            wait1(2);
            Actions actions = new Actions(driver);
            actions.MoveToElement(elements).Perform();

            // actions.Click();
            actions.SendKeys(Keys.ArrowDown);
            actions.Build().Perform();

            actions.SendKeys(Keys.Enter);
            actions.Build().Perform();
            wait1(10);
            TakeScreenshot();

            driver.FindElement(By.XPath("//tr[3]/td[2]/input[@class='chkBoxDistrict']")).Click();
            wait1(5);
            element = driver.FindElement(By.XPath("//div[@id='district-grid']/div[@class='k-grid-content']/table/tbody/tr[3]/td[3]"));
            district = element.Text;

            driver.FindElement(By.XPath("//*//tr[3]/td[1]/input[@class='chkBoxCampus']")).Click();
            wait1(5);
            element = driver.FindElement(By.XPath("//div[@id='campusgrid']/div[@class='k-grid-content']/table/tbody/tr[3]/td[3]"));
            campus = element.Text;
            driver.FindElement(By.XPath("//*//tr[1]/td[1]/input[@class='chkBoxClinician']")).Click();
            wait1(5);
            element = driver.FindElement(By.XPath("//div[@id='cliniciangrid']/div[@class='k-grid-content']/table/tbody/tr[1]/td[2]"));
            clinician = element.Text;
            driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxService']")).Click();
            wait1(5);
            element = driver.FindElement(By.XPath("//div[@id='servicegrid']/div[@class='k-grid-content']/table/tbody/tr[1]/td[2]"));
            service = element.Text;
            TakeScreenshot();

            driver.FindElement(By.XPath("//*[@id=\"runReport\"]")).Click();
            wait1(12);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@id='newMessageBtn']")));

            wait1(15);

            TakeScreenshot();

            element= driver.FindElement(By.XPath("//*[@id='ReportViewer_fixedTable']//tr[3]/td[2]/div[1]"));


            string district_temp = element.Text;
            element = driver.FindElement(By.XPath("//*[@id='ReportViewer_fixedTable']//tr[4]/td[2]/div[1]"));
            string campus_temp = element.Text;
            element = driver.FindElement(By.XPath("//*[@id='ReportViewer_fixedTable']//tr[5]/td[2]/div[1]"));
            string clinician_temp = element.Text;
            element = driver.FindElement(By.XPath("//*[@id='ReportViewer_fixedTable']//tr[6]/td[2]/div[1]"));
            string service_temp = element.Text;

                if ((district == district_temp) && (campus == campus_temp) && (clinician == clinician_temp) && (service == service_temp))
                    finalResult = true;

                else
                    throw new NativeException();
               
            }

            catch
            {
                finalResult = false;

            }

            

        }
        public void DistrictAppealsReport(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
            {
                driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                wait1(3);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                wait1(7);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

            }
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
            IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
            element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
            wait1(5);
            TakeScreenshot();
            driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();
            wait1(15);
            IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            TakeScreenshot();
            driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
            wait1(5);
            driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
            IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
            wait1(2);
            Actions actions = new Actions(driver);
            actions.MoveToElement(elements).Perform();
            bool flag = false;
            int counter = 1;
            String title, ExecuteScript;
            while (flag != true)
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                foreach (IWebElement link in links)
                   if (link.Text == "District Appeals")
                    {
                        elements = driver.FindElement(By.XPath("//*[contains(text(),\"District Appeals\") and @class='k-input']"));
                        wait1(5);
                        actions.MoveToElement(elements).Perform();
                        actions.SendKeys(Keys.ArrowDown);
                        actions.Build().Perform();
                        wait1(3);
                        actions.SendKeys(Keys.ArrowUp);
                        actions.Build().Perform();
                        actions.SendKeys(Keys.Enter);
                        actions.Build().Perform();
                        flag = true;

                    }
                 else
                    {
                        counter += 1;
                        ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";
                        title = (string)js.ExecuteScript(ExecuteScript);
                    }
            }

            wait1(10);
            TakeScreenshot();
            driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]")).Click();
            wait1(2);
            driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]")).Click();
            elements = driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]"));
            actions.MoveToElement(elements).Perform();
            actions.SendKeys(Keys.ArrowDown);
            actions.Build().Perform();
            actions.SendKeys(Keys.Enter);
            actions.Build().Perform();
            wait1(3);
            driver.FindElement(By.XPath("//span[@class='k-input' and contains(text(),\"-\")]")).Click();
            wait1(3);
            driver.FindElement(By.XPath("//span[@class='k-input' and contains(text(),\"-\")]")).Click();
            elements = driver.FindElement(By.XPath("//span[@class='k-input' and contains(text(),\"-\")]"));
            actions.MoveToElement(elements).Perform();
            actions.SendKeys(Keys.ArrowDown);
            actions.Build().Perform();
            actions.SendKeys(Keys.ArrowDown);
            actions.Build().Perform();
            actions.SendKeys(Keys.Enter);
            actions.Build().Perform();
            // title = (string)js.ExecuteScript("$(\"#listSchoolYear\").data(\"kendoDropDownList\").select(1);");
            wait1(3);
            driver.FindElement(By.XPath("//*[@id='contactPerson']")).SendKeys("Evans");
            TakeScreenshot();
            wait1(10);
            driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
            wait1(5);
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(text(),\"Student Last Name\")]")));
            TakeScreenshot();
            wait1(12);
            if (driver.PageSource.Contains("District Appeals Report"))
                finalResult = true;
            }

         catch
            {
                finalResult = false;
            }
        } // Function ends
        


        public void DistrictAppealsReportValidation(String urls)
        {
            if (Url.Contains("dev"))
            {
                driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                wait1(3);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                wait1(7);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

            }


            try
            {

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links)

                        if (link.Text == "District Appeals")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"District Appeals\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }
                
                wait1(10);
                TakeScreenshot();
                //title = (string)js.ExecuteScript("$(\"#dropDownDistrict\").data(\"kendoDropDownList\").select(1);");
                //wait1(3);                
                //driver.FindElement(By.XPath("//*[@id='contactPerson']")).SendKeys("Eshars");
                wait1(10);
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='okBtn']")).Click();

                wait1(5);
                TakeScreenshot();
                finalResult = true;
            }

            catch
            {
                finalResult = false;

            }
        }


        public void UserVisitTracking(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
            {
                driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                wait1(3);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                wait1(7);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

            }


            

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links)

                        if (link.Text == "User Visit Tracking")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"User Visit Tracking\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]")).Click();
                elements = driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]"));
                actions.MoveToElement(elements).Perform();
                actions.SendKeys(Keys.ArrowDown);
                actions.Build().Perform();
                actions.SendKeys(Keys.Enter);
                actions.Build().Perform();
               
                
                title = (string)js.ExecuteScript("$(\"#dropDownDistrict\").data(\"kendoDropDownList\").select(1);");
                wait1(10);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxCampus']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//*[@id='firstName']")).SendKeys("ABC");
                wait1(3);
                driver.FindElement(By.XPath("//*[@id='lastName']")).SendKeys("XYZ");
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                wait1(5);
                TakeScreenshot();
                if (driver.PageSource.Contains("User Visit Tracking"))
                    finalResult = true;
            }

            catch
            {
                finalResult = false;
           }
        } // Function End


        public void UserVisitTrackingValidation(String urls)
        {
            if (Url.Contains("dev"))
            {
                driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                wait1(3);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                wait1(7);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

            }


            try
            {

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links)

                        if (link.Text == "User Visit Tracking")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"User Visit Tracking\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]")).Click();
                elements = driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]"));
                actions.MoveToElement(elements).Perform();
                actions.SendKeys(Keys.ArrowDown);
                actions.Build().Perform();
                actions.SendKeys(Keys.Enter);
                actions.Build().Perform();
               
                wait1(3);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxCampus']")).Click();
                wait1(2);
               
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                wait1(5);
                TakeScreenshot();
                finalResult = true;
            }

            catch
            {
                finalResult = false;
            }

        }

        public void ProcessTabs(String urls)
        {
            if (Url.Contains("dev"))
            {
                driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                wait1(3);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                wait1(7);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

            }


            try
            {

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[2]/a")).Click();
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h3[@class='panel-title']")));
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[3]/a")).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='ssn-search']")));
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[4]/a")).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='btn btn-sm btn-danger']")));
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[5]/a")).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@id='cancelBtn']")));
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(),\"Run Report\")]")));
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[7]/a")).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@id='deleteAppealsButton']")));
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[8]/a")).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@id='newMessageBtn']")));
                wait1(3);
                TakeScreenshot();
               
                finalResult = true;
            }

            catch
            {
                finalResult = false;

             }

        }  //  Script Ends


        public void ClinicianUsers(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }




                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links)

                        if (link.Text == "Clinician Users")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"Clinician Users\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]")).Click();
                elements = driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a district\")]"));
                actions.MoveToElement(elements).Perform();
                actions.SendKeys(Keys.ArrowDown);
                actions.Build().Perform();
                actions.SendKeys(Keys.Enter);
                actions.Build().Perform();

                title = (string)js.ExecuteScript("$(\"#listSchoolYear\").data(\"kendoDropDownList\").select(2);");
                wait1(10);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxCampus']")).Click();
                wait1(2);
                title = (string)js.ExecuteScript("$(\"#listSchoolYear\").data(\"kendoDropDownList\").select(2);");
                
                wait1(3);
                driver.FindElement(By.XPath("//label/input[@id='checkAllStatuses']")).Click();

                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                wait1(5);
                TakeScreenshot();
                if (driver.PageSource.Contains("Clinician Users"))
                    finalResult = true;
            }

            catch
            {
                finalResult = false;
            }
        } // Function End


        public void DistrictARD(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }




                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links1 = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links1)

                        if (link.Text == "District ARD")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"District ARD\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
               
                wait1(10);
                driver.FindElement(By.XPath("//tr[1]/td[2]/input[@class='chkBoxDistrict']")).Click();
                wait1(2);     
                driver.FindElement(By.XPath("//tr[1]/td[2]/input[@class='chkBoxDistrict']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//tr[1]/td[2]/input[@class='chkBoxDistrict']")).Click();
                TakeScreenshot();
                wait1(4);
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//tr[1]/td[1]/input[@class='chkBoxCampus']")));
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxCampus']")).Click();
                TakeScreenshot();
                wait1(4);
                driver.FindElement(By.XPath("//tr/td[1]/input[@class='chkBoxClinician']")).Click();
                TakeScreenshot();
                wait1(4);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxService']")).Click();
                TakeScreenshot();
                wait1(4);
                driver.FindElement(By.XPath("//input[@id='radioAllStudents']")).Click();
                TakeScreenshot();
                wait1(4);
                title = (string)js.ExecuteScript("$(\"#listSchoolYear\").data(\"kendoDropDownList\").select(2);");
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                foreach (IWebElement link in links)
                    if (link.Text.Contains("20") | link.Text.Contains("19"))
                    {
                        string abc = "//span[@class='k-input' and contains (text(),\"20\")]";
                        driver.FindElement(By.XPath(abc)).Click();
                        wait1(2);
                        elements = driver.FindElement(By.XPath(abc));
                        wait1(5);
                        actions.MoveToElement(elements).Perform();
                        actions.SendKeys(Keys.ArrowDown);
                        actions.Build().Perform();
                        wait1(3);
                        actions.SendKeys(Keys.Escape);
                        actions.Build().Perform();


                        actions.SendKeys(Keys.Enter);
                        actions.Build().Perform();
                    }

                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(10);
                TakeScreenshot();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='btnBack']")));
                wait1(10);
                if (driver.PageSource.Contains("ARD"))
                finalResult = true;
            }

            catch
            {
                finalResult = false;
            }
        } // Function End



        public void DistrictRevenue(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }




                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links)

                        if (link.Text == "District Revenue")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"District Revenue\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//tr[2]/td[2]/input[@class='chkBoxDistrict']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a District\")]")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a District\")]")).Click();
                elements = driver.FindElement(By.XPath("//span[@class='k-input'  and contains(text(),\"Select a District\")]"));
                actions.MoveToElement(elements).Perform();
                actions.SendKeys(Keys.ArrowDown);
                actions.Build().Perform();
                actions.SendKeys(Keys.ArrowDown);
                actions.Build().Perform();
                actions.SendKeys(Keys.Enter);
                actions.Build().Perform();

               // title = (string)js.ExecuteScript("$(\"#listSchoolYear\").data(\"kendoDropDownList\").select(2);");
                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='btnBack']")));
                wait1(5);
                if (driver.PageSource.Contains("Revenue"))
                    finalResult = true;
            }

            catch
            {
                finalResult = false;
            }
        } // Function End

        public void ProgressNotes(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }




                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links1 = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links1)

                        if (link.Text == "Progress Notes")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"Progress Notes\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//tr[1]/td[2]/input[@class='chkBoxDistrict']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxCampus']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxService']")).Click();
                wait1(2);
                TakeScreenshot();
                title = (string)js.ExecuteScript("$(\"#listSchoolYear\").data(\"kendoDropDownList\").select(2);");
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                foreach (IWebElement link in links)
                    if (link.Text.Contains("20")| link.Text.Contains("19"))
                    { string abc= "//span[@class='k-input' and contains (text(),\"20\")]";
                        driver.FindElement(By.XPath(abc)).Click();
                        wait1(2);
                        elements = driver.FindElement(By.XPath(abc));
                        wait1(5);
                        actions.MoveToElement(elements).Perform();
                        actions.SendKeys(Keys.ArrowDown);
                        actions.Build().Perform();
                        wait1(3);
                        actions.SendKeys(Keys.Escape);
                        actions.Build().Perform();
                        

                        actions.SendKeys(Keys.Enter);
                        actions.Build().Perform();
                    }
            

                driver.FindElement(By.XPath("//input[@id='StudentFirstName']")).SendKeys("Myeshars");
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='StudentLastName']")).SendKeys("eshars");

                
                
                wait1(2);
                
                
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='btnBack']")));
                wait1(5);
                if (driver.PageSource.Contains("Progress"))
                    finalResult = true;
            }

            catch
            {
                finalResult = false;
            }
        } // Function End


        public void StudentsData(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }




                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links1 = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links1)

                        if (link.Text == "Student Data")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"Student Data\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//tr[1]/td[2]/input[@class='chkBoxDistrict']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxCampus']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxService']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='chkParentalConsentYes']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='radioAllStudents']")).Click();
                wait1(2);
                TakeScreenshot();
                title = (string)js.ExecuteScript("$(\"#listSchoolYear\").data(\"kendoDropDownList\").select(2);");
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                foreach (IWebElement link in links)
                    if (link.Text.Contains("20") | link.Text.Contains("19"))
                    {
                        string abc = "//span[@class='k-input' and contains (text(),\"20\")]";
                        driver.FindElement(By.XPath(abc)).Click();
                        wait1(2);
                        elements = driver.FindElement(By.XPath(abc));
                        wait1(5);
                        actions.MoveToElement(elements).Perform();
                        actions.SendKeys(Keys.ArrowDown);
                        actions.Build().Perform();
                        wait1(3);
                        actions.SendKeys(Keys.Escape);
                        actions.Build().Perform();
                        actions.SendKeys(Keys.Enter);
                        actions.Build().Perform();
                    }
                driver.FindElement(By.XPath("//input[@id='parentalConsentStartDate']")).SendKeys("01/01/2016");
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='parentalConsentEndDate']")).SendKeys("12/31/2016");
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='btnBack']")));
                wait1(5);
                if (driver.PageSource.Contains("Student"))
                    finalResult = true;
            }

            catch
            {
                finalResult = false;
            }
        } // Function End

        public void TMHPComparison(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }




                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links1 = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links1)

                        if (link.Text == "TMHP Comparison")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"TMHP Comparison\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//tr[2]/td[2]/input[@class='chkBoxDistrict']")).Click();
                wait1(2);
                
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='startDate']")).Clear();
                wait1(2);
                TakeScreenshot();
                
                driver.FindElement(By.XPath("//input[@id='startDate']")).SendKeys("01/01/2016");
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='endDate']")).Clear();
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='endDate']")).SendKeys("12/31/2016");
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='btnBack']")));
                wait1(10);
                if (driver.PageSource.Contains("TMHP"))
                    finalResult = true;
            }

            catch
            {
                finalResult = false;
            }
        } // Function End

        public void TRMonthlyReport(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }




                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links1 = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links1)

                        if (link.Text == "TR Monthly Report")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"TR Monthly Report\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//tr[1]/td[2]/input[@class='chkBoxDistrict']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='monthData']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='monthData']")).SendKeys("October");
                elements = driver.FindElement(By.XPath("//input[@id='monthData']"));
                wait1(3);
                actions.MoveToElement(elements).Perform();
                actions.SendKeys(Keys.Tab);
                actions.Build().Perform();
                driver.FindElement(By.XPath("//input[@id='monthData']")).SendKeys("2016");
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//button[@id='addMonth']")).Click();
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//select[@id='lstBoxSelectedMonths']/option[1]")).Click();
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//button[@id='removeMonth']")).Click();
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='monthData']")).SendKeys("October");
                elements = driver.FindElement(By.XPath("//input[@id='monthData']"));
                wait1(5);
                actions.MoveToElement(elements).Perform();
                actions.SendKeys(Keys.Tab);
                actions.Build().Perform();
                driver.FindElement(By.XPath("//input[@id='monthData']")).SendKeys("2016");
                TakeScreenshot();
                driver.FindElement(By.XPath("//button[@id='addMonth']")).Click();
                wait1(2);
                TakeScreenshot();


                driver.FindElement(By.XPath("//input[@id='routeData']")).SendKeys("123");
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='btnBack']")));
                wait1(5);
                if (driver.PageSource.Contains("Monthly"))
                    finalResult = true;
            }

            catch
            {
                finalResult = false;
            }
        } // Function End


        public void UserCaseload(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }




                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links1 = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links1)

                        if (link.Text == "User Caseload")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"User Caseload\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                            actions.SendKeys(Keys.ArrowUp);
                            actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//tr[1]/td[2]/input[@class='chkBoxDistrict']")).Click();
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxCampus']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxService']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='radioMedicaid']")).Click();
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='radioAllStudents']")).Click();
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='radioCaseloadAssignedStudents']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='radioCaseloadAllStudents']")).Click();
                TakeScreenshot();
                //driver.FindElement(By.XPath("//span[@class='k-input' and contains(text(),\"Select a District\")]")).Click();
                wait1(1);
                title = (string)js.ExecuteScript("$(\"#listSchoolYear\").data(\"kendoDropDownList\").select(2);");
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                foreach (IWebElement link in links)
                    if (link.Text.Contains("20") | link.Text.Contains("19"))
                    {
                        string abc = "//span[@class='k-input' and contains (text(),\"20\")]";
                        driver.FindElement(By.XPath(abc)).Click();
                        wait1(2);
                        elements = driver.FindElement(By.XPath(abc));
                        wait1(5);
                        actions.MoveToElement(elements).Perform();
                        actions.SendKeys(Keys.ArrowDown);
                        actions.Build().Perform();
                        wait1(3);
                        actions.SendKeys(Keys.Escape);
                        actions.Build().Perform();


                        actions.SendKeys(Keys.Enter);
                        actions.Build().Perform();
                    }
                
                wait1(5);
                        
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='btnBack']")));
                wait1(5);
                if (driver.PageSource.Contains("Caseload"))
                    finalResult = true;
            }

            catch
            {
                finalResult = false;
            }
        } // Function End

        public void VisitDetails(String urls)
        {
            try
            {
                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }




                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a")).Click();

                wait1(15);
                IWebElement element1 = driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[6]/a"));

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                wait1(5);
                driver.FindElement(By.XPath("//*[@class='k-input']")).Click();
                IWebElement elements = driver.FindElement(By.XPath("//*[@class='k-input']"));
                wait1(2);
                Actions actions = new Actions(driver);
                actions.MoveToElement(elements).Perform();
                bool flag = false;
                int counter = 1;
                String title, ExecuteScript;
                while (flag != true)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links1 = driver.FindElements(By.XPath("//*[@class='k-input']"));
                    foreach (IWebElement link in links1)

                        if (link.Text == "User Visit Tracking")
                        {
                            elements = driver.FindElement(By.XPath("//*[contains(text(),\"User Visit Tracking\") and @class='k-input']"));
                            wait1(5);
                            actions.MoveToElement(elements).Perform();
                            actions.SendKeys(Keys.ArrowDown);
                            actions.Build().Perform();
                            wait1(3);
                           // actions.SendKeys(Keys.ArrowUp);
                           // actions.Build().Perform();

                            actions.SendKeys(Keys.Enter);
                            actions.Build().Perform();
                            flag = true;

                        }
                        else
                        {
                            counter += 1;
                            ExecuteScript = "$(\"#reportId\").data(\"kendoDropDownList\").select(" + Convert.ToString(counter) + ");";

                            title = (string)js.ExecuteScript(ExecuteScript);
                        }
                }

                wait1(10);
                TakeScreenshot();
                driver.FindElement(By.XPath("//tr[1]/td[2]/input[@class='chkBoxDistrict']")).Click();
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxCampus']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxClinician']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//tr[1]/td[1]/input[@class='chkBoxService']")).Click();
                wait1(2);
                driver.FindElement(By.XPath("//input[@id='radioAllStudents']")).Click();
                
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='radioMedicaid']")).Click();
                wait1(2);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='radioByCreatedDate']")).Click();
                wait1(2);
                wait1(5);
               TakeScreenshot();
                
                wait1(1);
                title = (string)js.ExecuteScript("$(\"#listSchoolYear\").data(\"kendoDropDownList\").select(2);");
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//*[@class='k-input']"));
                foreach (IWebElement link in links)
                    if (link.Text.Contains("20") | link.Text.Contains("19"))
                    {
                        string abc = "//span[@class='k-input' and contains (text(),\"20\")]";
                        driver.FindElement(By.XPath(abc)).Click();
                        wait1(2);
                        elements = driver.FindElement(By.XPath(abc));
                        wait1(5);
                        actions.MoveToElement(elements).Perform();
                        actions.SendKeys(Keys.ArrowDown);
                        actions.Build().Perform();
                        wait1(3);
                        actions.SendKeys(Keys.Escape);
                        actions.Build().Perform();


                        actions.SendKeys(Keys.Enter);
                        actions.Build().Perform();
                    }

                wait1(5);
              
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[@id='runReport']")).Click();
                wait1(5);
                TakeScreenshot();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='btnBack']")));
                wait1(5);
                if (driver.PageSource.Contains("Caseload"))
                    finalResult = true;
            }

            catch
            {
                
                finalResult = false;
            }
        } // Function End


        public void HomeDashboardValidation(String urls)
        {
            try
            {
                String LastName, FirstName, DOB, StudentId, MedicaidId, CampusName, SSNId;

                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }



                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();

                LastName = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[1]")).Text;
                FirstName = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[2]")).Text;
                DOB = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[3]")).Text;
                StudentId = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[4]")).Text;
                MedicaidId = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[5]")).Text;
                CampusName = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[6]")).Text;


                driver.FindElement(By.XPath("//tr[@class='k-alt'][1]/td[3]/button[@class='btn btn-default']")).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(" //button[@id='deleteVisitBtn']")));
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.Id("cancelVisitBtn")).Click();
                wait1(3);
                TakeScreenshot();

                if (driver.FindElement(By.XPath("//div[@class='toast-title']")).Text == "WARNING MESSAGE")
                    driver.FindElement(By.Id("yesBtn")).Click();

                wait1(5);
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                TakeScreenshot();
                driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[7]/span/a[@class='btn btn-default'] ")).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[contains(text(),\"General\")]")));
                wait1(3);
                TakeScreenshot();

                if ((driver.FindElement(By.Id("LastName")).Text == LastName) && (driver.FindElement(By.Id("FirstName")).Text == FirstName) && (driver.FindElement(By.Id("DateOfBirth")).Text == DOB)
                        && (driver.FindElement(By.Id("StudentNumber")).Text == StudentId) && (driver.FindElement(By.Id("MedicaidId")).Text == MedicaidId) && (driver.FindElement(By.Id("CampusName")).Text == CampusName))
                {
                    SSNId = driver.FindElement(By.Id("SSN")).Text;
                    String[] SSN = Regex.Split(SSNId, "-");
                    for(int i=0; i<=2;++i)
                    {
                        switch(i)
                        {
                            case 0:
                                if (SSN[i] == "xxx")
                                    break;
                                else
                                    throw new NativeException();


                            case 1:
                                if (SSN[i] == "xx")
                                    break;
                                else
                                    throw new NativeException();
                            case 2:
                                int ssn = Convert.ToInt32(SSN[i]);
                                bool numeric = int.TryParse("123",out ssn);
                                if (numeric == true)
                                    break;
                                else
                                    throw new NativeException();
                        }
                    }
                }
                    driver.FindElement(By.XPath("//*[contains(text(),\"Visit History\")] ")).Click();
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='btn btn-sm btn-danger']")));
                    wait1(3);
                    TakeScreenshot();

                    driver.FindElement(By.XPath("//*[contains(text(),\"ARD\")] ")).Click();
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(),\"Add new record\")]")));
                    wait1(3);
                    TakeScreenshot();

                    driver.FindElement(By.XPath("//*[contains(text(),\"Campus History\")]  ")).Click();
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[contains(text(),\"Add New Campus\")]")));
                    wait1(3);
                    TakeScreenshot();

                    driver.FindElement(By.XPath("//*[contains(text(),\"District History\")]  ")).Click();
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[@id='district-grid']/table/tbody/tr/td[1]")));
                    wait1(3);
                    TakeScreenshot();

                    driver.FindElement(By.XPath("//*[contains(text(),\"Medicaid History\")]  ")).Click();
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[@id='medicaid-grid']/table/tbody/tr/td[1]")));
                    wait1(3);
                    TakeScreenshot();

                    driver.FindElement(By.XPath("//*[contains(text(),\"TR Route\")]  ")).Click();
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(),\"AM/Pickup Route\")][1]")));
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//*[contains(text(),\"Parental Consent\")]  ")).Click();
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[contains(text(),\"Parental Consent Flag\")]")));
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//*[contains(text(),\"Attachments\")]  ")).Click();
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='attachmentUpload']")));
                    wait1(3);
                    TakeScreenshot();


                

                if (driver.PageSource.Contains("Upload Attachment"))
                    finalResult = true;
            }

            catch
            {
                
                finalResult = false;
            }
        } // Function End

        public void CloseVisit(String urls)
        {
            try
            {
                String LastName, FirstName, DOB, StudentId, MedicaidId, CampusName, SSNId;

                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }



                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();

                LastName = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[1]")).Text;
                FirstName = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[2]")).Text;
                DOB = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[3]")).Text;
                StudentId = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[4]")).Text;
                MedicaidId = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[5]")).Text;
                CampusName = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[1]/td[6]")).Text;


                driver.FindElement(By.XPath("//tr[1]/td[7]//button[@class='btn btn-default visitInit']")).Click();
                wait1(1);
                TakeScreenshot();
                string service = driver.FindElement(By.XPath("//tr[1]/td[7]//ul[@class='dropdown-menu']/li[2]/a")).Text;

                driver.FindElement(By.XPath("//tr[1]/td[7]//ul[@class='dropdown-menu']/li[2]/a")).Click();
                wait1(3);

                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h3[@class='panel-title main-title']")));

               
                TakeScreenshot();
                if (driver.FindElement(By.XPath("//h3[@class='panel-title main-title']")).Text ==   service.ToUpper())
                {
                    driver.FindElement(By.XPath("//input[@id='VisitDate']")).SendKeys("12/07/2016");
                    wait1(3);
                    TakeScreenshot();
                    element = driver.FindElement(By.XPath("//input[@id='VisitDate']"));
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(element).Perform();

                    // actions.Click();
                    actions.SendKeys(Keys.Tab);
                    actions.Build().Perform();

                }
                else
                     throw new NativeException();

               // driver.FindElement(By.XPath("//input[@id='VisitDate']")).SendKeys("12/07/2016");
                wait1(3);
                driver.FindElement(By.XPath("//input[@id='VisitStartTime']")).SendKeys("7:00 AM");
                wait1(3);
                driver.FindElement(By.XPath("//input[@id='VisitEndTime']")).SendKeys("7:30 AM");
                wait1(3);
                driver.FindElement(By.XPath("//textarea[@id='Notes']")).SendKeys("Testing");
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.Id("cancelVisitBtn")).Click();
                wait1(3);
                TakeScreenshot();

                if (driver.FindElement(By.XPath("//div[@class='toast-title']")).Text == "WARNING MESSAGE")
                    driver.FindElement(By.Id("yesBtn")).Click();

                wait1(5);
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                TakeScreenshot();
                if (driver.PageSource.Contains("Students"))
                    finalResult = true;
            }

            catch
            {

                finalResult = false;
            }
        } // Function End
        /// <summary>
        /// AddDeleteVisit Method is for adding  a visit to a particular tudent and then navigate to Visit History Tab to search it, followed by deleting it.
        /// </summary>
        /// <param name="urls"></param>
        public void AddDeleteVisit(String urls)  
        {
            try
            {
                String LastName, FirstName, DOB, StudentId, MedicaidId, CampusName, SSNId;

                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }



                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);
                TakeScreenshot();

                LastName = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[3]/td[1]")).Text;
                FirstName = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[3]/td[2]")).Text;
                DOB = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[3]/td[3]")).Text;
                StudentId = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[3]/td[4]")).Text;
                MedicaidId = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[3]/td[5]")).Text;
                CampusName = driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[3]/td[6]")).Text;
                string FinalName = LastName + "," + FirstName;


                driver.FindElement(By.XPath("//tr[3]/td[7]//button[@class='btn btn-default visitInit']")).Click();
                wait1(1);
                TakeScreenshot();
                string service = driver.FindElement(By.XPath("//tr[3]/td[7]//ul[@class='dropdown-menu']/li[2]/a")).Text; 

                driver.FindElement(By.XPath("//tr[3]/td[7]//ul[@class='dropdown-menu']/li[2]/a")).Click();// Clicked on service Audiology Evaluation
                wait1(3);

                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h3[@class='panel-title main-title']")));  // Waiting till same service page appears


                TakeScreenshot();
                if (driver.FindElement(By.XPath("//h3[@class='panel-title main-title']")).Text == service.ToUpper())
                {
                    driver.FindElement(By.XPath("//input[@id='VisitDate']")).SendKeys("12/07/2016");
                    wait1(3);
                    TakeScreenshot();
                    element = driver.FindElement(By.XPath("//input[@id='VisitDate']"));
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(element).Perform();

                    // actions.Click();
                    actions.SendKeys(Keys.Tab);
                    actions.Build().Perform();

                }
                else
                    throw new NativeException();

                // driver.FindElement(By.XPath("//input[@id='VisitDate']")).SendKeys("12/07/2016");
                wait1(3);
                driver.FindElement(By.XPath("//input[@id='VisitStartTime']")).SendKeys("7:00 AM");
                wait1(3);
                driver.FindElement(By.XPath("//input[@id='VisitEndTime']")).SendKeys("7:30 AM");
                wait1(3);
                driver.FindElement(By.XPath("//textarea[@id='Notes']")).SendKeys("Testing");
                wait1(3);
                driver.FindElement(By.Id("DocumentationDoNotBill")).Click();
                wait1(1);
                TakeScreenshot();
                driver.FindElement(By.Id("saveVisitBtn")).Click();
                wait1(15);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[1]/a")).Click();
                
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                TakeScreenshot();
                wait1(10);
                driver.FindElement(By.XPath("//div[@id='caseLoadGrid']//tbody/tr[3]/td[7]/span/a[@class='btn btn-default']")).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[contains(text(),\"General\")]")));
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//*[contains(text(),\"Visit History\")] ")).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='btn btn-sm btn-danger']")));
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.XPath("//input[@id='visitSearchStartDate']")).SendKeys("12072016");
                wait1(4);
               // driver.FindElement(By.XPath("//input[@id='visitSearchEndDate']")).SendKeys("12182016");
                
                wait1(3);
                TakeScreenshot();
                driver.FindElement(By.Id("listServiceTypes")).SendKeys("Audiology Evaluation");
                wait1(3);
                driver.FindElement(By.XPath("//select[@id='listVisitStatuses']")).SendKeys("Not Billable");
                TakeScreenshot();
                driver.FindElement(By.Id("search-button")).Click();
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//tr[1]/td[12]/button[@class='btn btn-default'][3]")).Click();
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.Id("yesBtn")).Click();
                wait1(5);
                TakeScreenshot();
                driver.FindElement(By.XPath("//ul[@class='nav navbar-right top-nav']/li[1]/a")).Click();

                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                TakeScreenshot();
                wait1(5);
                
                if (driver.PageSource.Contains("Students"))
                    finalResult = true;
            }

            catch
            {
                
                finalResult = false;
            }
        } // Function End


        public void AddToCaseload(String urls)
        {
            try
            {


               
                if (Url.Contains("dev"))
                {
                    driver.FindElement(By.XPath("//*//input[@id='txtLocation']")).Click();
                    wait1(3);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-orgTree'][3]")).Click();
                    wait1(7);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//input[@id='txtProvider']")).Click();
                    wait1(5);
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][3]")).Click();
                    wait1(3);
                    TakeScreenshot();
                    driver.FindElement(By.XPath("//li[@class='list-group-item node-impersonatorTree'][7]")).Click();

                }
                int i;
                bool flag;

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]")));
                IWebElement element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                element = driver.FindElement(By.XPath("//a[@class='k-link' and contains(text(),\"Students\")]"));
                wait1(5);

                ElementClick("Xpath", "//ul[@class='nav navbar-right top-nav']/li[2]/a", "CaseLoad Tab");
                WaitUntil("id", "search-button");
                wait1(5);
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath("//div[@class='k-loading-image'][1]")));


                string FirstName = driver.FindElement(By.XPath("//div[@id='studentListGrid']/table/tbody/tr[1]/td[3]")).Text;
                string LastName = driver.FindElement(By.XPath("//div[@id='studentListGrid']/table/tbody/tr[1]/td[2]")).Text;
                string FirstNameLocator = "NULL", LastNameLocator = "NULL";
                ElementClick("Xpath", "//tr[1]/td[8]/button[@class='btn btn-default addToCaseLoad']", "Add To CaseLoad");
                wait1(5);
                do
                    wait1(2);

                while (FirstName != driver.FindElement(By.XPath("//div[@id='studentListGrid']/table/tbody/tr[1]/td[3]")).Text);

                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath("//div[@class='k-loading-image'][1]")));
            Start:
                try
                {

                    for (i = 1; i <= 20; ++i)
                    {

                        FirstNameLocator = "//div[@id='Caseload-grid']/table/tbody/tr[" + i + "]/td[1]";
                        LastNameLocator = "//div[@id='Caseload-grid']/table/tbody/tr[" + i + "]/td[2]";
                        if ((FirstName == driver.FindElement(By.XPath(FirstNameLocator)).Text) && (LastName == driver.FindElement(By.XPath(LastNameLocator)).Text))
                        {
                            flag = true;
                            Reporting("Pass", "Sucessfully Added to CaseLoad");
                            break;

                        }

                        else
                            flag = false;

                    }
                }
                catch
                {
                    if (driver.FindElement(By.XPath("//a[@class='k-link k-pager-nav k-state-disabled'][2]")).Enabled)
                    {
                        ElementClick("xpath", "//a[@class='k-link k-pager-nav k-state-disabled'][2]", "Next CaseLoad Arrow");
                        goto Start;
                    }

                    else
                        throw new  NativeException();


                }


                wait1(10);
                ElementClick("xpath", "//tr[" + i + "]/td[3]/a[@class='k-button k-button-icontext k-grid-Edit']/button[@class='btn btn-default']", "RemoveStudent From Caseload");
                wait1(5);
                ElementClick("xpath", "//button[@id='yesBtn']", "Confirmation Box");
                wait1(5);
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath("//div[@class='k-loading-image'][1]")));
                do
                    wait1(2);

                while (FirstName != driver.FindElement(By.XPath("//div[@id='studentListGrid']/table/tbody/tr[1]/td[3]")).Text);

                if ((driver.FindElement(By.XPath("//div[@id='studentListGrid']/table/tbody/tr[1]/td[3]")).Text == FirstName) && (LastName == driver.FindElement(By.XPath("//div[@id='studentListGrid']/table/tbody/tr[1]/td[2]")).Text))
                    Reporting("Pass", "Student Removed From Caseload");
                else
                    Reporting("Fail ", "Failed to remove Student From CaseLoad");

                if (driver.PageSource.Contains("Create My Caseload"))
                    finalResult = true;
                


            }

            
            catch
            {
                
                finalResult = false;
            }
        } // Function End




    }
}
