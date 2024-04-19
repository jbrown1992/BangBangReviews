// See https://aka.ms/new-console-template for more information
using AngleSharp;
using AngleSharp.Dom;
using System.Text.RegularExpressions;
using System.Web;


try
{
    var config = Configuration.Default.WithDefaultLoader();
    var address = "https://www.bangbangoriental.com/foodhall";
    var context = BrowsingContext.New(config);
    var document = await context.OpenAsync(address);

    IEnumerable<IElement> sqsContent = document.All.Where(m => m.ClassList.Contains("sqs-html-content"));

    var vendors = new List<string>();
    var listOfVendorReviews = new List<VendorReview>();
    foreach (var item in sqsContent)
    {
        vendors.Add(item.TextContent);

        var vendor = item.TextContent;
        if(vendor.Contains("FOODHALL.Choose your favourite Asian cuisine") 
            || vendor.Contains("OUR VENDORS")
            || vendor.Contains("399 EDGWARE ROADCOLINDALELONDON NW9 0FH")
            || vendor.Contains("INFO@BANGBANGORIENTAL.COMBECOME A TENANT")
            || vendor.Contains("© 2023 Bang Bang Oriental Foodhall (K.S)")) {
            continue;
        }
        string replacement = Regex.Replace(vendor, @"\t|\n|\r", "");
        string v1 = replacement.Replace(" ", "+");

        var gAddress = $"https://www.google.com/search?q={v1}+bang+bang+google+review";
        //var gAddress = $"https://www.google.com/search?q=r{v1}+bang+bang+uber+eats+review";

        var gContext = BrowsingContext.New(config);
        var gDocument = await context.OpenAsync(gAddress);
        IEnumerable<IElement> googleClass1 = gDocument.All.Where(m => m.ClassList.Contains("kp-header"));

        IEnumerable<IElement> googleClass2 = gDocument.All.Where(m => m.ClassList.Contains("tP9Zud"));

        //get highest review count
        var maxCount = 0;
        var largestReviewIndex = 0;
        for (int i = 0; i < googleClass2.Count(); i++)
        {
            var array = googleClass2.ToArray();
            var rating = array[i].TextContent;
            if(!rating.Contains(")")) {
                continue;
            }

            if (rating.Contains("/"))
            {
                continue;
            }

            var count = Int32.Parse(rating.Split(' ')[3].Replace("(", "").Replace(")", "").Replace(",", ""));
            if(count >maxCount)
            {
                maxCount = count;
                largestReviewIndex = i;
            }

        }


            var bestRating = googleClass2.ToArray()[largestReviewIndex].TextContent;


                var vendorResult = new VendorReview();
                vendorResult.Name = replacement;
                vendorResult.Rating = Double.Parse(bestRating.Split(' ')[1]);
                vendorResult.NumberOfRatings = Int32.Parse(bestRating.Split(' ')[3].Replace("(", "").Replace(")", "").Replace(",", ""));
                listOfVendorReviews.Add(vendorResult);
                //Console.WriteLine(replacement + " " + rating);
    
        
        //gDocument.All.Where(m => m.Cont.Contains("Aq14fc"));
    }


    var sorted = listOfVendorReviews.OrderByDescending(o => o.Rating).ToList();

    foreach (var v in sorted)
    {
        Console.WriteLine(v.Name + "  " + v.Rating + " (" +v.NumberOfRatings+")");

    }
}
catch (HttpRequestException e)
{
    Console.WriteLine("\nException Caught!");
    Console.WriteLine("Message :{0} ", e.Message);
}

public class VendorReview
{
    public string Name { get; set; }
    public double Rating { get; set; }
    public int NumberOfRatings { get; set; }
}