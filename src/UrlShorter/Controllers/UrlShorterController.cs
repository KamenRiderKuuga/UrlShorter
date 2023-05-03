using FASTER.core;
using Microsoft.AspNetCore.Mvc;
using Standart.Hash.xxHash;
using System.Text.Json;
using System.Threading.Tasks;
using UrlShorter.Models;
using UrlShorter.Utils;

namespace UrlShorter.Controllers
{
    [ApiController]
    public class UrlShorterController : ControllerBase
    {
        private readonly FasterKV<long, string> _store;

        public UrlShorterController(FasterKV<long, string> store)
        {
            _store = store;
        }

        private const string DUPLICATED_FLAG = "[DUPLICATED]";

        /// <summary>
        /// Generate a short code from the url
        /// </summary>
        /// <param name="urlGenerateDto"></param>
        /// <returns></returns>
        [HttpPost("[controller]")]
        public async Task<IActionResult> Generate([FromBody] UrlDto urlGenerateDto)
        {
            if (urlGenerateDto.Url?.Length < 10240 && (urlGenerateDto.Url.StartsWith("http://") || urlGenerateDto.Url.StartsWith("https://")))
            {
                using var session = _store.For(new SimpleFunctions<long, string>()).NewSession<SimpleFunctions<long, string>>();
                {
                    var shortCode = xxHash32.ComputeHash(urlGenerateDto.Url + urlGenerateDto.AppendData ?? string.Empty);
                    var (status, output) = (await session.ReadAsync(shortCode)).Complete();
                    if (status.NotFound)
                    {
                        await (await session.UpsertAsync(shortCode, JsonSerializer.Serialize(urlGenerateDto))).CompleteAsync();
                        return Ok(UrlUtil.Base62Encode(shortCode));
                    }
                    else
                    {
                        var urlDto = JsonSerializer.Deserialize<UrlDto>(output);
                        if (urlGenerateDto.Url == urlDto.Url)
                        {
                            return Ok(UrlUtil.Base62Encode(shortCode));
                        }
                        else
                        {
                            urlGenerateDto.AppendData = (urlGenerateDto.AppendData ?? string.Empty) + DUPLICATED_FLAG;
                            return await Generate(urlGenerateDto);
                        }
                    }
                }
            }
            return BadRequest("The input url is invalid");
        }

        /// <summary>
        /// Get the original url by short code
        /// </summary>
        /// <param name="shortCode"></param>
        /// <returns></returns>
        [HttpGet("{shortCode}")]
        public async Task<IActionResult> Get(string shortCode)
        {
            if (!string.IsNullOrEmpty(shortCode))
            {
                var shortCodeNumber = UrlUtil.Base62Decode(shortCode);
                using var session = _store.For(new SimpleFunctions<long, string>()).NewSession<SimpleFunctions<long, string>>();
                {
                    var (status, output) = (await session.ReadAsync(shortCodeNumber)).Complete();
                    if (!status.NotFound)
                    {
                        var urlDto = JsonSerializer.Deserialize<UrlDto>(output);
                        return Redirect(urlDto.Url);
                    }
                }
            }

            return NotFound();
        }
    }
}