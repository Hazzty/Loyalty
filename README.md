<h1>Loyalty</h1>
Have you ever wanted to give your loyal(tm) players additional permissions/usergroups? - Then Loyalty(tm) is for you!

Loyalty allows administrators to reward their players with new permissions and usergroups according to their playtime.

<h2>What is Loyalty used for?</h2>
Maybe your server gets full fairly often and you allow players to donate to skip the queue. Sadly, not everyone has the money to spare to skip the queue. This leaves out some of your most loyal players that's been there from the start. That's when Loyalty comes in handy! Just make a reward for example 24 hours of gametime gives players the permission to skip queue.
You get to see familiar faces when the server is full and your Loyal players get to actually keep playing on your server. It's a win-win situation!

<h2>Loyalty points</h2>
Loyalty points is the way Loyalty measures how long a player has spent on the server. Every minute(by default) the plugin checks which players are online. Everyone that is online recieves 1 loyalty point. When a player reaches a loyalty point requirement for a given permission/usergroup this player is granted that permission/usergroup. Simple stuff!

<b>Note:</b> Whenever a player is assigned a new usergroup by time or via /loyalty set the player will be removed from their previous group. It also will not impact any usergroups assigned via the console as long as they are not a reward in Loyalty too.

<h2>Commands</h2>
* <b>/loyalty</b> - return your current loyalty points
* <b>/loyalty add {int: loyaltyrequirement} {string: (-)permission} {string: alias} </b> - Add a new loyalty reward
* <b>/loyalty remove {string: permission}</b> - Remove an existing loyalty reward
* <b>/loyalty set {string: playername} {int: newLoyalty}</b> - Set a users loyalty points to a specific value
* <b>/loyalty reset {string: playername}</b> - Set a users loyalty points to 0
* <b>/loyalty top</b> - Display a list of the top 10 loyal players on the server
* <b>/loyalty lookup {string: playername}</b> - Lookup the loyalty points of a player
* <b>/loyalty help</b> - Shows basic information about the plugin
* <b>/loyalty addg {int: loyaltyrequirement} {string: (-)groupname}</b> - Adds a usergroup as a loyalty reward 
* <b>/loyalty removeg {string: groupname}</b> - Removes a usergroup loyalty reward
* <b>/loyalty rewards</b> - Lists the senders next 5 upcoming permission rewards
* <b>/loyalty rewardsg</b> - Lists the senders next 5 upcoming usergroup rewards

<h2>Permissions</h2>
* <b>loyalty.loyalty</b> - Allows the use of /loyalty
* <b>loyalty.add</b> - /loyalty add
* <b>loyalty.remove</b> - /loyalty remove
* <b>loyalty.set</b> - /loyalty set
* <b>loyalty.reset</b> - /loyalty reset
* <b>loyalty.top</b> - /loyalty top
* <b>loyalty.lookup</b> - /loyalty lookup
* <b>loyalty.help</b> - /loyalty help
* <b>loyalty.addgroup</b> - /loyalty addg
* <b>loyalty.removegroup</b> - /loyalty removeg
* <b>loyalty.rewards</b> - /loyalty rewards
* <b>loyalty.rewardsg</b> - /loyalty rewardsg

<h2>Config</h2>
The config file for the plugin is located in Oxide's default config file folder(serverRoot/server/serverIdentity/oxide/config)
* <b>"serverIconID": "76561198314979344"</b> - Change "76561198314979344" to the steam id of a steam user whose profile picture you want the plugin to use when sending messages. Default is using Loyalty's icon.
* <b>"serverName": "Default Server"</b> - Change "Default Server" to whatever server name you want displayed when the server sends messages.
* <b>"allowAdmin": true</b> - Set this to true to allow admins loyalty to be recorded.
* <b>"colorError": "red"</b> - Sets the color of your error messages.
* <b>"colorHighlight":"yellow"</b> - Sets the color of highlighted words and sentences
* <b>"colorText": "#FFFFFF"</b> - Sets the color of pretty much all messages apart from errors/highlights
* <b>"debug": false</b> - Puts information to the console
* <b>"rate": 60.0</b> - The rate of which players are given loyalty points in seconds

All the colors support both hex colors "#FFFFFF" as well as plain color names("red", "blue", etc)

<h2>Lang</h2>
The lang file is located in oxide's default lang folder(serverRoot/server/serverIdentity/oxide/lang). It contains all the strings that are used in the plugin, these can be customized but don't be surprised if you mess something up by fiddling with them. Don't worry though, just delete the file and it will recreate itself on your next restart/reload.
