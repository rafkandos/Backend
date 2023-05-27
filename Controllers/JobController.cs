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

namespace CualiVy_CC.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JobController : ControllerBase
{
    private readonly CualiVyContext _context;
    private readonly IConfiguration _configuration;

    public JobController(CualiVyContext DBContext, IConfiguration configuration)
    {
        this._context = DBContext;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<ReturnAPI>> Get()
    {
        var rtn = new ReturnAPI();
        var listData = _context.Job.Where(e => e.thirdparty == "LinkedIn").ToList();

        var config = new MapperConfiguration(cfg => cfg.CreateMap<Job, JobMapping>());
        var mapper = new Mapper(config);
        List<JobMapping> listJob = mapper.Map<List<JobMapping>>(listData);

        rtn.data = listJob;
        return rtn;
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

        var config = new MapperConfiguration(cfg => cfg.CreateMap<Job, JobMapping>());
        var mapper = new Mapper(config);
        List<JobMapping> listJob = mapper.Map<List<JobMapping>>(listData);

        rtn.totalData = listJob.Count;
        rtn.data = listJob;
        return rtn;
    }

    [HttpPost("Detail")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<ActionResult<ReturnAPI>> Detail([FromForm] Detail det)
    {
        var rtn = new ReturnAPI();
        var dt = new Job();

        string sqlQuery = "SELECT * FROM Job WHERE guid = '" + det.guid + "'";

        dt = _context.Job.FromSqlRaw(sqlQuery).FirstOrDefault();

        //rtn.totalData = listJob.Count;
        rtn.data = dt;
        return rtn;
    }
}