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
        public ActionResult<Board> Create(Board board)
        {
            boardService.Create(board);

            return CreatedAtRoute("GetBoard", new { id = board.Id.ToString() }, board);
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
            board.Lanes.Add(lane);
            boardService.Update(board.Id, board);

            return CreatedAtRoute("GetBoard", new { id = board.Id }, board);
        }

        /// <summary>
        /// Find and deletes the lane with id of param laneId
        /// Removes the lane from the board with id of param boardId
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="laneId"></param>
        /// <returns>Returns 200 if deleted or 204 if an id was not found</returns>
        [HttpDelete("{boardId:length(24)}/lanes/{laneId:length(24)}", Name = "DeleteLane")]
        public ActionResult DeleteLane(string boardId, string laneId) {
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

            board.Lanes.RemoveAt(index);
            boardService.Update(board.Id, board);

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

            var card = await cardService.Create(cardIn);
            lane.Cards.Add(card);
            laneService.Update(lane.Id, lane);
            board.Lanes[index] = lane;
            boardService.Update(board.Id, board);

            return CreatedAtRoute("GetBoard", new { id = board.Id }, board);
        }
    }
}