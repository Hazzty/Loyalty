<h1>This plugin is under development and not recommended for use on a live server environment!</h1>
<h1>Loyalty</h1>
Loyalty is an Oxide plugin for Rust that allows server owners to grant players permissions depending on how long they've spent on the server.

 <h2>Information</h2>
First of all this plugin is under development and not recommended for use on a live server environment. Also the plugin is under development and not recommended for use on a live server environment. 

If you find any bugs or have any suggestions for the plugin make sure you file an issue or let me know in any other way.
 
<h2>Commands</h2>
* <b>/loyalty</b> - return your current loyalty points
* <b>/loyalty add {string: alias} {string: permission} {int: loyaltyrequirement}</b> - Add a new loyalty reward
* <b>/loyalty remove {string: permission}</b> - Remove an existing loyalty reward
* <b>/loyalty set {string: playername} {int: newLoyalty}</b> - Set a users loyalty points to a specific value
* <b>/loyalty reset {string: playername}</b> - Set a users loyalty points to 0
* <b>/loyalty top</b> - Display a list of the top 10 loyal players on the server
* <b>/loyalty lookup {string: playername}</b> - Lookup the loyalty points of a player
* <b>/loyalty help</b> - Shows basic information about the plugin

<h2>Config</h2>
The config file for the plugin is located in Oxide's default config file folder(serverroot/server/serveridentity/oxide/config)
* <b>"serverID": "0"</b> - Change "0" to the steam id of a steam user whose profile picture you want the plugin to use when sending messages.
* <b>"serverName": "DefaultServer"</b> - Change "DefaultServer" to whatever server name you want displayed when the server sends messages.

<h2>Lang</h2>
The lang file is located in oxide's default lang folder. It contains all the strings that are used in the plugin, these can be customized but don't be surprised if you mess something up by fiddling with them.
