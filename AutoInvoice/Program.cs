using Microsoft.Extensions.Configuration;
using System.Text;

Console.WriteLine("=================================");
Console.WriteLine(" Auto Invoice Console Started");
Console.WriteLine("=================================");

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

bool enabled = configuration.GetValue<bool>("AutoInvoice:Enabled");
int interval = configuration.GetValue<int>("AutoInvoice:IntervalMinutes");
int intervalSec = configuration.GetValue<int>("AutoInvoice:IntervalSecond");

Console.WriteLine($"Enabled : {enabled}");
//Console.WriteLine($"Interval: {interval} Minute(s)");
Console.WriteLine($"Interval in Sececond: {intervalSec} Second(s)");

HttpClient client = new HttpClient();

client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]);

while (enabled)
{
    Console.WriteLine("----------------------------------------");
    Console.WriteLine("Checking Due Invoices...");
    Console.WriteLine(DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt"));
    Console.WriteLine("----------------------------------------");

    // Actual Work Start here *******************
    try
    {
        var response = await client.PostAsync("BillingRevenue/AutoGenerateInvoice", new StringContent("", Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Count: (Total Invoice Inserted) " + json);
        }
        else
        {
            Console.WriteLine("API Error : " + response.StatusCode);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }



    // Actual Work End here *******************

    //await Task.Delay(TimeSpan.FromMinutes(interval));
    await Task.Delay(TimeSpan.FromSeconds(intervalSec));
}
