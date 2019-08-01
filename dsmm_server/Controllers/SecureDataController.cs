﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSmm.Attributes;
using DSmm.Models;
using DSmm.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DSmm.Controllers
{
    [ServiceFilter(typeof(AuthenticationFilterAttribute))]
    [ApiController]
    public class SecureDataController : ControllerBase
    {
        Regex r = new Regex("[^A-Za-z0-9]$");

        private readonly IDataRepository _dataRepository;
        private readonly ILogger _logger;

        public SecureDataController(IDataRepository dataRepository, ILogger<SecureDataController> logger)
        {
            _dataRepository = dataRepository;
            _logger = logger;
        }

        [HttpGet("secure/data/last/{last}/{id}")]
        public async Task<IActionResult> GetLast(string id, string last)
        {
            _logger.LogInformation("Getting info from {id}", id);
            if (id.Length != 64) return BadRequest("Wrong id.");
            if (r.IsMatch(id)) return BadRequest("Wrong id.");
            return Ok(_dataRepository.GetLast(id, last));
        }

        [HttpPost("secure/data/info")]
        public async Task<IActionResult> GetInfo([FromBody]DSinfo info)
        {
            _logger.LogInformation("Getting info from {id}", info.Name);
            if (info.Name.Length != 64) return BadRequest("Wrong id.");
            if (r.IsMatch(info.Name)) return BadRequest("Wrong id.");
            return Ok(_dataRepository.Info(info));
        }

        [HttpPost("secure/data/upload/{id}")]
        public async Task<IActionResult> Upload(string id)
        {
            _logger.LogInformation("Getting data from {id}", id);
            if (id.Length != 64) return BadRequest("Wrong id.");
            if (r.IsMatch(id)) return BadRequest("Wrong id.");

            if (Request.Form.Files.Count != 1) return BadRequest("We need one file.");
            long size = Request.Form.Files.Sum(f => f.Length);
            if (size > 10485760) return BadRequest("File Size.");

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in Request.Form.Files)
            {
                
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                } 
                bool fileok = await _dataRepository.GetFile(id, filePath);
                if (fileok == false)
                {
                    return BadRequest("Wrong file.");
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok();
        }
    }
}