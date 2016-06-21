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

        #region Classes
        class Data
        {
            public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();
            public HashSet<UserGroup> usergroups = new HashSet<UserGroup>();
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
        public class UserGroup
        {
            public string usergroup { get; set; }
            public uint requirement { get; set; }

            public UserGroup() { usergroup = ""; requirement = 0;}
            public UserGroup(string usergroup, uint requirement = 0)
            {
                this.usergroup = usergroup;
                this.requirement = requirement;
            }



        }
        #endregion Classes

        private Data data;

        #region Hooks
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
                    if ((player.IsAdmin() && (bool)Config["allowAdmin"]) || !player.IsAdmin())
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
                                    SendMessage(player, "accessGranted", reward.requirement, Config["serverName"].ToString(), reward.alias);
                                }
                            foreach (var usergroup in data.usergroups)
                                if (data.players[player.userID].loyalty == usergroup.requirement)
                                {
                                    rust.RunServerCommand("usergroup add" + rust.QuoteSafe(player.displayName) + " " + rust.QuoteSafe(usergroup.usergroup));
                                    SendMessage(player, "groupAssigned", usergroup.requirement, Config["serverName"].ToString(), usergroup.usergroup);
                                }
                        }
                    }
                    Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
                }
            }); 
        }

        void Unload()
        {
            Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file for Loyalty");
            Config.Clear();
            Config["serverName"] = "DefaultServer";
            Config["serverID"] = "76561197981174278";
            Config["allowAdmin"] = false;
            SaveConfig();
        }

        void OnPlayerInit(BasePlayer player)
        {
            if ((player.IsAdmin() && (bool)Config["allowAdmin"]) || !player.IsAdmin())
            {
                if (!data.players.ContainsKey(player.userID))
                {
                    data.players.Add(player.userID, new Player(player.userID, player.displayName, 0));
                    Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
                }
            }
        }
        #endregion Hooks

        #region Main
        [ChatCommand("loyalty")]
        void loyalty(BasePlayer sender, string command, string[] args)
        {
            if (args.Length <= 0)
                if (permission.UserHasPermission(sender.UserIDString, "loyalty.loyalty") || sender.IsAdmin())
                {
                    if (data.players.ContainsKey(sender.userID))
                        SendMessage(sender, "loyaltyCurrent", data.players[sender.userID].loyalty, Config["serverName"]);
                    else
                        SendMessage(sender, "errorNoLoyalty");
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
                    case "removeg":
                        if (!permission.UserHasPermission(sender.UserIDString, "loyalty.removegroup") && !sender.IsAdmin())
                        {
                            SendMessage(sender, "accessDenied");
                            return;
                        }
                        if (args.Length != 2)
                        {
                            SendMessage(sender, "syntaxRemoveGroup");
                            return;
                        }
                        SendMessage(sender, removeUserGroup(args[1]));
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

                        rewards(sender);
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
                        lookup(sender, args[1]);
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
                    case "addg":
                        if(!permission.UserHasPermission(sender.UserIDString, "loyalty.addgroup") && !sender.IsAdmin())
                        {
                            SendMessage(sender, "accessDenied");
                            return;
                        }
                        if(args.Length != 3)
                        {
                            SendMessage(sender, "syntaxAddGroup");
                            return;
                        }
                        SendMessage(sender, addUserGroup(args[1], args[2]));
                        break;

                    default:
                        SendMessage(sender, "errorNoCommand");
                        break;
                };
            }
        }
        #endregion Main

        #region Subcommands
        string add(string alias, string permission, string timereq)
        {
            if (!Regex.IsMatch(timereq, "^\\d+$"))
                return FormatMessage("syntaxNotInt", 3);

            if (RewardExists(rust.QuoteSafe(permission)))
                return FormatMessage("rewardExists", permission);

            data.rewards.Add(new LoyaltyReward(rust.QuoteSafe(alias), rust.QuoteSafe(permission), Convert.ToUInt32(timereq, 10)));
            Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);

            return FormatMessage("successAdd", alias, permission, Convert.ToUInt32(timereq, 10));
        }

        string remove(string permission)
        {
            if (!RewardExists(rust.QuoteSafe(permission)))
                return FormatMessage("rewardNoExist", permission);
            foreach (LoyaltyReward reward in data.rewards)
                if (reward.permission == rust.QuoteSafe(permission))
                {
                   data.rewards.Remove(reward);
                   Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
                   return FormatMessage("rewardRemoved", permission);
                }
            return "errorFatal";
        }

        string reset(string playerName)
        {
            Player player = data.players.Values.FirstOrDefault(x => x.name.StartsWith(playerName, StringComparison.CurrentCultureIgnoreCase));
            if (player == null)
                return FormatMessage("errorPlayerNotFound", playerName);

            data.players[player.id].loyalty = 0;
            foreach (var reward in data.rewards)
            {
                rust.RunServerCommand("revoke user " + rust.QuoteSafe(player.name) + " " + reward.permission);
                SendMessage(BasePlayer.FindByID(player.id), "accessLost", reward.alias);
            }
            return FormatMessage("successReset", player.name);
        }

        string set(string playerName, string newLoyalty)
        {
            Player player = data.players.Values.FirstOrDefault(x => x.name.StartsWith(playerName, StringComparison.CurrentCultureIgnoreCase));
            if (player == null)
                return FormatMessage("errorPlayerNotFound", playerName);
            if (!Regex.IsMatch(newLoyalty, "^\\d+$"))
                return FormatMessage("syntaxNotInt", 2);

            data.players[player.id].loyalty = Convert.ToUInt32(newLoyalty, 10);

            foreach (var reward in data.rewards)
            {
                if (data.players[player.id].loyalty >= reward.requirement)
                {
                    rust.RunServerCommand("grant user " + rust.QuoteSafe(player.name) + " " + reward.permission);
                    SendMessage(BasePlayer.FindByID(player.id), "accessGranted", reward.requirement, Config["serverName"].ToString(), reward.alias);
                }
                if(data.players[player.id].loyalty < reward.requirement)
                {
                    rust.RunServerCommand("revoke user " + rust.QuoteSafe(player.name) + " " + reward.permission);
                    SendMessage(BasePlayer.FindByID(player.id), "accessLost", reward.alias);
                }
            }
            return FormatMessage("successSet", player.name, Convert.ToUInt32(newLoyalty, 10));

        }

        void rewards(BasePlayer sender) //Todo make less useless
        {
            foreach (var reward in data.rewards)
                SendMessage(sender, "entryReward", reward.alias, reward.permission, reward.requirement);
        }

        void lookup(BasePlayer sender, string player)
        {

            Player lookUpPlayer = data.players.Values.FirstOrDefault(x => x.name.StartsWith(player, StringComparison.CurrentCultureIgnoreCase));
            if (lookUpPlayer != null)
                SendMessageFromID(sender, "entryLookup", lookUpPlayer.id, lookUpPlayer.name, data.players[lookUpPlayer.id].loyalty);
            else
                SendMessage(sender, "errorPlayerNotFound", player);
        }

        void top(BasePlayer sender)
        {
            var topList = (from entry in data.players orderby entry.Value.loyalty descending select entry).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value);
            int counter = 0;
            SendMessage(sender, "Top 10 most loyal players");

            foreach (var entry in topList)
               SendMessageFromID(sender, "entryTop", entry.Value.id, ++counter, entry.Value.name, entry.Value.loyalty);
        }

        string addUserGroup(string usergroup, string requirement)
        {
            if (!Regex.IsMatch(requirement, "^\\d+$"))
                return FormatMessage("syntaxNotInt", 2);

            if (UserGroupExists(rust.QuoteSafe(usergroup)))
                return FormatMessage("groupExists", usergroup);

            data.usergroups.Add(new UserGroup(rust.QuoteSafe(usergroup), Convert.ToUInt32(requirement, 10)));
            Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);

            return FormatMessage("successAddGroup", rust.QuoteSafe(usergroup), Convert.ToUInt32(requirement, 10));
        }

        string removeUserGroup(string usergroup)
        {
            if (!UserGroupExists(rust.QuoteSafe(usergroup)))
                return FormatMessage("groupNoExists", usergroup);
            foreach (UserGroup usergroupEntry in data.usergroups)
                if (usergroupEntry.usergroup == rust.QuoteSafe(usergroup))
                {
                    data.usergroups.Remove(usergroupEntry);
                    Interface.Oxide.DataFileSystem.WriteObject("LoyaltyData", data);
                    return FormatMessage("groupRemoved", usergroup);
                }
            return "errorFatal";
        }
        #endregion Subcommands

        #region Helpers
        void SendMessage(BasePlayer receiver, string messageID, params object[] args)
        {
            rust.SendChatMessage(receiver, "",
               String.Format(lang.GetMessage("stylingMessage", this), (args.Length > 0 ? String.Format(lang.GetMessage(messageID, this), args) : lang.GetMessage(messageID, this))),
               Config["serverID"].ToString());
        }

        void SendMessageAsServer(BasePlayer receiver, string messageID, params object[] args)
        {
            rust.SendChatMessage(receiver,
               String.Format(lang.GetMessage("stylingSender", this), Config["serverName"]),
                String.Format(lang.GetMessage("stylingMessage", this), (args.Length > 0 ? String.Format(lang.GetMessage(messageID, this), args) : lang.GetMessage(messageID, this))), 
                Config["serverID"].ToString());
        }

        void SendMessageFromID(BasePlayer receiver, string messageID, ulong senderID, params object[] args)
        {
            rust.SendChatMessage(receiver, 
                String.Format(lang.GetMessage("stylingSender", this), ""),
                String.Format(lang.GetMessage("stylingMessage", this), (args.Length > 0 ? String.Format(lang.GetMessage(messageID, this), args) : lang.GetMessage(messageID, this))),
                Convert.ToString(senderID));
        }

        string FormatMessage(string messageID, params object[] args)
        {
            return String.Format(lang.GetMessage(messageID, this), args);
        }

        bool RewardExists(string permission)
        {
            foreach (LoyaltyReward reward in data.rewards)
                if (reward.permission == permission)
                    return true;

            return false;
        }
        bool UserGroupExists(string usergroup)
        {
            foreach (UserGroup usergEntry in data.usergroups)
                if (usergEntry.usergroup == usergroup)
                    return true;

            return false;
        }

        void RegisterMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["syntaxAdd"] = "<color=red>Too few or too many arguments. \nUse <color=grey>/loyalty add {string: /alias} {string: permission.permission {int: loyaltyrequirement}</color></color>",
                ["syntaxRemove"] = "<color=red>Too few or too many arguments. \nUse <color=grey>/loyalty remove {string: permission.permission}</color></color>",
                ["syntaxRemoveGroup"] = "<color=red>Too few or too many arguments.\nUse <color=grey>/loyalty removeg { string: loyaltygroup }</color></color>",
                ["syntaxSet"] = "<color=red>Too few or too many arguments. \nUse <color=grey>/loyalty set {string: username} {int: loyaltyPoints}</color></color>",
                ["syntaxReset"] = "<color=red>Too few or too many arguments. \nUse <color=grey>/loyalty reset {string: username}</color></color>",
                ["syntaxHelp"] = "<color=red>Too few or too many arguments. \nUse <color=grey>/loyalty help</color></color>",
                ["syntaxRewards"] = "<color=red>Too few or too many arguments. \nUse <color=grey/loyalty rewards</color></color>",
                ["syntaxLookup"] = "<color=red>Too few or too many arguments. \nUse <color=grey>/loyalty lookup {string: playername}</color></color>",
                ["syntaxTop"] = "<color=red>Too few or too many arguments. \nUse <color=grey>/loyalty top</color></color>",
                ["syntaxAddGroup"] = "<color=red>Too few or too many arguments. \nUse <color=grey>/loyalty addg {string: group} {int: loyaltyrequirement}</color></color>",
                ["syntaxNotInt"] = "<color=red>Invalid syntax. Parameter <color=grey>#{0}</color> needs to be a positive integer.</color>",
                ["rewardExists"] = "<color=red>A reward for the permission <color=grey>{0}</color> already exists.</color>",
                ["rewardNoExist"] = "<color=red>No reward for the permission <color=grey>{0}</color> was found.</color>",
                ["rewardRemoved"] = "Loyalty reward {0} was successfully removed.",
                ["accessGranted"] = "Congratulations, by spending <color=yellow>{0} minutes</color> on <color=yellow>{1}</color> you have gained access to the command <color=grey>{2}</color>. Thank you for playing!",
                ["accessDenied"] = "<color=red>You do not have access to that command.</color>",
                ["accessLost"] = "<color=red>You have lost access to <color=yellow>{0}</color> due to an administrator changing your loyalty.</color>",
                ["loyaltyCurrent"] = "You have accumulated a total of<color=yellow> {0} </color>loyalty points by playing on <color=yellow>{1}</color>",
                ["errorNoLoyalty"] = "<color=red>You have not yet earned any loyalty points. Check again in a minute!</color>",
                ["errorNoCommand"] = "<color=red>There's no command by that name.</color>",
                ["errorPlayerNotFound"] = "<color=red>No player by the name {0} was found.</color>",
                ["errorFatal"] = "FATAL ERROR. If you see this something has gone terribly wrong.",
                ["stylingMessage"] = "{0}",
                ["stylingSender"] = "<color=lime>{0}</color>",
                ["successSet"] = "Player {0}'s loyalty points were successfully set to {1}.",
                ["successReset"] = "Player {0}'s loyalty points were successfully reset.",
                ["successAdd"] = "Successfully added: {0} {1} {2}",
                ["successAddGroup"] = "Successfully added: {0} {1}",
                ["entryReward"] = "Alias: {0} Perm: {1} Req: {2}",
                ["entryTop"] = "{0}. <color=lime>{1}</color> - {2}",
                ["entryLookup"] = "<color=#6495ED>{0}</color> has accumulated a total of {1} loyalty points.",
                ["groupExists"] = "<color=red>A loyalty reward for the usergroup <color=grey>{0}</color> already exists.</color>",
                ["groupNoExists"] = "<color=red>No group reward called <color=grey>{0}</color> was found.</color>",
                ["groupRemoved"] = "Group reward {0} was successfully removed.",
                ["groupAssigned"] = "Congratulations, by spending <color=yellow>{0} minutes</color> on <color=yellow>{1}</color> you have been assigned the usergroup <color=grey>{2}</color>. Thank you for playing!",
                ["help"] = "<color=yellow>Loyalty by Bamabo</color>\nLoyalty is a plugin that lets server owners reward their players with permissions according to how much time they've spent on the server. 1 Loyalty = 1 minute. \n<color=grey>/loyalty add/remove/set/reset/rewards/top/lookup/addg/removeg</color>\n More info and source on <color=grey>github.com/Hazzty/Loyalty</color>",
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
            permission.RegisterPermission("loyalty.addgroup", this);
            permission.RegisterPermission("loyalty.removegroup", this);
        }

        #endregion Helpers
    }

}