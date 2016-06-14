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

        void Init()
		{
            RegisterMessages();
            RegisterPermissions();
        }
		
        void Loaded()
        {
            data = Interface.Oxide.DataFileSystem.ReadObject<Data>("LoyaltyData");
            timer.Repeat(60f, 0, () =>
            {
                foreach (var player in BasePlayer.activePlayerList) 
                {
                    if (!data.players.ContainsKey(player.userID))
                        data.players.Add(player.userID, new Player(player.userID, player.displayName, 1));
                    else
                    {
                        data.players[player.userID].loyalty += 1;
                        foreach (var reward in data.rewards)
                            if (data.players[player.userID].loyalty == reward.requirement)
                            {
                                rust.RunServerCommand("grant user " + rust.QuoteSafe(player.displayName) + " " + rust.QuoteSafe(reward.permission));
                                SendMessage(player, "accessGranted", reward.requirement, (string)Config["serverName"], reward.alias);
                            }
                    }
                    Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
                }
            });
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file for Loyalty");
            Config.Clear();
            Config["serverName"] = "DefaultServer";
            Config["serverID"] = "76561197981174278";
            SaveConfig();
        }

        void Unload()
        {
            Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
        }

        void OnPlayerInit(BasePlayer player)
        {
            if (!data.players.ContainsKey(player.userID))
            {
                data.players.Add(player.userID, new Player(player.userID, player.displayName, 0));
                Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
            }
        }


        [ChatCommand("loyalty")]
        void loyalty(BasePlayer sender, string command, string[] args)
        {
            if (args.Length <= 0)
                if (permission.UserHasPermission(sender.UserIDString, "loyalty.loyalty") || sender.IsAdmin())
                {
                    if (data.players.ContainsKey(sender.userID))
                        SendMessage(sender, "loyaltyCurrent", data.players[sender.userID].loyalty, Config["serverName"]);
                    else
                        SendMessage(sender, "noLoyalty");
                }
                else
                    SendMessage(sender, "accessDenied");

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "add":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.add") && !sender.IsAdmin())
                        {
                            SendMessage(sender, "accessDenied");
                            return;
                        }
                        if (args.Length != 4)
                        {
                            SendMessage(sender, "syntaxAdd");
                            return;
                        }
                        SendMessage(sender, add(args[1], args[2], args[3]));
                        break;

                    case "remove":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.remove") && !sender.IsAdmin())
                        {
                            SendMessage(sender, "accessDenied");
                            return;
                        }
                        if (args.Length != 2)
                        {
                            SendMessage(sender, "syntaxRemove");
                            return;
                        }
                        SendMessage(sender, remove(args[1]));
                        break;

                    case "reset":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.reset") && !sender.IsAdmin())
                        {
                            SendMessage(sender, "accessDenied");
                            return;
                        }
                        if (args.Length != 2)
                        {
                            SendMessage(sender, "syntaxReset");
                            return;
                        }
                        SendMessage(sender, reset(args[1]));
                        break;

                    case "set":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.set") && !sender.IsAdmin())
                        {
                            SendMessage(sender, "accessDenied");
                            return;
                        }
                        if (args.Length != 3)
                        {
                            SendMessage(sender, "syntaxSet");
                            return;
                        }
                        SendMessage(sender, set(args[1], args[2]));
                        break;

                    case "rewards":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.rewards") && !sender.IsAdmin())
                        {
                            SendMessage(sender, "accessDenied");
                            return;
                        }
                        if (args.Length != 1)
                        {
                            SendMessage(sender, "syntaxRewards");
                            return;
                        }
              
                        SendMessage(sender, rewards(sender));
                        break;

                    case "help":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.help") && !sender.IsAdmin())
                        {
                            SendMessage(sender, "accessDenied");
                            return;
                        }
                        if (args.Length != 1)
                        {
                            SendMessage(sender, "syntaxHelp");
                            return;
                        }
                        SendMessage(sender, "help");
                        break;

                    case "lookup":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.lookup") && !sender.IsAdmin())
                        {
                            SendMessage(sender, "accessDenied");
                            return;
                        }
                        if (args.Length != 2)
                        {
                            SendMessage(sender, "syntaxLookup");
                            return;
                        }
                        SendMessage(sender, lookup(args[1]));
                        break;

                    case "top":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.top") && !sender.IsAdmin())
                        {
                            SendMessage(sender, "accessDenied");
                            return;
                        }
                        if (args.Length != 1)
                        {
                            SendMessage(sender, "syntaxTop");
                            return;
                        }
                        top(sender);
                        break;

                    default:
                        SendMessage(sender, "There's no secondary argument with that identifier.");
                        break;
                };
            }
        }

        string add(string alias, string permission, string timereq)
        {
            if (!Regex.IsMatch(timereq, "^\\d+$"))
                return "syntaxNotInt";

            if (rewardExists(rust.QuoteSafe(permission)))
                return FormatMessage("rewardExists", permission);

            data.rewards.Add(new LoyaltyReward(rust.QuoteSafe(alias), rust.QuoteSafe(permission), Convert.ToUInt32(timereq, 10)));
            Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);

            return FormatMessage("addSuccess", alias, permission, Convert.ToUInt32(timereq, 10));
        }

        string remove(string permission)
        {
            if (!rewardExists(rust.QuoteSafe(permission)))
                return FormatMessage("rewardNoExist", permission);
            foreach (LoyaltyReward reward in data.rewards)
                if (reward.permission == rust.QuoteSafe(permission))
                {
                   data.rewards.Remove(reward);
                   return FormatMessage("rewardRemoved", permission);
                }
            return "fatalError";
        }

        string reset(string playerName)
        {
            Player player = data.players.Values.FirstOrDefault(x => x.name.StartsWith(playerName, StringComparison.CurrentCultureIgnoreCase));
            if (player == null)
                return FormatMessage("playerNotFound", playerName);

            data.players[player.id].loyalty = 0;
            foreach (var reward in data.rewards)
            {
                rust.RunServerCommand("revoke user " + rust.QuoteSafe(player.name) + " " + reward.permission);
                SendMessage(BasePlayer.FindByID(player.id), FormatMessage("accessLost", reward));
            }
            return FormatMessage("resetSuccess", player.name);
        }

        string set(string playerName, string newLoyalty)
        {
            Player player = data.players.Values.FirstOrDefault(x => x.name.StartsWith(playerName, StringComparison.CurrentCultureIgnoreCase));
            if (player == null)
                return FormatMessage("playerNotFound", playerName);
            if (!Regex.IsMatch(newLoyalty, "^\\d+$"))
                return("Invalid syntax. The third argument needs to be a positive integer.");

            data.players[player.id].loyalty = Convert.ToUInt32(newLoyalty, 10);

            foreach (var reward in data.rewards)
            {
                if (data.players[player.id].loyalty >= reward.requirement)
                {
                    rust.RunServerCommand("grant user " + rust.QuoteSafe(player.name) + " " + reward.permission);
                    SendMessage(BasePlayer.FindByID(player.id), FormatMessage("accessGained", reward.requirement, Config["serverName"], reward.alias));
                }
                if(data.players[player.id].loyalty < reward.requirement)
                {
                    rust.RunServerCommand("revoke user " + rust.QuoteSafe(player.name) + " " + reward.permission);
                    SendMessage(BasePlayer.FindByID(player.id), FormatMessage("accessLost", reward.alias));
                }
            }
            return FormatMessage("setSuccess", player.name, Convert.ToUInt32(newLoyalty, 10));

        }

        string rewards(BasePlayer sender) //Todo make less useless
        {
            foreach (var reward in data.rewards)
                SendMessage(sender, "Alias: " + reward.alias + " Perm: " + reward.permission + " Req: " + reward.requirement);

            return "End of reward list"; 
        }

        string lookup(string player)
        {

            Player lookUpPlayer = data.players.Values.FirstOrDefault(x => x.name.StartsWith(player, StringComparison.CurrentCultureIgnoreCase));
            if (lookUpPlayer != null)
                return FormatMessage("lookupEntry", lookUpPlayer.name, data.players[lookUpPlayer.id].loyalty);
            else
                return FormatMessage("playerNotFound", player);
        }

        void top(BasePlayer sender)
        {
            var topList = (from entry in data.players orderby entry.Value.loyalty descending select entry).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value);
            int counter = 0;
            SendMessage(sender, "Top 10 most loyal players");

            foreach (var entry in topList)
               SendMessageFromID(sender, "topEntry", entry.Value.id, ++counter, entry.Value.name, entry.Value.loyalty);
        }
        
        void SendMessage(BasePlayer receiver, string messageID, params object[] args)
        {
            rust.SendChatMessage(receiver, "",
               String.Format(lang.GetMessage("messageStyling", this), (args.Length > 0 ? String.Format(lang.GetMessage(messageID, this), args) : lang.GetMessage(messageID, this))),
               Config["serverID"].ToString());
        }

        void SendMessageAsServer(BasePlayer receiver, string messageID, params object[] args)
        {
            rust.SendChatMessage(receiver,
               String.Format(lang.GetMessage("senderStyling", this), Config["serverName"]),
                String.Format(lang.GetMessage("messageStyling", this), (args.Length > 0 ? String.Format(lang.GetMessage(messageID, this), args) : lang.GetMessage(messageID, this))), 
                Config["serverID"].ToString());
        }

        void SendMessageFromID(BasePlayer receiver, string messageID, ulong senderID, params object[] args)
        {
            rust.SendChatMessage(receiver, 
                String.Format(lang.GetMessage("senderStyling", this), "",
                String.Format(lang.GetMessage("messageStyling", this), (args.Length > 0 ? String.Format(lang.GetMessage(messageID, this), args) : lang.GetMessage(messageID, this))),
                Convert.ToString(senderID)));
        }

        string FormatMessage(string messageID, params object[] args)
        {
            return String.Format(lang.GetMessage(messageID, this), args);
        }

        bool rewardExists(string permission)
        {
            foreach (LoyaltyReward reward in data.rewards)
                if (reward.permission == permission)
                    return true;

            return false;
        }

        void RegisterMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["syntaxAdd"] = "<color=red>Too few or too many arguments. \nUse /loyalty add {string: /alias} {string: permission.permission {int: loyaltyrequirement}</color>",
                ["syntaxRemove"] = "<color=red>Too few or too many arguments. \nUse /loyalty remove {string: permission.permission}</color>",
                ["syntaxSet"] = "<color=red>Too few or too many arguments. \nUse /loyalty set {string: username} {int: loyaltyPoints}</color>",
                ["syntaxReset"] = "<color=red>Too few or too many arguments. \nUse /loyalty reset {string: username}</color>",
                ["syntaxHelp"] = "<color=red>Too few or too many arguments. \nUse /loyalty help</color>",
                ["syntaxRewards"] = "<color=red>Too few or too many arguments. \nUse /loyalty rewards</color>",
                ["syntaxLookup"] = "<color=red>Too few or too many arguments. \nUse /loyalty lookup {string: playername}</color>",
                ["syntaxTop"] = "<color=red>Too few or too many arguments. \nUse /loyalty top</color>",
                ["syntaxNotInt"] = "Invalid syntax. Loyalty requirement needs to be a positive integer.",
                ["rewardExists"] = "A reward for the permission {0} already exists.",
                ["rewardNoExist"] = "No reward for the permission ",
                ["rewardRemoved"] = "Loyalty reward {0} was successfully removed.",
                ["rewardEntry"] = "Alias: {0} Perm: {1} Req: {2}",
                ["accessGained"] = "Congratulations by spending <color=yellow>{0 minutes</color> on <color=yellow>{1}</color> you have gained access to the command <color=grey>{2}</color>. Thank you for playing!",
                ["accessDenied"] = "<color=red>You do not have access to that command.</color>",
                ["accessLost"] = "<color=red>You have lost access to <color=yellow>{0}</color> due to an administrator changing your loyalty.</color>",
                ["loyaltyCurrent"] = "You have accumulated a total of<color=yellow> {0} </color>loyalty points by playing on <color=yellow>{1}</color>",
                ["noLoyalty"] = "<color=red>You have not yet earned any loyalty point. Check again later!</color>",
                ["noCommand"] = "<color=red>There's no command by that name.</color>",
                ["playerNotFound"] = "<color=red>No player by the name {0} was found.</color>",
                ["fatalError"] = "FATAL ERROR. If you see this something has gone terribly wrong.",
                ["messageStyling"] = "{0}",
                ["senderStyling"] = "<color=lime>{0}</color>",
                ["setSuccess"] = "Player {0}'s loyalty points were successfully set to {1}.",
                ["resetSuccess"] = "Player {0}'s loyalty points were successfully reset.",
                ["addSuccess"] = "Successfully added: {0} {1} {2}",
                ["topEntry"] = "{0}. <color=lime>{1}</color> - {2}",
                ["lookupEntry"] = "Player <color=lime>{0}</color> has accumulated a total of {1} loyalty points.",
                ["help"] = "<color=yellow>Loyalty by Bamabo</color>\nLoyalty is a plugin that lets server owners reward their players with permissions according to how much time they've spent on the server. 1 Loyalty = 1 minute. \n<color=grey>/loyalty add/remove/set/reset/rewards/top/lookup</color>\n More info and source on <color=grey>github.com/Hazzty/Loyalty</color>",
            }, this);
        }

        void RegisterPermissions()
        {
            permission.RegisterPermission("loyalty.loyalty", this);
            permission.RegisterPermission("loyalty.add", this);
            permission.RegisterPermission("loyalty.remove", this);
            permission.RegisterPermission("loyalty.reset", this);
            permission.RegisterPermission("loyalty.set", this);
            permission.RegisterPermission("loyalty.lookup", this);
            permission.RegisterPermission("loyalty.top", this);
            permission.RegisterPermission("loyalty.rewards", this);
            permission.RegisterPermission("loyalty.help", this);
        }

    }

}