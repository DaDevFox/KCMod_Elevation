# How to use

## Calling Commands

This API system makes use of the [Zat InterModComm (IMC) library](https://github.com/BigMo/KCMods/blob/master/Scripts/Shared/Zat.InterModComm.cs). In order to use it, you must have an IMCPort that can send messages to the target with the name `ElevationAPI`. Below is a list of messages you can send to the `ElevationAPI` port that will be processed and make something happen. 

Any command's given name is `Category:CommandName`, or with multiple categories `Category:SubCategory:CommandName`, so for example `Elevation:Get`, or `Elevation:Set`

Due to how the IMC library works, any given function is only allowed one return value and one argument, but that argument/return value can be a custom object you created, so you can make an object with multiple fields thereby storing multiple arguments; under **Data Structures** is a list of objects I have used to implment messages, you do not have to name your objects the same when sending a message, but they **must** have the same fields. 

See below for an example of using a command: 
```cs
// In this example we will ask the elevation mod for the elevation of a tile at (1, 5)
// The first thing to do is check the docs, it says Elevation:Get is a function that has a Vector2 argument called position and returns an int. 

void SceneLoaded(){
    // The following code can be called anywhere in your mod, however it must be called *after* Preload

    // This will send a message to the Elevation mod to find the elevation of a tile at the coordinates (1, 5)
    port.RPC<Vector2 /* type of argument */, int /* type of return value */>("ElevationAPI" /* target */, "Elevation:Get" /* function to call */, new Vector2(1, 5) /* argument (position of tile) */, 30 /* timeout */, OnComplete /* what to do when complete */, OnError /* this will be called if something goes wrong (maybe the user doesn't have the elevation mod) */ )
}

private void OnComplete(int elevation){
    // this will be called once we recieve a response from the Elevation mod, and the argument `elevation` to this function is the elevation of the tile at (1, 5) 
}

private void OnError(){
    // whot to do when something goes wrong
}
``` 
where `port` is an object of type IMCPort. 


## Submods

### To Create a mod

**Note: experience in coding ([C#](https://docs.microsoft.com/en-us/dotnet/csharp/)) and the [Unity game engine](https://unity.com/) required**

If you would like to create submods for Elevation, download the source (either by subscribing on Steam or going to the GitHub page) and make a copy; inside of the submods folder, create your submod in its own folder seperate from the rest (External/Submods) once you have completed it, contact me, I am available on the Kingdoms and Castle discord at @Agentfox#7515, and I will integrate it into the source code. Make sure that somewhere in your submod, you have a bool called `enabled`, and if this is false, the submod should do nothing. When you submit your submod to me, also be sure to give it a unique string ID that is not the same as any other submod. 

This is not all however, by default, your submod will be disabled. Now that it is a part of Elevation, when somebody subscribes to the Elevation mod they are getting the base mod + submods that have been integrated, but they didn't ask for that, so how it works is, the submods are disabled by default, and as a mod creator, you must create a mod on Steam that has a little code that enables your submod. 

So to be clear, on Steam there will be 2 mods, the base mod (Elevation) and your submod. The base mod contains your actual submod code but doesn't use it unless told to. The submod simply tells the base mod to enable your submod. 

To tell Elevation to 

#### Submods and Communication Explanation

This may seem counterintuitive, however the reason submods must be done this way is because, due to the way Kingdoms and Castles works, mods may not, directly, communicate with each other. To better understand what exactly this means, let's walk thorugh an example:
In my mod, Elevation, there's a class, `UI`, it contains a reference to the Creative Mode Raising/Lowering UI, say a submod wanted to add more buttons to it. In the submod's code, you can get the GameObject `UI.raiseLowerUI` and add a child to it; except you can't, in your choice of IDE you can add a reference to `ElevationMod.dll` (if I were to build the mod to a dll), and you can access the `UI` class, but when mods load up, the game creates a sort of 'box' where they are placed, inside the box they are only allowed to access a predetermined list of references that include Harmony, the System libraries, and the UnityEngine libraries. 

So if mods cannot call methods, read vars, or even access any code from eachother, how can they possibly communicate? Well, while mods are not allowed to access eachother's libraries, they **all** have access to a group of core libraries, the ones mentioned above. Through these libraries, there are some ways to possibly communicate; one way is to use the Unity scene structure and have each mod drop a GameObject in a specified location as their representative and use Unity's `SendMessage` function to send messages to eachother. @Zat#8030 on the KC Discord has kindly compiled this into a library, the IMC (Inter Mod Comm) library. 

Because the only way for mods to communicate is in this way, it would be extremely cumbersome to write a submod as a seperate mod, so submods are integrated as part of the source. 


