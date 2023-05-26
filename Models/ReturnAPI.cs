public class ReturnAPI
{
    public int status { get; set; } = 200;
    public string message { get; set; } = "Success";
    public object data { get; set; }
    public int totalData { get; set; }
}