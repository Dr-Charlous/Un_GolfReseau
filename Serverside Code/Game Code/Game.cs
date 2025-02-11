using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using PlayerIO.GameLibrary;

namespace UnityChestServer
{
    public class Player : BasePlayer
    {
        public int ActualPieces;
        public int Score;
    }

    [RoomType("UnityRoom")]
    public class GameCode : Game<Player>
    {
        // This method is called when an instance of your the game is created
        public override void GameStarted()
        {
            // anything you write to the Console will show up in the 
            // output window of the development server
            Console.WriteLine("Game is started: " + RoomId);
        }

        // This method is called when the last player leaves the room, and it's closed down.
        public override void GameClosed()
        {
            Console.WriteLine("RoomId: " + RoomId);
        }

        //public override bool AllowUserJoin(Player player)
        //{
        //    int maxplayers; //Default
        //                    //Parse roomdata
        //    if (!int.TryParse(RoomData["maxplayers"], out maxplayers))
        //    {
        //        maxplayers = 4; //Default
        //    }
        //    //Check if there's room for this player.
        //    if (Players.Count() < maxplayers - 1)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        // This method is called whenever a player joins the game
        public override void UserJoined(Player player)
        {
            foreach (Player pl in Players)
            {
                if (pl.ConnectUserId == player.ConnectUserId)
                {
                    player.Send("PlayerJoined", player.ConnectUserId, true);
                    player.Send("ChatSystem", pl.ConnectUserId, pl.ConnectUserId + " join");
                }
                else if (pl.ConnectUserId != player.ConnectUserId)
                {
                    pl.Send("PlayerJoined", player.ConnectUserId, false);
                    player.Send("PlayerJoined", pl.ConnectUserId, false);

                    pl.Send("ChatSystem", player.ConnectUserId, player.ConnectUserId + " join");
                    player.Send("ChatSystem", pl.ConnectUserId, pl.ConnectUserId + " join");
                }
            }
        }

        // This method is called when a player leaves the game
        public override void UserLeft(Player player)
        {
            foreach (Player pl in Players)
            {
                if (pl.ConnectUserId != player.ConnectUserId)
                {
                    pl.Send("ChatSystem", player.ConnectUserId, player.ConnectUserId + " left");
                    player.Send("ChatSystem", pl.ConnectUserId, pl.ConnectUserId + " left");
                }
            }

            Broadcast("PlayerLeft", player.ConnectUserId);
        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message message)
        {
            switch (message.Type)
            {
                // called when a player clicks on the ground
                case "Move":
                    Broadcast("Move", message.GetString(0), message.GetString(1), message.GetString(2));
                    break;
                case "Chat":
                    foreach (Player pl in Players)
                    {
                        if (pl.ConnectUserId != player.ConnectUserId)
                        {
                            pl.Send("Chat", player.ConnectUserId, message.GetString(0));
                        }
                    }
                    break;
                case "ChatSystem":
                    foreach (Player pl in Players)
                    {
                        if (pl.ConnectUserId != player.ConnectUserId)
                        {
                            pl.Send("Chat", player.ConnectUserId, message.GetString(0));
                        }
                    }
                    break;
                case "NextLevel":
                    foreach (Player pl in Players)
                    {
                        pl.Send("NextLevel", player.ConnectUserId);
                    }
                    break;
                case "IsArrived":
                    foreach (Player pl in Players)
                    {
                        if (pl.ConnectUserId != player.ConnectUserId)
                        {
                            pl.Send("IsArrived", player.ConnectUserId, message.GetBoolean(1));
                        }
                    }
                    break;
                case "ChangeTurn":
                    foreach (Player pl in Players)
                    {
                        pl.Send("ChangeTurn", player.ConnectUserId);
                    }
                    break;
            }
        }
    }
}