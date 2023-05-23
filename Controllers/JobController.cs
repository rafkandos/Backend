using Microsoft.AspNetCore.Mvc;
using CualiVy_CC.Models;
using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace CualiVy_CC.Controllers;

[Authorize]
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

        var byExp = _context.Job.Where(e => search.experience.Contains(e.position)).ToList();
        var byEdu = _context.Job.Where(e => search.education.Contains(e.education)).ToList();
        var byLoc = _context.Job.Where(e => search.location.Contains(e.location)).ToList();
        var byMajor = _context.Job.Where(e => search.major.Contains(e.major)).ToList();
        var bySkills = _context.Job.Where(e => search.skills.Contains(e.skills)).ToList();
        var byMinYears = _context.Job.Where(e => e.minimumyears >= search.minimumyears).ToList();

        listData.AddRange(byExp);
        listData.AddRange(byEdu);
        listData.AddRange(byLoc);
        listData.AddRange(byMajor);
        listData.AddRange(bySkills);
        listData.AddRange(byMinYears);

        var config = new MapperConfiguration(cfg => cfg.CreateMap<Job, JobMapping>());
        var mapper = new Mapper(config);
        List<JobMapping> listJob = mapper.Map<List<JobMapping>>(listData);

        rtn.data = listJob;
        return rtn;
    }
}