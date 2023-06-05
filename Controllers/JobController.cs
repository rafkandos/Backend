using Microsoft.AspNetCore.Mvc;
using CualiVy_CC.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using CualiVy_CC.Services;

namespace CualiVy_CC.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JobController : ControllerBase
{
    private readonly CualiVyContext _context;
    private readonly IConfiguration _configuration;
    private readonly IOCRService _ocrService;
    private readonly ICVService _cvService;
    private readonly InferenceSession _session;
    private readonly IWebHostEnvironment _environment;

    public JobController(CualiVyContext DBContext,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IOCRService ocrService,
            ICVService cvService)
    {
        this._context = DBContext;
        _configuration = configuration;
        _environment = environment;
        _ocrService = ocrService;
        _cvService = cvService;

        // Get the path of the ONNX model file
        string modelFilePath = Path.Combine(_environment.WebRootPath, "modelv2_900.onnx");
        // Load the ONNX model
        _session = new InferenceSession(modelFilePath);
    }

    [HttpPost("SearchJob")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<ActionResult<ReturnAPI>> SearchJob([FromForm] string input)
    {
        var extractedText = _ocrService.GetExtractedText(input);

        // Convert the input string to a byte array
        byte[] textToByte = System.Text.Encoding.UTF8.GetBytes(extractedText);

        int length = 900;
        byte[] inputData = new byte[length];

        // Masking byte length
        if (textToByte.Length > length)
        {
            Array.Copy(textToByte, inputData, length);
        }
        else
        {
            Array.Copy(textToByte, inputData, textToByte.Length);
        }

        // Prepare the input tensor
        string inputName = _session.InputMetadata.Keys.First();
        int inputSize = inputData.Length;

        // Create an array to hold the float values
        float[] inputFloats = new float[inputSize];

        // Convert the byte array to float array
        for (int i = 0; i < inputSize; i++)
        {
            inputFloats[i] = inputData[i];
        }

        // Create the input tensor using the float array
        var inputTensor = new DenseTensor<float>(inputFloats, new int[] { 1, inputData.Length });

        // Create a collection of NamedOnnxValue objects for input
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
        };

        // Prepare the output names
        var outputNames = _session.OutputMetadata.Keys.ToList();

        // Run the model
        var outputs = _session.Run(inputs);

        // Get the output tensor as a float array
        var outputTensor = outputs.First().AsTensor<float>().ToArray();
        int tes = outputTensor.Length;

        string[] labels = {
            "software engineer",
            "ios developer",
            "database engineer",
            "mobile developer",
            "php developer",
            "devops developer",
            "business analyst",
            "software developer",
            "administrator",
            "full stack developer",
            "system engineer",
            "data science",
            "data analyst",
            "front end developer",
            "java developer",
            "react js developer",
            "cyber security",
            "sql developer",
            "machine learning developer",
            "android developer",
            "python developer",
            "web developer"
        };

        // Mengambil 3 indeks dengan probabilitas tertinggi
        int[] topIndices = outputTensor
            .Select((value, index) => new { Value = value, Index = index })
            .OrderByDescending(item => item.Value)
            .Take(3)
            .Select(item => item.Index)
            .ToArray();

        // Mengambil 3 label dengan probabilitas tertinggi
        string[] topLabels = topIndices.Select(index => labels[index]).ToArray();

        // Dictionary<string, float> dict = new Dictionary<string, float>();
        // for (int i = 0; i < outputTensor.Length; i++)
        // {
        //     dict.Add(labels[i], outputTensor[i]);
        // }

        // ResultTestModel res = new ResultTestModel { cat = topLabels, All = dict };

        var rtn = new ReturnAPI();
        var listData = new List<Job>();

        string sqlQuery = "SELECT *, CONCAT('') AS image FROM Job WHERE guid = '' ";

        if (topLabels.Length > 0)
        {
            for (int i = 0; i < topLabels.Length; i++)
            {
                sqlQuery += "OR POSITION LIKE '%" + topLabels[i] + "%' ";
            }
        }

        listData = _context.Job.FromSqlRaw(sqlQuery).ToList();

        List<JobMapping> listJob = new List<JobMapping>();
        foreach (var i in listData)
        {
            JobMapping jm = new JobMapping();
            jm.guid = i.guid;
            jm.position = i.position;
            jm.companyname = i.companyname;
            jm.location = i.location;
            jm.notes = i.notes;
            jm.thirdparty = i.thirdparty;
            jm.image = _cvService.GetImage(i.thirdparty);

            listJob.Add(jm);
        }

        rtn.totalData = listJob.Count;
        rtn.data = listJob;
        return rtn;
    }

    // [HttpPost("search")]
    // public async Task<ActionResult<ReturnAPI>> Post(Search search)
    // {
    //     var rtn = new ReturnAPI();
    //     var listData = new List<Job>();

    //     string sqlQuery = "SELECT *, CONCAT('') AS image FROM Job WHERE guid = '' ";

    //     if (search.skills.Length > 0)
    //     {
    //         for (int i = 0; i < search.skills.Length; i++)
    //         {
    //             sqlQuery += "OR skills LIKE '%" + search.skills[i] + "%' ";
    //         }
    //     }

    //     if (search.experience.Length > 0)
    //     {
    //         for (int i = 0; i < search.experience.Length; i++)
    //         {
    //             string[] divExp = search.experience[i].Split(' ');
    //             for (int j = 0; j < divExp.Length; j++)
    //             {
    //                 sqlQuery += "OR position LIKE '%" + divExp[i] + "%' ";
    //             }
    //         }
    //     }

    //     listData = _context.Job.FromSqlRaw(sqlQuery).ToList();

    //     var config = new MapperConfiguration(cfg => cfg.CreateMap<Job, JobMapping>());
    //     var mapper = new Mapper(config);
    //     List<JobMapping> listJob = mapper.Map<List<JobMapping>>(listData);

    //     rtn.totalData = listJob.Count;
    //     rtn.data = listJob;
    //     return rtn;
    // }

    [HttpGet("get10")]
    public async Task<ActionResult<ReturnAPI>> GetSample()
    {
        var rtn = new ReturnAPI();
        var listData = new List<Job>();

        string sqlQuery = "SELECT *, CONCAT('') AS image FROM Job LIMIT 10";

        listData = _context.Job.FromSqlRaw(sqlQuery).ToList();

        List<JobMapping> listJob = new List<JobMapping>();
        foreach (var i in listData)
        {
            JobMapping jm = new JobMapping();
            jm.guid = i.guid;
            jm.position = i.position;
            jm.companyname = i.companyname;
            jm.location = i.location;
            jm.notes = i.notes;
            jm.thirdparty = i.thirdparty;
            jm.image = _cvService.GetImage(i.thirdparty);

            listJob.Add(jm);
        }

        rtn.totalData = listJob.Count;
        rtn.data = listJob;
        return rtn;
    }

    [HttpGet("Detail/{id}")]
    public async Task<ActionResult<ReturnAPI>> Detail(string id)
    {
        var rtn = new ReturnAPI();
        var dt = new Job();

        string sqlQuery = "SELECT *, CONCAT('') AS image FROM Job WHERE guid = '" + id + "'";

        dt = _context.Job.FromSqlRaw(sqlQuery).FirstOrDefault();
        dt.image = _cvService.GetImage(dt.thirdparty);

        //rtn.totalData = listJob.Count;
        rtn.data = dt;
        return rtn;
    }
}