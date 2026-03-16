
# RoboPatch (Alpha)

RoboPatch is a patcher that allows injecting custom assets into the game Robotopia by Tomato Cake Inc.

# How does it work?

RoboPatch works by using BepInEx, A open source patcher and plugin loader for Unity based games and the Harmony libary: A library for patching, replacing and decorating .NET and Mono methods during runtime. Before the game loads its assets RoboPatch loads custom assets by tricking Unity into thinking that RoboPatch's assets are what its supposed to load and injecting the custom assets. All changes are temporary and if you delete BepInEx your game will run normally again.

# Installation guide

* **Please keep in mind RoboPatch has not been tested on linux or macOS!**

## Requirements:
* Latest Robotopia build
* BepInEx Bleeding Edge

## Installation

First of all you need the game, which you can download from the Robotopia Discord server right here: https://discord.gg/5vQvxFNDGJ.

Afterwards you need to download and install the latest BepInEx Bleeding Edge build, You can do so here: https://builds.bepinex.dev (The reason it uses Bleeding Edge is because Unity 6 is not supported by BepInEx Stable right now!)

Once you have downloaded BepInEx you need to unzip the file and paste the contents inside of the Robotopia folder after you have installed the game.

**IMPORTANT**: You need to run the game at least *ONCE* for BepInEx to create the necessary files.

Once BepInEx is installed you just need to download the latest release of RoboPatch from... well... the Releases page. Once you downloaded it, Unzip the folder and put it inside of the folder you installed Robotopia and BepInEx and merge the folders. 

**Hooray, You now have RoboPatch installed!**

Right now all RoboPatch does is allow you to change "SystemPrompt" using a text file inside the same directory... sad but its because this is more of a "Proof of concept" than a finished polished modloader or something... BUT if you want to help make it a finished polished modloader you can help contribute!

# How to contribute

If you don't know how to clone a repo... look here: https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository

All you gotta do to contribute is to clone the repo, Make your changes, and make a pull request to the repo where I will review it and see if its a good change.

If you want some suggestions for changes it would be nice if:
* We had a better README
* The code is more stable and allows people to actually use it as a foundation to build good mods
* Tbh all I can think about, if your reading this you can just make a pull request and add your suggestion lol.

# Credits
* Robotopia Dev team / Tomato Cake Inc: For allowing me to create this project. https://discord.gg/5vQvxFNDGJ
* BepInEx team: For the whole weird patch framework. https://github.com/BepInEx/BepInEx
* Harmony team: For the weird patch... libary?? https://github.com/pardeike/Harmony



