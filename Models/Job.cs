using Microsoft.EntityFrameworkCore;

namespace CualiVy_CC.Models;

public class Job
{
    public Guid guid { get; set; }
    public string position { get; set; }
    public string kindofwork { get; set; }
    public string companyname { get; set; }
    public string location { get; set; }
    public string education { get; set; }
    public string major { get; set; }
    public string description { get; set; }
    public string thirdparty { get; set; }
    public string notes { get; set; }
    public int? minimumyears { get; set; }
    public string skills { get; set; }
    public string link { get; set; }
    public DateTime? createdat { get; set; }
    public DateTime? updatedat { get; set; }
}

public class JobMapping
{
    public Guid guid { get; set; }
    public string position { get; set; }
    public string companyname { get; set; }
    public string location { get; set; }
    public string notes { get; set; }
    public string thirdparty { get; set; }
}

public class Search
{
    public string[] experience { get; set; }
    public string[] education { get; set; }
    public string[] location { get; set; }
    public string[] major { get; set; }
    public string[] skills { get; set; }
    public int minimumyears { get; set; }
}