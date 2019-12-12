using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TruelimeBackend.Models;
using TruelimeBackend.Services;

namespace TruelimeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardsController : ControllerBase
    {
        private readonly BoardService boardService;
        private readonly LaneService laneService;
        private readonly CardService cardService;

        public BoardsController(BoardService boardService, LaneService laneService, CardService cardService)
        {
            this.boardService = boardService;
            this.laneService = laneService;
            this.cardService = cardService;
        }

        [HttpGet]
        public ActionResult<List<Board>> Get() =>
            boardService.Get();

        [HttpGet("{id:length(24)}", Name = "GetBoard")]
        public ActionResult<Board> Get(string id) {
            var board = boardService.Get(id);
            if (board == null)
            {
                return NoContent();
            }

            return board;
        }

        [HttpPost]
        public ActionResult<Board> CreateBoard(Board board)
        {
            boardService.Create(board);

            return CreatedAtRoute("GetBoard", new { id = board.Id.ToString() }, board);
        }

        [HttpDelete("{boardId:length(24)}", Name = "DeleteBoard")]
        public ActionResult DeleteBoard(string boardId) {
            var board = boardService.Get(boardId);
            if (board == null) {
                return NoContent();
            }
            foreach(Lane lane in board.Lanes) {
                foreach (Card card in lane.Cards) {
                    cardService.Remove(card);
                }
                laneService.Remove(lane);
            }

            boardService.Remove(board.Id);

            return Ok();
        }

        /// <summary>
        /// Creates a new lane and adds it to the board with id of param boardId
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="laneIn"></param>
        /// <returns>Returns the board this lane was added to</returns>
        [HttpPost("{boardId:length(24)}/lanes", Name = "PostLane")]
        public async Task<ActionResult<Board>> CreateLane(string boardId, Lane laneIn)
        {
            var board = boardService.Get(boardId);
            if (board == null)
            {
                return NoContent();
            }

            var lane = await laneService.Create(laneIn);
            await boardService.AddLane(board.Id, lane);

            return Ok("Lane added");
        }

        /// <summary>
        /// Find and deletes the lane with id of param laneId
        /// Removes the lane from the board with id of param boardId
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="laneId"></param>
        /// <returns>Returns 200 if deleted or 204 if an id was not found</returns>
        [HttpDelete("{boardId:length(24)}/lanes/{laneId:length(24)}", Name = "DeleteLane")]
        public async Task<ActionResult> DeleteLane(string boardId, string laneId) {
            var board = boardService.Get(boardId);
            if (board == null) {
                return NoContent();
            }
            var lane = laneService.Get(laneId);
            if (lane == null) {
                return NoContent();
            }
            var index = board.Lanes.FindIndex(l => l.Id == lane.Id);
            if (index < 0)
            {
                return NoContent();
            }
            foreach(Card card in lane.Cards)
            {
                cardService.Remove(card);
            }

            await boardService.RemoveLane(board.Id, lane);

            laneService.Remove(lane.Id);

            return Ok();
        }

        /// <summary>
        /// Creates a new card and adds it to the lane with id of param laneId
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="laneId"></param>
        /// <param name="cardIn"></param>
        /// <returns>Returns the board this card was added to</returns>
        [HttpPost("{boardId:length(24)}/lanes/{laneId:length(24)}/cards", Name = "PostCard")]
        public async Task<ActionResult<Board>> CreateCard(string boardId, string laneId, Card cardIn) {
            var board = boardService.Get(boardId);
            if (board == null) {
                return NoContent();
            }
            var lane = laneService.Get(laneId);
            if (lane == null) {
                return NoContent();
            }
            var index = board.Lanes.FindIndex(l => l.Id == lane.Id);
            if (index < 0) {
                return NoContent();
            }

            // Create card in database
            var card = await cardService.Create(cardIn);
            // Add card to lane
            var updatedLane = await laneService.AddCard(lane.Id, card);
            // Update lane in board
            await boardService.UpdateLane(board.Id, updatedLane);

            return Ok("Card added");
        }

        /// <summary>
        /// Find and deletes the card with id of param cardId
        /// Removes the card from the lane with id of param laneId
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="laneId"></param>
        /// /// <param name="cardId"></param>
        /// <returns>Returns 200 if deleted or 204 if an id was not found</returns>
        [HttpDelete("{boardId:length(24)}/lanes/{laneId:length(24)}/cards/{cardId:length(24)}", Name = "DeleteCard")]
        public async Task<ActionResult> DeleteCard(string boardId, string laneId, string cardId) {
            var board = boardService.Get(boardId);
            if (board == null) {
                return NoContent();
            }
            var lane = laneService.Get(laneId);
            if (lane == null) {
                return NoContent();
            }
            var card = cardService.Get(cardId);
            if (card == null) {
                return NoContent();
            }
            var laneIndex = board.Lanes.FindIndex(l => l.Id == lane.Id);
            if (laneIndex < 0) {
                return NoContent();
            }
            var cardIndex = lane.Cards.FindIndex(c => c.Id == card.Id);
            if (cardIndex < 0) {
                return NoContent();
            }

            // Remove card from lane, returns the updated lane
            var updatedLane = await laneService.RemoveCard(lane.Id, card);
            // Update this lane on the board
            await boardService.UpdateLane(board.Id, updatedLane);
            // Remove the card from the database
            cardService.Remove(card);

            return Ok();
        }
    }
}