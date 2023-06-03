using Microsoft.AspNetCore.Mvc;
using CualiVy_CC.Models;
using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Tesseract;

namespace CualiVy_CC.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JobController : ControllerBase
{
    private readonly CualiVyContext _context;
    private readonly IConfiguration _configuration;
    private readonly InferenceSession _session;
    private readonly IWebHostEnvironment _environment;

    public JobController(CualiVyContext DBContext, IConfiguration configuration, IWebHostEnvironment environment)
    {
        this._context = DBContext;
        _configuration = configuration;
        _environment = environment;

        // Get the path of the ONNX model file
        string modelFilePath = Path.Combine(_environment.WebRootPath, "modelcv.onnx");
        // Load the ONNX model
        _session = new InferenceSession(modelFilePath);
    }

    [HttpPost]
    [Consumes("application/x-www-form-urlencoded")]
    public IActionResult ExtractText([FromForm] string input)
    {
        try
        {
            // Decode Base64 string to byte array
            byte[] imageBytes = Convert.FromBase64String(input);

            // Create Pix object from byte array
            using (var ms = new MemoryStream(imageBytes))
            using (var image = Pix.LoadFromMemory(imageBytes))
            {
                // Create a Tesseract instance and set the language
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    // Set page segmentation mode
                    engine.SetVariable("tessedit_pageseg_mode", "6");

                    // Set the image to be recognized
                    using (var page = engine.Process(image))
                    {
                        // Get the extracted text
                        string extractedText = page.GetText();

                        return Ok(new { ExtractedText = extractedText.Replace("\n", " ").Replace("\r", " ") });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { Error = ex.Message });
        }
    }

    [HttpPost("TestModel")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<ActionResult<ReturnAPI>> TestModel([FromForm] string input)
    {
        // Convert the input string to a byte array
        byte[] inputData = System.Text.Encoding.UTF8.GetBytes(input);

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
        var inputTensor = new DenseTensor<float>(inputFloats, new int[] { 1, 100 });

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
        string[] labels = new string[]
        {
            "accountancy", "accountancyqualified", "adminsecretarialpa", "apprenticeships", "banking", "catering", "charity", "constructionproperty",
            "customerservice", "education", "energy", "engineering", "estateagent", "factory", "finance", "fmcg", "generalinsurance",
            "graduatetraininginternships", "health", "hr", "it", "law", "leisuretourism", "logistics", "marketing", "mediadigitalcreative",
            "motoringautomotive", "other", "purchasing", "recruitmentconsultancy", "retail", "sales", "science", "securitysafety",
            "socialcare", "strategyconsultancy", "training"
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

        // Convert the float array back to a string
        //string outputString = System.Text.Encoding.UTF8.GetString(outputTensor.Select(f => (byte)f).ToArray());

        return Ok(topLabels);
    }

    [HttpPost("search")]
    public async Task<ActionResult<ReturnAPI>> Post(Search search)
    {
        var rtn = new ReturnAPI();
        var listData = new List<Job>();

        string sqlQuery = "SELECT * FROM Job WHERE guid = '' ";

        if (search.skills.Length > 0)
        {
            for (int i = 0; i < search.skills.Length; i++)
            {
                sqlQuery += "OR skills LIKE '%" + search.skills[i] + "%' ";
            }
        }

        if (search.experience.Length > 0)
        {
            for (int i = 0; i < search.experience.Length; i++)
            {
                string[] divExp = search.experience[i].Split(' ');
                for (int j = 0; j < divExp.Length; j++)
                {
                    sqlQuery += "OR position LIKE '%" + divExp[i] + "%' ";
                }
            }
        }

        listData = _context.Job.FromSqlRaw(sqlQuery).ToList();

        var config = new MapperConfiguration(cfg => cfg.CreateMap<Job, JobMapping>());
        var mapper = new Mapper(config);
        List<JobMapping> listJob = mapper.Map<List<JobMapping>>(listData);

        rtn.totalData = listJob.Count;
        rtn.data = listJob;
        return rtn;
    }

    [HttpGet("get10")]
    public async Task<ActionResult<ReturnAPI>> GetSample()
    {
        var rtn = new ReturnAPI();
        var listData = new List<Job>();

        string sqlQuery = "SELECT * FROM Job LIMIT 10";

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
            jm.image = i.thirdparty;

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

        string sqlQuery = "SELECT * FROM Job WHERE guid = '" + id + "'";

        dt = _context.Job.FromSqlRaw(sqlQuery).FirstOrDefault();

        //rtn.totalData = listJob.Count;
        rtn.data = dt;
        return rtn;
    }
}