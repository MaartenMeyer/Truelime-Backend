using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TruelimeBackend.Helpers;
using TruelimeBackend.Models;
using TruelimeBackend.Services;

namespace TruelimeBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BoardsController : ControllerBase {
        private readonly BoardService boardService;
        private readonly LaneService laneService;
        private readonly CardService cardService;
        private readonly UserService userService;
        private readonly IHubContext<BroadcastHub> hubContext;
        private readonly IHttpContextAccessor httpContextAccessor;

        public BoardsController(BoardService boardService, LaneService laneService, CardService cardService, UserService userService, IHubContext<BroadcastHub> hubContext, IHttpContextAccessor httpContextAccessor) {
            this.boardService = boardService;
            this.laneService = laneService;
            this.cardService = cardService;
            this.userService = userService;
            this.hubContext = hubContext;
            this.httpContextAccessor = httpContextAccessor;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult<List<Board>> Get()
        {
            var userId = httpContextAccessor.HttpContext.User.Identity.Name;

            return boardService.Get(userId);
        }

        [AllowAnonymous]
        [HttpGet("{id:length(24)}", Name = "GetBoard")]
        public ActionResult<Board> Get(string id) {
            var board = boardService.GetById(id);
            if (board == null)
            {
                return NoContent();
            }

            return board;
        }

        [HttpPost]
        public async Task<ActionResult<Board>> CreateBoard(Board board)
        {
            var userId = httpContextAccessor.HttpContext.User.Identity.Name;
            var user = userService.GetById(userId);

            Lane[] lanes = {
                new Lane{Title = "Wat ging goed?"},
                new Lane{Title = "Wat kon beter?"},
                new Lane{Title = "Te ondernemen acties"}
            };
            board.Owner = new BoardUser(user.Id, user.Username);
            boardService.Create(board);
            foreach (var lane in lanes)
            {
                var createdLane = await laneService.Create(lane);
                await boardService.AddLane(board.Id, createdLane);
            }

            return CreatedAtRoute("GetBoard", new { id = board.Id }, board);
        }

        /// <summary>
        /// Finds and updates a board
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="boardIn"></param>
        /// <returns>Returns status 200 if successful or 204 if and id was not found</returns>
        [HttpPut("{boardId:length(24)}", Name = "UpdateBoard")]
        public async Task<ActionResult<Board>> UpdateBoard(string boardId, Board boardIn) {
            var board = boardService.GetById(boardId);
            if (board == null) {
                return NoContent();
            }

            // Update board
            await boardService.Update(boardId, boardIn);

            hubContext.Clients.All.SendAsync("BroadcastMessage");

            return Ok(new { message = "Board updated" });
        }

        /// <summary>
        /// Find and delete a board and its lanes and cards
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns>Returns status 200 if successful or 204 if and id was not found</returns>
        [HttpDelete("{boardId:length(24)}", Name = "DeleteBoard")]
        public async Task<ActionResult> DeleteBoard(string boardId) {
            var board = boardService.GetById(boardId);
            if (board == null) {
                return NoContent();
            }
            foreach(var lane in board.Lanes) {
                foreach (var card in lane.Cards) {
                    cardService.Remove(card.Id);
                }
                laneService.Remove(lane.Id);
            }

            boardService.Remove(board.Id);

            hubContext.Clients.All.SendAsync("BroadcastMessage");

            return Ok(new { message = "Board deleted" });
        }

        /// <summary>
        /// Clears board of all existing cards
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns>Returns status 200 if succesful or 204 if id was not found</returns>
        [HttpDelete("{boardId:length(24)}/cards", Name = "ClearBoard")]
        public async Task<ActionResult<Board>> ClearBoard(string boardId) {
            var board = boardService.GetById(boardId);
            if (board == null) {
                return NoContent();
            }
            foreach(var lane in board.Lanes) {
                foreach(var card in lane.Cards) {
                    var updatedLane = await laneService.RemoveCard(lane.Id, card);
                    await boardService.UpdateLane(board.Id, updatedLane);
                    cardService.Remove(card.Id);
                }
                
            }

            hubContext.Clients.All.SendAsync("BroadcastMessage");

            return Ok("Board cleared");
        }

        /// <summary>
        /// Creates a new lane and adds it to the board with id of param boardId
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="laneIn"></param>
        /// <returns>Returns status 200 if successful or 204 if and id was not found</returns>
        [HttpPost("{boardId:length(24)}/lanes", Name = "PostLane")]
        public async Task<ActionResult<Board>> CreateLane(string boardId, Lane laneIn) {
            var board = boardService.GetById(boardId);
            if (board == null)
            {
                return NoContent();
            }

            // Creates a new lane in the database
            var lane = await laneService.Create(laneIn);
            // Adds the new lane to the board
            await boardService.AddLane(board.Id, lane);

            hubContext.Clients.All.SendAsync("BroadcastMessage");

            return Ok(new { message = "Lane added" });
        }

        /// <summary>
        /// Finds and updates a lane
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="laneId"></param>
        /// <param name="laneIn"></param>
        /// <returns>Returns status 200 if successful or 204 if and id was not found</returns>
        [HttpPut("{boardId:length(24)}/lanes/{laneId:length(24)}", Name = "UpdateLane")]
        public async Task<ActionResult<Board>> UpdateLane(string boardId, string laneId, Lane laneIn) {
            var board = boardService.GetById(boardId);
            if (board == null) {
                return NoContent();
            }
            var lane = laneService.Get(laneId);
            if (lane == null) {
                return NoContent();
            }
            // Check if lane is present in this board
            if (board.Lanes.FindIndex(l => l.Id == lane.Id) < 0) {
                return NoContent();
            }

            // Update lane in database and return lane
            var updatedLane = await laneService.Update(lane.Id, laneIn);
            // Update lane in board
            await boardService.UpdateLane(board.Id, updatedLane);

            hubContext.Clients.All.SendAsync("BroadcastMessage");

            return Ok(new { message = "Lane updated" });
        }

        /// <summary>
        /// Find and deletes a lane and its cards
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="laneId"></param>
        /// <returns>Returns 200 if deleted or 204 if an id was not found</returns>
        [HttpDelete("{boardId:length(24)}/lanes/{laneId:length(24)}", Name = "DeleteLane")]
        public async Task<ActionResult> DeleteLane(string boardId, string laneId) {
            var board = boardService.GetById(boardId);
            if (board == null) {
                return NoContent();
            }
            var lane = laneService.Get(laneId);
            if (lane == null) {
                return NoContent();
            }
            if (board.Lanes.FindIndex(l => l.Id == lane.Id) < 0)
            {
                return NoContent();
            }
            foreach(var card in lane.Cards)
            {
                cardService.Remove(card.Id);
            }

            // Removes the lane from the board
            await boardService.RemoveLane(board.Id, lane);
            // Removes the lane from the database
            laneService.Remove(lane.Id);

            hubContext.Clients.All.SendAsync("BroadcastMessage");

            return Ok(new { message = "Lane deleted" });
        }

        /// <summary>
        /// Creates a new card and adds it to the lane with id of param laneId
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="laneId"></param>
        /// <param name="cardIn"></param>
        /// <returns>Returns status 200 if successful or 204 if an id was not found</returns>
        [HttpPost("{boardId:length(24)}/lanes/{laneId:length(24)}/cards", Name = "PostCard")]
        public async Task<ActionResult<Board>> CreateCard(string boardId, string laneId, Card cardIn) {
            var board = boardService.GetById(boardId);
            if (board == null) {
                return NoContent();
            }
            var lane = laneService.Get(laneId);
            if (lane == null) {
                return NoContent();
            }
            if (board.Lanes.FindIndex(l => l.Id == lane.Id) < 0) {
                return NoContent();
            }

            // Create card in database
            var card = await cardService.Create(cardIn);
            // Add card to lane
            var updatedLane = await laneService.AddCard(lane.Id, card);
            // Update lane in board
            await boardService.UpdateLane(board.Id, updatedLane);

            hubContext.Clients.All.SendAsync("BroadcastMessage");

            return Ok(new { message = "Card added" });
        }

        /// <summary>
        /// Finds and updates a card
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="laneId"></param>
        /// <param name="cardId"></param>
        /// <param name="cardIn"></param>
        /// <returns>Returns status 200 if successful or 204 if an id was not found</returns>
        [HttpPut("{boardId:length(24)}/lanes/{laneId:length(24)}/cards/{cardId:length(24)}", Name = "UpdateCard")]
        public async Task<ActionResult<Board>> UpdateCard(string boardId, string laneId, string cardId, Card cardIn) {
            var board = boardService.GetById(boardId);
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
            // Check if lane is present in this board and if the card is present in this lane
            if (board.Lanes.FindIndex(l => l.Id == lane.Id) < 0 || lane.Cards.FindIndex(c => c.Id == cardId) < 0) {
                return NoContent();
            }

            // Update card in database and return card
            var updatedCard = await cardService.Update(card.Id, cardIn);
            // Update card in lane
            var updatedLane = await laneService.UpdateCard(lane.Id, updatedCard);
            // Update lane in board
            await boardService.UpdateLane(board.Id, updatedLane);

            hubContext.Clients.All.SendAsync("BroadcastMessage");

            return Ok(new { message = "Card updated" });
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
            var board = boardService.GetById(boardId);
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
            cardService.Remove(card.Id);

            hubContext.Clients.All.SendAsync("BroadcastMessage");

            return Ok(new { message = "Delete card" });
        }
    }
}