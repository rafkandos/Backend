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

        // var newList = new List<Job>();
        // for (int i = 0; i < search.skills.Length; i++)
        // {
        //     newList.AddRange(listData.Where(e => search.skills[i].Contains(e.skills)).ToList());
        // }

        // newList = newList.Distinct().ToList();

        // var byExp = _context.Job.Where(e => search.experience.Contains(e.position)).ToList();
        // var byEdu = _context.Job.Where(e => search.education.Contains(e.education)).ToList();
        // var byLoc = _context.Job.Where(e => search.location.Contains(e.location)).ToList();
        // var byMajor = _context.Job.Where(e => search.major.Contains(e.major)).ToList();

        // var bySkills = _context.Job.ToList()
        //                 .AsEnumerable()
        //                 .Where(e => search.skills.Any(x => e.skills.Contains(x)))
        //                 .ToList();
        // // var bySkills = new List<Job>();
        // // for (int i = 0; i < search.skills.Length; i++)
        // // {
        // //     bySkills.AddRange(_context.Job.Where(e => search.skills[i].Contains(e.skills)).ToList());
        // // }

        // var byMinYears = _context.Job.Where(e => e.minimumyears >= search.minimumyears).ToList();

        // listData.AddRange(byExp);
        // listData.AddRange(byEdu);
        // listData.AddRange(byLoc);
        // listData.AddRange(byMajor);
        // listData.AddRange(bySkills);
        // listData.AddRange(byMinYears);

        // listData = listData.Distinct().ToList();

        // List<Job> newList = new List<Job>();
        // foreach (var i in listData)
        // {
        //     if (search.skills.Contains(i.skills))
        //     {
        //         if (search.education.Contains(i.education))
        //         {
        //             newList.Add(i);
        //         }
        //     }
        //     // if (search.experience.Contains(i.position))
        //     // {
        //     //     if (search.education.Contains(i.education))
        //     //     {
        //     //         newList.Add(i);
        //     //         // if (search.skills.Contains(i.skills))
        //     //         // {
        //     //         //     newList.Add(i);
        //     //         // }
        //     //         // else if (search.location.Contains(i.location))
        //     //         // {
        //     //         //     newList.Add(i);
        //     //         // }
        //     //     }
        //     // }
        // }

        var config = new MapperConfiguration(cfg => cfg.CreateMap<Job, JobMapping>());
        var mapper = new Mapper(config);
        List<JobMapping> listJob = mapper.Map<List<JobMapping>>(listData);

        rtn.totalData = listJob.Count;
        rtn.data = listJob;
        return rtn;
    }
}