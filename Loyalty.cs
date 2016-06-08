using System.Collections.Generic;
using Oxide.Core;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Oxide.Plugins
{
    [Info("Loyalty", "Bamabo", 0.1)]
    [Description("Provides players with additional permissions for spending time on their server.")]

    class Loyalty : RustPlugin
    {

        class Data
        {
            public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();
            public HashSet<LoyaltyReward> rewards = new HashSet<LoyaltyReward>();

            public Data() { }
        }

        public class LoyaltyReward
        {
            public string alias { get; set; }
            public string permission { get; set; }
            public uint requirement { get; set; }

            public LoyaltyReward() { alias = null; permission = null; requirement = 0; }

            public LoyaltyReward(string alias, string permission, uint requirement = 0)
            {
                this.alias = alias;
                this.permission = permission;
                this.requirement = requirement;
            }

        }

        public class Player
        {
            public string name { get; set; }
            public ulong id { get; set; }
            public uint loyalty { get; set; }

            public Player() { }

            public Player(ulong id, string name, uint loyalty = 0)
            {
                this.id = id;
                this.name = name;
                this.loyalty = loyalty;
            }
            public override bool Equals(object obj)
            {
                Player pItem = obj as Player;
                return pItem.GetHashCode() == this.GetHashCode();
            }

            public override int GetHashCode()
            {
                return (int)this.id;
            }

        }

        Data data;

        void Loaded()
        {
            data = Interface.Oxide.DataFileSystem.ReadObject<Data>("LoyaltyData");

            permission.RegisterPermission("loyalty.loyalty", this);
            permission.RegisterPermission("loyalty.add", this);
            permission.RegisterPermission("loyalty.remove", this);
            permission.RegisterPermission("loyalty.reset", this);
            permission.RegisterPermission("loyalty.set", this);
            permission.RegisterPermission("loyalty.lookup", this);
            permission.RegisterPermission("loyalty.top", this);
            permission.RegisterPermission("loyalty.rewards", this);
            permission.RegisterPermission("loyalty.help", this);

            timer.Repeat(60f, 0, () =>
            {
                foreach (var player in BasePlayer.activePlayerList)
                {

                    if (!data.players.ContainsKey(player.userID))
                        data.players.Add(player.userID, new Player(player.userID, player.displayName, 1));
                    else
                    {
                        Player p;
                        data.players.TryGetValue(player.userID, out p);
                        data.players[player.userID].loyalty = p.loyalty + 1;

                        foreach (var reward in data.rewards)
                            if (p.loyalty + 1 == reward.requirement)
                            {
                                rust.RunServerCommand("grant user " + rust.QuoteSafe(player.displayName) + " " + rust.QuoteSafe(reward.permission));
                                SendReply(player, "You've gained access to " + reward.alias);
                            }
                    }
                }
                Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
            });
        }
        void Unload()
        {
            Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
        }

        [ChatCommand("loyalty")]
        void loyalty(BasePlayer sender, string command, string[] args)
        {

            if (args.Length == 0)
                if (!permission.UserHasPermission(sender.UserIDString, "loyalty.loyalty"))
                {
                    if (data.players.ContainsKey(sender.userID))
                    {
                        SendReply(sender, "Your current loyalty points: " + data.players[sender.userID].loyalty);
                    }
                    else
                        SendReply(sender, "You have not yet earned any loyalty point. Check again later!");
                }
                else
                    SendReply(sender, "You do not have access to that command."); 


            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "add":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.add"))
                        {
                            SendReply(sender, "You do not have access to that command.");
                            return;
                        }

                        if (args.Length != 4)
                        {
                            SendReply(sender, "Too few or too many arguments. \nUse /loyalty add {string: /alias} {string: permission.permission {int: loyaltyrequirement}");
                            return;
                        }
                        SendReply(sender, add(args[1], args[2], args[3]));
                        break;

                    case "remove":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.remove"))
                        {
                            SendReply(sender, "You do not have access to that command.");
                            return;
                        }
                        if (args.Length != 2)
                        {
                            SendReply(sender, "Too few or too many arguments. \nUse /loyalty remove {string: permission.permission}");
                            return;
                        }
                        SendReply(sender, remove(args[1]));
                        break;

                    case "reset":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.reset"))
                        {
                            SendReply(sender, "You do not have access to that command.");
                            return;
                        }
                        if (args.Length != 2)
                        {
                            SendReply(sender, "Too few or too many arguments. \nUse /loyalty reset {string: username}");
                            return;
                        }
                        SendReply(sender, reset(args[1]));
                        break;

                    case "set":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.set"))
                        {
                            SendReply(sender, "You do not have access to that command.");
                            return;
                        }
                        if (args.Length != 3)
                        {
                            SendReply(sender, "Too few or too many arguments. \nUse /loyalty set {string: username} {int: loyaltyPoints}");
                            return;
                        }
                        SendReply(sender, set(args[1], args[2]));
                        break;

                    case "rewards":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.rewards"))
                        {
                            SendReply(sender, "You do not have access to that command.");
                            return;
                        }
                        if (args.Length != 1)
                        {
                            SendReply(sender, "Too few or too many arguments. \nUse /loyalty rewards");
                            return;
                        }
              
                        SendReply(sender, rewards(sender));
                        break;

                    case "help":
                        //todo
                        break;
                    case "lookup":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.lookup"))
                        {
                            SendReply(sender, "You do not have access to that command.");
                            return;
                        }
                        if (args.Length != 2)
                        {
                            SendReply(sender, "Too few or too many arguments. \nUse /loyalty lookup {string: playername}");
                            return;
                        }
                        SendReply(sender, lookup(args[1]));
                        break;

                    case "top":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.top"))
                        {
                            SendReply(sender, "You do not have access to that command.");
                            return;
                        }
                        top(sender);
                        break;
                    default:
                        SendReply(sender, "There's no secondary argument with that identifier.");
                        break;
                };
            }
        }

        string add(string alias, string permission, string timereq)
        {
            

            if (!Regex.IsMatch(timereq, "^\\d+$"))
                return  "Invalid syntax. The fourth argument needs to be a positive integer.";

            if (rewardExists(rust.QuoteSafe(permission)))
                return "A loyalty reward for that permission already exists.";

            data.rewards.Add(new LoyaltyReward(rust.QuoteSafe(alias), rust.QuoteSafe(permission), Convert.ToUInt32(timereq, 10)));
            Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
            return ("Successfully added: " + alias + " " + permission + " " + Convert.ToUInt32(timereq, 10));
        }

        string remove(string permission)
        {
            if (!rewardExists(rust.QuoteSafe(permission)))
                return ("Loyalty reward with permission " + rust.QuoteSafe(permission) + " does not exist.");

            foreach (LoyaltyReward reward in data.rewards)
                if (reward.permission == rust.QuoteSafe(permission))
                {
                    data.rewards.Remove(reward);
                    return ("Loyalty reward " + rust.QuoteSafe(permission) + " was successfully removed.");
                }
            return "Error if this happens something has gone terribly wrong. In remove function.";
        }

        string reset(string playerName)
        {
            BasePlayer player = BasePlayer.Find(playerName);
            if (player == null)
                return ("No player by the name " + rust.QuoteSafe(playerName) + "was found.");

            data.players[player.userID].loyalty = 0;
            foreach (var reward in data.rewards)
            {
                    rust.RunServerCommand("revoke user " + rust.QuoteSafe(player.displayName) + " " + reward.permission);
                    SendReply(player, "You've lost access to " + reward.alias);
            }
            return ("Player " + player.displayName + "'s loyalty point successfully reset.");
        }

        string set(string playerName, string newLoyalty)
        {
            BasePlayer player = BasePlayer.Find(playerName);
            if (player == null)
                return("No player by the name " + rust.QuoteSafe(playerName) + "was found.");
            if (!Regex.IsMatch(newLoyalty, "^\\d+$"))
                return("Invalid syntax. The fourth argument needs to be a positive integer.");

            data.players[player.userID].loyalty = Convert.ToUInt32(newLoyalty, 10);

            foreach (var reward in data.rewards)
            {
                if (data.players[player.userID].loyalty >= reward.requirement)
                {
                    rust.RunServerCommand("grant user " + rust.QuoteSafe(player.displayName) + " " + reward.permission);
                    SendReply(player, "You've gained access to " + reward.alias);
                }
                if(data.players[player.userID].loyalty < reward.requirement)
                {
                    rust.RunServerCommand("revoke user " + rust.QuoteSafe(player.displayName) + " " + reward.permission);
                    SendReply(player, "You've lost access to " + reward.alias);
                }
            }
            return ("Player " + player.displayName + "'s loyalty point was successfully set to " + Convert.ToUInt32(newLoyalty, 10));

        }

        string rewards(BasePlayer sender)
        {
           
            SendReply(sender, "List of all rewards: ");

            foreach (var reward in data.rewards)
                SendReply(sender, "Alias: " + reward.alias + " Perm: " + reward.permission + " Req: " + reward.requirement);

            return "End of list.";
        }

        string lookup(string player)
        {

            Player lookUpPlayer = data.players.Values.FirstOrDefault(x => x.name.StartsWith(player, StringComparison.CurrentCultureIgnoreCase));

            if (lookUpPlayer != null)
               return (rust.QuoteSafe(lookUpPlayer.name) + " has " + data.players[lookUpPlayer.id].loyalty + " loyality points.");
            else
                return("No player by the name " + rust.QuoteSafe(player) + " was found.");
        }

        void top(BasePlayer sender)
        {
            var topList = (from entry in data.players orderby entry.Value.loyalty descending select entry).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value);
            int counter = 0;

            SendReply(sender, "Top 10 most loyal players");

            foreach (var entry in topList)
            {
                SendReply(sender, ++counter + ". " + entry.Value.name + " - " + entry.Value.loyalty);
                if (counter == 10)
                    break;
            }
            SendReply(sender, "End of list.");
            
        }

        bool rewardExists(string permission)
        {
            foreach (LoyaltyReward reward in data.rewards)
            {
                if (reward.permission == permission)
                    return true;
            }
            return false;
        }
    }

}