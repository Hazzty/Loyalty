<h1>This plugin is under development and not recommended for use on a live server environment!</h1>
<h1>Loyalty</h1>
Loyalty is an Oxide plugin for Rust that allows server owners to grant players permissions depending on how long they've spent on the server.

<h2>Information</h2>
First of all this plugin is under development and not recommended for use on a live server environment. Also the plugin is under development and not recommended for use on a live server environment. 

If you find any bugs or have any suggestions for the plugin make sure you file an issue or let me know in any other way.

<h2>Loyalty points</h2>
Loyalty points are a way to measure how long a player has spent on the server. Every minute the plugin checks which players are online. Everyone that is online recieves 1 loyalty point. When a player reaches a loyalty point requirement for a given permission this player is granted that permission. Simple stuff!

<h2>Commands</h2>
* <b>/loyalty</b> - return your current loyalty points
* <b>/loyalty add {string: alias} {string: permission} {int: loyaltyrequirement}</b> - Add a new loyalty reward
* <b>/loyalty remove {string: permission}</b> - Remove an existing loyalty reward
* <b>/loyalty set {string: playername} {int: newLoyalty}</b> - Set a users loyalty points to a specific value
* <b>/loyalty reset {string: playername}</b> - Set a users loyalty points to 0
* <b>/loyalty top</b> - Display a list of the top 10 loyal players on the server
* <b>/loyalty lookup {string: playername}</b> - Lookup the loyalty points of a player
* <b>/loyalty help</b> - Shows basic information about the plugin
* <b>/loyalty addg {string: groupname} {int: loyaltyrequirement}</b> - Adds a usergroup as a loyalty reward 
* <b>/loyalty removeg {string: groupname}</b> - Removes a usergroup loyalty reward

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

<h2>Config</h2>
The config file for the plugin is located in Oxide's default config file folder(serverRoot/server/serverIdentity/oxide/config)
* <b>"serverIconID": "00000000000000000"</b> - Change "00000000000000000" to the steam id of a steam user whose profile picture you want the plugin to use when sending messages.
* <b>"serverName": "DefaultServer"</b> - Change "DefaultServer" to whatever server name you want displayed when the server sends messages.
* <b>"allowAdmin": false</b> - Set this to true to allow admins loyalty to be recorded.

<h2>Lang</h2>
The lang file is located in oxide's default lang folder(serverRoot/server/serverIdentity/oxide/lang). It contains all the strings that are used in the plugin, these can be customized but don't be surprised if you mess something up by fiddling with them. Don't worry though, just delete the file and it will recreate itself on your next restart/reload.
