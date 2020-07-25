# C#-Growtopia-Server
A C# Growtopia Server made by kevz#2073 (NOT PURE!)

# Credits
- GrowtopiaNoobs (Some of his c++ functions are converted)

- Williyao123 (Methods, local variables, packet) 

- cmd#1000 (some of functions are taken from him)

- Alexander#6398 (Helped me much at this)

Special thanks to this people!

# How-To-Use
1. Make sure you have installed Enet.Managed library version 3.0.2 and MySql.Data any version.

2. Decode items.dat and place it to the same DIRECTORY as your ".exe" in. (This steps is important because the blocks database uses decoded items.dat instead of CoreData)

3. Place items.dat,descriptions,CoreData in the same directory. (CoreData needed for clothing only.)

4. You can build in both debug/release

5. Enjoy!

# Info

You need to pass in your connection string in DbContext class since database uses MySql instead of json file.

Don't forget to change the items.dat hash in "OnSuperMain"

Some of functions aren't included, such as drop, and so on

All the methods use async, that means the server will do a things before waiting another things. Make alot faster.

Added growscan

Worlds are not saved to the database, you need to add it urself
