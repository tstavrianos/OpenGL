# OpenGL
## Simple OpenGL bindings
Over time I have tried nearly all of the OpenGL bindings for CSharp but I will always find something that will ever so slightly irritate me.
As a result I had a look at the Khronos XML OpenGL spec and this project is the result of a couple of days of work.

### Generator
Using a function from https://github.com/luca-piccioni/OpenGL.Net and some code from https://github.com/senzible/senzible-opengl , I parse the XML spec (using classes generated from XML2Sharp) and then go through the binding generation, with the result being split into 7 files:
#### Constants
Contains all the OpenGL enums/constants, defined as static & readonly fields (to allow me to use `using static Constants` to reference them directly from CSharp as I would I C/C++).
#### Delegates
Internal definition of the OpenGL function signatures
#### Enums
Strongly typed enums, to provide an alternative way of calling the OpenGL functions.
#### Features
All the features listed in the OpenGL spec, defined as booleans. Only the GL version ones will get set during load (for now).
#### Loader
Main class that will take care of loading all the supported OpenGL functions and filling the extensions hash. 
The Init function works in several steps to:
- Determine the supported features.
- Load the functions for the supported features.
- Optionally disable anything that the spec says was removed (default = disabled).
- Optionally determine the supported extensions (default = enabled).
- Optionally load the functions added by the supported extensions (default = enabled).
- Optionally load any leftover functions that were in the spec (default = disabled).
#### Pointers
Internal static variables that hold the loaded opengl functions.
#### Wrappers
- Functions pointing to the internal opengl functions.
- Functions that allow using arrays instead of pointers (unsafe array pining for the time being).
- Functions using the enums as an alternative calling method.

The generated bindings have been tested using .NET Core 2.0 and compile with no errors. 

### Credits
- Constant declaration code was copied from https://github.com/luca-piccioni/OpenGL.Net
- StringBuilder wrapper and type mapping was copied from https://github.com/senzible/senzible-opengl
- XML parsing is based on classes generated using http://xmltocsharp.azurewebsites.net/