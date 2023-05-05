using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Drawing;
using System.IO.Compression;

var rand = new Random();
var chars = "abcdefghijklmnopqrstuvwxyz.,;     ".ToCharArray();


string GetRandomString(Random rand, char[] chars)
{
    var size = rand.NextInt64(500, 1_000);
    var data = new char[size];
    for (var j = 0; j < size; j++)
    {
        data[j] = chars[rand.NextInt64(chars.Length)];
    }
    return new String(data);
}

byte[] GetRandomImage(Random rand, int width, int height)
{
    var memoryStream = new MemoryStream();

    var canvas = new System.Drawing.Bitmap(width, height);
    for (var x = 0; x < width; x++)
    {
        for (var y = 0; y < height; y++)
        {
            var c = Color.FromArgb(rand.Next());
            canvas.SetPixel(x, y, c);
        }
    }
    canvas.SaveJpeg(memoryStream, 80);
    return memoryStream.ToArray();
}

long.TryParse(args[0], out long targetCount);
Console.WriteLine($"Generating {targetCount} files");
var accountNumbers = args.Skip(1).ToList();
if (accountNumbers.Count == 0)
{
    accountNumbers.Add("1000");
}
Console.WriteLine($"Using Account(s): {string.Join(',', accountNumbers)}");

var fileName = "media-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
var zipFileName = $"output/{fileName}.zip";
Console.WriteLine($"Saving output to: {zipFileName}");

if (File.Exists("output/manifest.csv"))
    File.Delete("output/manifest.csv");
if (File.Exists($"{zipFileName}"))
    File.Delete($"{zipFileName}");
using var manifest = new StreamWriter(new FileStream("output/manifest.csv", FileMode.Create));
manifest.WriteLine("AccountId,Type,Year,Month,FileName");
int year = 2022;
int month = 1;
int i = 0;
int j = 0;
while (i < targetCount)
{
    i++;
    HttpClient client = new HttpClient();
    var document = Document.Create(
    container =>
    {
        container.Page(page =>
        {
            page
                .Content()
                .Column(column =>
                {
                    for (var x = 0; x < rand.Next(20, 50); x++)
                    {
                        var image = GetRandomImage(rand, rand.Next(320, 640), rand.Next(240, 320));
                        var str = GetRandomString(rand, chars);
                        column.Item()
                            .Padding(10)
                            .Image(image, ImageScaling.FitArea);
                        column.Item()
                            .PaddingHorizontal(10)
                            .Text(str);
                    }
                });
        });
    });

    string entryName = $"{i}.pdf";
    string pdfFile = $"output/last.pdf";
    document.GeneratePdf(pdfFile);

    using (var output = ZipFile.Open($"{zipFileName}", ZipArchiveMode.Update))
    {
        output.CreateEntryFromFile(pdfFile, entryName);
    }
    string accountNumber = accountNumbers[j++ % accountNumbers.Count];
    manifest.WriteLine($"{accountNumber},Billing Statement,{year},{month},{entryName}");

    Console.WriteLine($"{accountNumber} - {entryName}");

    month++;
    if (month == 13)
    {
        month = 1;
        year++;
    }
}
manifest.Flush();
manifest.Close();

try
{
    using (var output = ZipFile.Open($"{zipFileName}", ZipArchiveMode.Update))
    {
        output.CreateEntryFromFile("output/manifest.csv", $"{fileName}.csv");
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
Console.WriteLine("Done");