using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SelenuimTests;

[TestFixture]
public class SeleniumTests
{
    private const string BaseUrl = "https://the-internet.herokuapp.com";
    private IWebDriver driver;
    private WebDriverWait wait;

    [SetUp]
    public void Setup()
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5)); 
    }

    [Test]
    public void AddRemove()
    {
        driver.Navigate().GoToUrl(BaseUrl + "/add_remove_elements/");

        IWebElement addButton = wait.Until(d => d.FindElement(By.XPath("//button[text()='Add Element']")));
        addButton.Click();

        IWebElement delButton = wait.Until(d => d.FindElement(By.ClassName("added-manually")));
        Assert.That(delButton.Text, Is.EqualTo("Delete"));

        delButton.Click();

        var remainingDeleteButtons = driver.FindElements(By.ClassName("added-manually"));
        Assert.That(remainingDeleteButtons, Is.Empty);
    }

    [Test]
    public void Checkboxes()
    {
        driver.Navigate().GoToUrl(BaseUrl + "/checkboxes");

        IWebElement checkbox1 = wait.Until(d => d.FindElement(By.CssSelector("#checkboxes input:first-of-type")));
        Assert.That(checkbox1.Selected, Is.False);

        checkbox1.Click();
        Assert.That(checkbox1.Selected, Is.True);

        string? checkedAttribute = checkbox1.GetAttribute("checked");
        Assert.That(checkedAttribute, Is.Not.Null);
    }

    [Test]
    public void Dropdown()
    {
        driver.Navigate().GoToUrl(BaseUrl + "/dropdown");

        IWebElement dropdownElement = wait.Until(d => d.FindElement(By.Id("dropdown")));
        Assert.That(dropdownElement.GetAttribute("disabled"), Is.Null);
        
        SelectElement select = new SelectElement(dropdownElement);

        select.SelectByValue("1");
        Assert.That(select.SelectedOption.GetAttribute("value"), Is.EqualTo("1"));

        select.SelectByValue("2");
        Assert.That(select.SelectedOption.GetAttribute("value"), Is.EqualTo("2"));
    }

    [Test]
    public void Inputs()
    {
        driver.Navigate().GoToUrl(BaseUrl + "/inputs");
        
        IWebElement input = wait.Until(d => d.FindElement(By.CssSelector("input[type='number']")));

        input.SendKeys("text");
        Assert.That(input.GetAttribute("value"), Is.Empty);

        input.Clear();
        input.SendKeys("1234567");
        Assert.That(input.GetAttribute("value"), Is.EqualTo("1234567"));
    }

    [Test]
    public void StatusCodes()
    {
        driver.Navigate().GoToUrl(BaseUrl + "/status_codes");
        
        IWebElement link200 = wait.Until(d => d.FindElement(By.CssSelector("a[href='status_codes/200']")));
        link200.Click();

        wait.Until(d => d.Url == BaseUrl + "/status_codes/200");
        Assert.That(driver.Url, Is.EqualTo(BaseUrl + "/status_codes/200"));

        driver.Navigate().Back();
        wait.Until(d => d.Url == BaseUrl + "/status_codes");
        Assert.That(driver.Url, Is.EqualTo(BaseUrl + "/status_codes"));
    }

    [Test]
    public void DynamicControls()
    {
        driver.Navigate().GoToUrl(BaseUrl + "/dynamic_controls");

        IWebElement removeAddButton = wait.Until(d => d.FindElement(By.CssSelector("button[onclick='swapCheckbox()']")));
        removeAddButton.Click();
        
        wait.Until(d => d.FindElements(By.CssSelector("#checkbox")).Count == 0);
        IWebElement mes = wait.Until(d => d.FindElement(By.Id("message")));
        Assert.That(mes.Text, Is.EqualTo("It's gone!"));

        IWebElement inp = wait.Until(d => d.FindElement(By.CssSelector("input[type='text']")));
        Assert.That(inp.Enabled, Is.False);

        IWebElement enableButton = wait.Until(d => d.FindElement(By.CssSelector("button[onclick='swapInput()']")));
        enableButton.Click();
        
        IWebElement mes2 = wait.Until(d => d.FindElement(By.Id("message")));
        wait.Until(d => d.FindElement(By.CssSelector("input[type='text']")).Enabled);
        Assert.That(mes2.Text, Is.EqualTo("It's enabled!"));
    }

    [Test]
    public void FileUpload()
    {
        driver.Navigate().GoToUrl(BaseUrl + "/upload");

        IWebElement uploader = wait.Until(d => d.FindElement(By.Id("file-upload")));
        string absolutePath = Path.GetFullPath("test_file.txt");
        uploader.SendKeys(absolutePath);

        IWebElement submitter = wait.Until(d => d.FindElement(By.Id("file-submit")));
        submitter.Click();
        
        wait.Until(d => d.FindElement(By.Id("uploaded-files")));

        var uploadedElements = driver.FindElements(By.Id("uploaded-files"));
        Assert.That(uploadedElements.Count, Is.EqualTo(1));

        driver.Navigate().Back();
        
        IWebElement dropzone = wait.Until(d => d.FindElement(By.Id("drag-drop-upload")));
        
        string script = @"
        var target = arguments[0];
        var input = document.createElement('input');
        input.type = 'file';
        input.style.display = 'none';
        input.onchange = function () {
            var dataTransfer = new DataTransfer();
            for (var i = 0; i < this.files.length; i++) {
                dataTransfer.items.add(this.files[i]);
            }
            var event = new DragEvent('drop', { bubbles: true, cancelable: true, dataTransfer: dataTransfer });
            target.dispatchEvent(event);
        };
        document.body.appendChild(input);
        return input;
        ";
        
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        IWebElement hiddenInput = (IWebElement)js.ExecuteScript(script, dropzone)!;
        
        hiddenInput.SendKeys(absolutePath);
        
        IWebElement uploadedFileName = wait.Until(d => d.FindElement(By.CssSelector("#drag-drop-upload .dz-filename span")));
        Assert.That(uploadedFileName.Text, Is.EqualTo("test_file.txt"));
    }

    [Test]
    public void JavaScriptAlerts()
    {
        driver.Navigate().GoToUrl(BaseUrl + "/javascript_alerts");
        
        IWebElement result = wait.Until(d => d.FindElement(By.Id("result")));
        
        IWebElement alert = wait.Until(d => d.FindElement(By.CssSelector("button[onclick='jsAlert()']")));
        alert.Click();
        IAlert jsAlert = driver.SwitchTo().Alert();
        
        Assert.That(jsAlert.Text, Is.EqualTo("I am a JS Alert"));
        jsAlert.Accept();
        Assert.That(result.Text, Is.EqualTo("You successfully clicked an alert"));
        
        IWebElement confirm = wait.Until(d => d.FindElement(By.CssSelector("button[onclick='jsConfirm()']")));
        confirm.Click();
        
        jsAlert = driver.SwitchTo().Alert();
        Assert.That(jsAlert.Text, Is.EqualTo("I am a JS Confirm"));
        jsAlert.Dismiss();
        Assert.That(result.Text, Is.EqualTo("You clicked: Cancel"));
        
        IWebElement prompt = wait.Until(d => d.FindElement(By.CssSelector("button[onclick='jsPrompt()']")));
        prompt.Click();
        
        jsAlert = driver.SwitchTo().Alert();
        jsAlert.SendKeys("Prompt Alert");
        Assert.That(jsAlert.Text, Is.EqualTo("I am a JS prompt"));
        jsAlert.Accept();
        Assert.That(result.Text, Is.EqualTo("You entered: Prompt Alert"));
        
        prompt.Click();
        jsAlert = driver.SwitchTo().Alert();
        jsAlert.Dismiss();
        Assert.That(result.Text, Is.EqualTo("You entered: null"));
    }

    [Test]
    public void MultipleWindows()
    {
        driver.Navigate().GoToUrl(BaseUrl + "/windows");
        
        IWebElement clicker = wait.Until(d => d.FindElement(By.CssSelector("a[href='/windows/new']")));
        clicker.Click();
        
        driver.SwitchTo().Window(driver.WindowHandles.Last());
        wait.Until(d => d.Url == "https://the-internet.herokuapp.com/windows/new");
        Assert.That(driver.Url, Is.EqualTo(BaseUrl + "/windows/new"));
        
        IWebElement title = wait.Until(d => d.FindElement(By.CssSelector("h3")));
        Assert.That(title.Text, Is.EqualTo("New Window"));
    }

    [Test]
    public void NotificationMessages()
    {
        driver.Navigate().GoToUrl(BaseUrl + "/notification_message_rendered");
        
        IWebElement loader = wait.Until(d => d.FindElement(By.CssSelector("a[href='/notification_message']")));
        loader.Click();
        
        wait.Until(d => d.FindElement(By.Id("flash")));
        
        IWebElement flashCloser = wait.Until(d => d.FindElement(By.CssSelector("#flash a.close")));
        flashCloser.Click();
        
        wait.Until(d => d.FindElements(By.Id("flash")).Count == 0);
    }
    
    [TearDown]
    public void TearDown()
    {
        driver.Quit();
    }
}