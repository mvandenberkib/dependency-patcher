# Dependency Patcher
A cross platform tool created using .NET Core 6.0 with Visual Studio Community 2022.

## The problem
When attempting to patch in a set of classes from one application into another some problems occured. The patched in class depended on certain Android classes that were already included into the target application. However, these classes were obfuscated so they wouldn't work as the patched in class could not find the correct functions by name.

## The solution
Renaming all references in the patched in class as well as the dependencies it relies on in order to be usable in the target application while not conflicting with its dependencies.

The application takes all files and classes and appends Patched to them. This is also how they are expected to be referred to in the code.

## How to use it
1. Disassemble the application that contains the class you want to patch in using APKTool.
2. Disassemble the target application you want to patch the class into using APKTool.
3. Run the application.
4. Enter the path and name of the folder the first application (the one having the class you want patched in) disassembled into. So the one holding folders named 'smali' and 'smali_classes1', 'smali_classes2', etc.
5. Then after waiting for a while (can take a long time depending on the amount of files) enter the path and name of the folder the second application (the one you want to patch) disassembled into. Again, this is the folder holding the 'smali', 'smali_classes1', etc. folders.
6. The class you want to use is now patched into the application and ready for use! Just make sure you don't forget to use the modified names. Every class name now has 'Patched' appended to it.
