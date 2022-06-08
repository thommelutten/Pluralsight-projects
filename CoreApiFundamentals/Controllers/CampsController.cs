using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _campRespotiry;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _campRespotiry = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await _campRespotiry.GetAllCampsAsync(includeTalks);

                CampModel[] models = _mapper.Map<CampModel[]>(results);

                return Ok(models);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await _campRespotiry.GetCampAsync(moniker);

                if (result == null) return NotFound();

                return _mapper.Map<CampModel>(result);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _campRespotiry.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any()) return NotFound();

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var campExist = await _campRespotiry.GetCampAsync(model.Moniker);
                if (campExist != null)
                {
                    return BadRequest("Moniker in use");
                }

                var location = _linkGenerator.GetPathByAction("Get",
                    "Camps",
                    new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location)) return BadRequest("Could not use current moniker");

                var camp = _mapper.Map<Camp>(model);
                _campRespotiry.Add(camp);
                if(await _campRespotiry.SaveChangesAsync())
                {
                    return Created(location, _mapper.Map<CampModel>(camp));
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel campModel)
        {
            try
            {
                var oldCamp = await _campRespotiry.GetCampAsync(campModel.Moniker);
                if (oldCamp == null) return NotFound($"Could not find camp with moniker of {moniker}");

                _mapper.Map(campModel, oldCamp);

                if(await _campRespotiry.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await _campRespotiry.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound($"Could not find camp with moniker of {moniker}");

                _campRespotiry.Delete(oldCamp);

                if (await _campRespotiry.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest("Failed to delete the camp");
        }

    }
}
