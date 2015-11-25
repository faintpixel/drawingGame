using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections.Concurrent;
using SignalRTest.Models;
using System.Threading.Tasks;

namespace SignalRTest
{
    public class DrawingHub : Hub
    {
        public static ConcurrentDictionary<string, ExquisiteCorpse> RoomId_Game = new ConcurrentDictionary<string, ExquisiteCorpse>(); // TO DO - why am i using ConcurrentDictionary instead of just dictionary?
        public static ConcurrentDictionary<string, string> User_RoomId = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, string> User_Name = new ConcurrentDictionary<string, string>();
        private readonly string LOBBY_GROUP = "lobby";

        /*
        [ ] make the eraser suck less... can we keep the path going instead of making a new path for each update?
        [ ] Make the page layout nicer
        [ ] Refactor    
        [ ] save to gallery
        [ ] flash tab when someone joins
        [ ] notification if someone leaves
        [ ] Pad the canvas area so the drawing experience is a bit nicer - if the mouse leaves the canvas while drawing weird things happen
        [ ] Add pressure sensitivity
            [X] IE
            [/] Chrome - waiting on them to impelement pointer events like ie... the wacom web plugin doesn't work anymore.
            [ ] Firefox - look into mozPressure
        */

        public DrawingHub()
        {
        }

        public void Send(string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }

        public async Task JoinLobby()
        {
            await Groups.Add(Context.ConnectionId, LOBBY_GROUP);

            List<string> openGames = GetOpenGames();
            Clients.Client(Context.ConnectionId).updateLobby(openGames);
        }

        public void ShowMyArea(int position)
        {
            // TO DO - don't trust the passed in position... get it from the game
            Clients.Group(User_RoomId[Context.ConnectionId]).revealDrawing(position);
        }

        public void HideMyArea(int position)
        {
            // TO DO - don't trust the passed in position... get it from the game
            Clients.Group(User_RoomId[Context.ConnectionId], Context.ConnectionId).hideDrawing(position);
        }

        public void ClearMyArea(int position)
        {
            // TO DO - don't trust the passed in position... get it from the game
            Clients.Group(User_RoomId[Context.ConnectionId]).clearDrawing(position, true);
        }

        public void ClearAll()
        {
            string roomId = User_RoomId[Context.ConnectionId];
            ExquisiteCorpse game = RoomId_Game[roomId];
            if (game.HasStarted == false)
            {
                Clients.Group(roomId).clearDrawing(0, false);
                Clients.Group(roomId).clearDrawing(1, false);
                Clients.Group(roomId).clearDrawing(2, false);
            }
        }

        private List<string> GetOpenGames()
        {
            List<string> openGames = new List<string>();
            foreach (var game in RoomId_Game.Values)
            {
                if (game.HasStarted == false)
                    openGames.Add(game.RoomId);
            }

            return openGames;
        }

        private async Task BroadcastOpenGames()
        {
            List<string> openGames = GetOpenGames();
            await Clients.Group(LOBBY_GROUP).updateLobby(openGames);
        }

        public async Task Draw(int prevX, int prevY, int currX, int currY, string strokeStyle, decimal lineWidth)
        {
            await Clients.Group(User_RoomId[Context.ConnectionId]).updateCanvas(prevX, prevY, currX, currY, strokeStyle, lineWidth);            
        }

        public async Task AddPoint(int x, int y, string strokeStyle, decimal lineWidth)
        {
            await Clients.Group(User_RoomId[Context.ConnectionId]).addPoint(x, y, strokeStyle, lineWidth);
        }

        public async Task Join(string roomId, string name)
        {
            if (RoomId_Game.ContainsKey(roomId) == false)
                RoomId_Game[roomId] = new ExquisiteCorpse { RoomId = roomId };

            ExquisiteCorpse game = RoomId_Game[roomId];
            if (game.Participants.Count > 3)
            {
                Clients.Client(Context.ConnectionId).displayMessage("Unable to join game. Already in progress.");
            }
            else
            {
                await Groups.Remove(Context.ConnectionId, LOBBY_GROUP); // they don't need to be included in lobby notifications anymore
                User_RoomId[Context.ConnectionId] = roomId;
                User_Name[Context.ConnectionId] = name;
                await Clients.Client(Context.ConnectionId).joinedGame(roomId);
                await Clients.Client(Context.ConnectionId).setPosition(game.Participants.Count); // position will be starting at 0

                await Groups.Add(Context.ConnectionId, roomId); // add them to the game
                game.Participants.Add(name);

                await Clients.Group(roomId).updateParticipants(game.Participants); // tell everyone who is in the game

                await BroadcastOpenGames(); // let everyone know the game is up and running

                if (game.Participants.Count >= 3)
                {
                    game.HasStarted = true;
                    await Clients.Group(roomId).startGame();
                }
            }
        }

        public void Quit()
        {
            string roomId = null;

            if (User_RoomId.ContainsKey(Context.ConnectionId))
            {
                User_RoomId.TryRemove(Context.ConnectionId, out roomId);
            }

            if (roomId != null)
            {
                Groups.Remove(Context.ConnectionId, roomId);
                if (RoomId_Game.ContainsKey(roomId))
                {
                    ExquisiteCorpse game = RoomId_Game[roomId];
                    string name = User_Name[Context.ConnectionId];
                    game.Participants.Remove(name);
                    Clients.Group(roomId).updateParticipants(game.Participants);
                    User_Name.TryRemove(Context.ConnectionId, out name);

                    if (game.Participants.Count <= 0)
                    {
                        RoomId_Game.TryRemove(game.RoomId, out game);
                        BroadcastOpenGames();
                    }
                }
            }
        }

        public void EndGame(string roomId)
        {
            ExquisiteCorpse removedItem;
            RoomId_Game.TryRemove(roomId, out removedItem);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Quit();
            return base.OnDisconnected(stopCalled);
        }
    }
}