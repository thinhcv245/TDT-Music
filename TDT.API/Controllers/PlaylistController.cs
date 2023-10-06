﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TDT.Core.Helper;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TDT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        // GET: api/<PlaylistController>
        [Route("release")]
        [HttpGet]
        public JsonResult release()
        {
            var list = FirebaseService.Instance.getPlaylistRelease().Result;
            list = list.OrderByDescending(x => long.Parse(x.Object.releasedAt)).Take(10).ToList();
            return new JsonResult(list);
        }

        // GET api/<PlaylistController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PlaylistController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PlaylistController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PlaylistController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
