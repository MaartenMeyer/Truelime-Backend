using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TruelimeBackend.Models;
using TruelimeBackend.Services;

namespace TruelimeBackend.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly CardService cardService;

        public CardsController(CardService cardService)
        {
            this.cardService = cardService;
        }

        [HttpGet]
        public ActionResult<List<Card>> Get() =>
            cardService.Get();

        [HttpGet("{id:length(24)}", Name = "GetBook")]
        public ActionResult<Card> Get(string id)
        {
            var card = cardService.Get(id);
            if (card == null)
            {
                return NotFound();
            }

            return card;
        }

        [HttpPost]
        public ActionResult<Card> Create(Card card)
        {
            cardService.Create(card);

            return CreatedAtRoute("GetCard", new {id = card.Id.ToString()}, card);
        }

        [HttpPut("{id:length(24)}")]
        public ActionResult Update(string id, Card cardIn) {
            var card = cardService.Get(id);
            if (card == null) {
                return NotFound();
            }

            cardService.Update(id, cardIn);

            return NoContent();
        }

        [HttpPut("{id:length(24)}")]
        public ActionResult Delete(string id) {
            var card = cardService.Get(id);
            if (card == null) {
                return NotFound();
            }

            cardService.Remove(card.Id);

            return NoContent();
        }
    }
}
