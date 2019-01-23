using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    public class CampsController : ControllerBase
    {
        private ICampRepository _campRepsoitory;
        private IMapper _mapper;
        private LinkGenerator _linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _campRepsoitory = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await _campRepsoitory.GetAllCampsAsync(includeTalks);

                return _mapper.Map<CampModel[]>(results);
            }
            catch (System.Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,"Database failure");
            }
            
        }
        [HttpGet("{moniker}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await _campRepsoitory.GetCampAsync(moniker);

                if (result == null)
                {
                    return NotFound();
                }
                return _mapper.Map<CampModel>(result);
            }
            catch (System.Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");


            }
        }
        [HttpGet("{moniker}")]
        [MapToApiVersion("1.1")]
        public async Task<ActionResult<CampModel>> Get11(string moniker)
        {
            try
            {
                var result = await _campRepsoitory.GetCampAsync(moniker, true);

                if (result == null)
                {
                    return NotFound();
                }
                return _mapper.Map<CampModel>(result);
            }
            catch (System.Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");


            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate,bool includeTalks = false)
        {
            try
            {
                var results = await _campRepsoitory.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any()) return NotFound();

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }
        }

  
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var location = _linkGenerator.GetPathByAction("Get", "Camps",  new{ moniker= model.Moniker});

                var camp = _mapper.Map<Camp>(model);

                _campRepsoitory.Add(camp);
                if (await _campRepsoitory.SaveChangesAsync())

                    return Created(location, _mapper.Map<CampModel>(camp));
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }
            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>>Put(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await _campRepsoitory.GetCampAsync(model.Moniker);
                if (oldCamp == null) return NotFound();

                _mapper.Map(model, oldCamp);

                if (await _campRepsoitory.SaveChangesAsync())
                    return _mapper.Map<CampModel>(oldCamp);

            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }
            return BadRequest();
        }
        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = _campRepsoitory.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound();

                _campRepsoitory.Delete(oldCamp);

                if (await _campRepsoitory.SaveChangesAsync())
                    return Ok();
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }

            return BadRequest();
        }

    }
}
