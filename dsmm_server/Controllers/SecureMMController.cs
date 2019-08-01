using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSmm.Attributes;
using DSmm.Models;
using DSmm.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using dsweb_electron6.Models;

namespace DSmm.Controllers
{
    [Route("mm")]
    [ServiceFilter(typeof(AuthenticationFilterAttribute))]
    [ApiController]
    public class SecureMMController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMMrepository _mmRepository;

        public SecureMMController(IMMrepository MMRepository, ILogger<SecureDataController> logger)
        {
            _mmRepository = MMRepository;
            _logger = logger;
        }

        [HttpPost("letmeplay")]
        public async Task<BasePlayer> LetmePlay([FromBody]SEplayer seplayer)
        {
            _logger.LogInformation("Getting letmeplay from {0}", seplayer.Name);
            return await _mmRepository.LetmePlay(seplayer);
        }

        [HttpGet("findgame/{id}")]
        public async Task<RetFindGame> FindGame(string id)
        {
            //_logger.LogInformation("Getting findgame from {id}", id
            return await _mmRepository.FindGame(id);
        }

        [HttpGet("exitq/{id}")]
        public async Task<string> Exit(string id)
        {
            _logger.LogInformation("Getting ExitQ from {id}", id);
            return await _mmRepository.ExitQ(id);
        }

        [HttpGet("status/{id}")]
        public async Task<MMgame> Status(string id)
        {
            //_logger.LogInformation("Getting ExitQ from {id}", id);
            return await _mmRepository.Status(id);
        }

        [HttpGet("decline/{name}/{id}")]
        public async Task<string> Decline(string name, string id)
        {
            _logger.LogInformation("Getting decline from {id}", id);
            return await _mmRepository.Decline(name, id);
        }

        [HttpGet("accept/{name}/{id}")]
        public async Task<string> Accept(string name, string id)
        {
            _logger.LogInformation("Getting accept from {id}", id);
            return await _mmRepository.Accept(name, id);
        }

        [HttpGet("deleteme/{id}")]
        public async Task<string> Deleteme(string id)
        {
            _logger.LogInformation("Getting delete from {id}", id);
            return await _mmRepository.Deleteme(id);
        }

        [HttpPost("report/{id}")]
        public async Task<MMgame> Report([FromBody]dsreplay replay, string id)
        {
            _logger.LogInformation("Getting report for {0}", id);
            return await _mmRepository.Report(replay, id);
        }

        [HttpGet("random/{id}")]
        public async Task<string> Random(string id)
        {
            _logger.LogInformation("Getting random from {id}", id);
            return await _mmRepository.Random(id);
        }

        [HttpGet("ladder/{id}")]
        public async Task<DSladder> Ladder(string id)
        {
            _logger.LogInformation("Requesting ladder from {id}", id);
            return await _mmRepository.Ladder(id);
        }

        [HttpPost("manual/{id}")]
        public async Task<IActionResult> Manual(List<IFormFile> files, string id)
        {
            long size = files.Sum(f => f.Length);
            if (size > 10485760) return BadRequest();

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            await _mmRepository.Manual(filePath, id);

            return Ok(new { count = files.Count, size, filePath });
        }
    }

}