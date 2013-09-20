Qbes voxel engine (by mcodl)
----------------------------

In late sumer 2012 I got really bored and also frustrated by certain's popular voxel game underperformance. I asked myself: does it really have to be that slow? How comes that Crysis 3 runs better than that game? Since I work as a business programmer I had my suspicions of what was wrong when observing resource consumption in the profiler.

I still remembered some OpenGL stuff from my time back in the university so I said myself: I'll write a voxel engine that will have reasonably low memory, CPU and GFX requirenments so that it could be extended for whatever game.

Although I have put together a working alpha stage engine I am hardly a games developer. And since I work full-time in a certain bank I don't have time to take it further. This is were GitHub comes in. I hope that someone will use his/her free time and do something nice with this.

If you do then I have just the following requests:
- Keep it all opensource under LGPL 3
- Check here and there if it runs under .Net 4.0 equivalent Mono as I'd like it to run on Linux as well

Later on I will update a PDF with description on how the engine works in the inside so that you don't have to just work with the code's comments.

And while speaking about code: there are many prototype and experimental parts which are quite messy but the most should be properly organized (from a business developer's point of view that is :-) ).

Used libraries:
- SdlDotNet
- Tao
- LZMA (7zip managed library)
- LidgrenNetwork
- NetworkComms

Unused but present libraries
- SevenZipSharp (excluded as it is a managed to native wrapper which caused issues with Mono)

Native libraries are supplied only for Windows as under Linux these are usually installed through the distribution's packaging system.