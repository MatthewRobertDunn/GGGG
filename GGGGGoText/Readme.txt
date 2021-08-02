

This is a go text protocol engine written by Matthew Dunn

This binary is for personal testing use only and should not be distributed.

You can plug it into any UI supporting GTP. Eg. SmartGo


This UI will write a training files into \Users\USER\AppData\Local\MattyGo which MattyGo can learn from to increase 
playing strength later.


Command line options:

GGGGGoText.exe think  

Allow think in opponent time, increases playing strength, but not so good for bot vs bot matches


GGGGGoText.exe dontpass

Don't pass early, makes the UI less agressive in passing

GGGGGoText.exe dontresign

Don't resign early, makes the UI less agressive in resigning

GGGGGoText.exe movetime 20
	
Take 20 seconds per move by default; useful if the UI isn't passing time control info.
This overrides any settings in the .config file

GGGGGoText.exe selftest

Developer only, run some kind of self test, prints to console, not compatible with GTP


You can combine some options

GGGGGoText.exe think dontpass movetime 20


