# Transfer - Encode - Decode

The purpose of this application to aid in transferring files over a restricted RDP session.

Often in business, IT admin will restrict an RDP session for copying files from or to the session from or to your machine, however they will not restrict text from the clipboard being copied and pasted.

Therefore, I've created this handy little tool as a workaround (and yes, this is mischievous).

All it does, is encode the file to Ascii85 (base85), compress it and copy the Ascii85 to the clipboard. On the other end, you run the utility to read from the clipboard, convert the Ascii85 back to bytes (and decompress) and write the bytes to the disk.

It's similar to running this in a command window on your machine:

```
certutil -encode "c:\temp\file.exe" "c:\temp\file.txt"
```

Copying the contents of the text file to clipboard, then on the other end:

```
certutil -encode "c:\temp\file.txt" "c:\temp\file.exe"
```

To produce the original file again.

However, this application gets added to Windows right-click context menu.

**Right Click On File:**

[![N|TransferEncodeDecode](https://portfolio.jb-net.co.uk/shared/ted-1.png)]()

**Right Click On Directory Background:**

[![N|TransferEncodeDecode](https://portfolio.jb-net.co.uk/shared/ted-2.png)]()

## Command Line Arguments

|Argument|Description|Additional|
|-|-|-|
|-e|Encode|Path of file to encode|
|-d|Decode|Path to folder where to decode to|
|-u|Uninstall|Remove registry keys and remove context menu (uninstall)|
|No arguments|Install|Set registry keys and add context menu (install)|

You can install by simply running the .exe... **However**, you must ensure you don't move the exe to another location, so place the .exe somewhere it's going to stay and run it once. You can run again to reset the registry keys needed for the Windows right-click context menu to work.

## But How Do I Get The EXE On The Remote Session?

Initially, use the `certutil` method.

## Need Admin Access For A File?

The program will restart itself and ask for admin privileges.