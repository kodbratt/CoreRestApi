using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private ICampRepository _campRespository;
        private IMapper _mapper;
        private LinkGenerator _linkGenerator;
        
        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _campRespository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await _campRespository.GetTalksByMonikerAsync(moniker,true);
                return _mapper.Map<TalkModel[]>(talks);
               
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await _campRespository.GetTalkByMonikerAsync(moniker, id,true);
                return _mapper.Map<TalkModel>(talk);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }
        }
        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model )
        {
            try
            {
                var camp = await _campRespository.GetCampAsync(moniker);
                if (camp == null) return BadRequest();

                var talk = _mapper.Map<Talk>(model);
                talk.Camp = camp;
                _campRespository.Add(talk);

                if (model.Speaker == null) return BadRequest();
                var speaker = await _campRespository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest();

                talk.Speaker = speaker;

                if (await _campRespository.SaveChangesAsync())
                {
                    var url = _linkGenerator.GetPathByAction(HttpContext,
                        "Get",
                        values: new { moniker, id = talk.TalkId });

                    return Created(url, _mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
