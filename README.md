# BeMoBI
The first implementation of an experiment using the BeMoBI Framework
Providing a Spatial cognition paradigm to show how such a thing could implemented with Unity3D


# Setup

### Submodules [optional]
Only necessary when your working with the git commandline tools!
After cloning this repository, call:

> git submodule init; git submodule update

To get the BeMoBI.Unity3D framework and the LSL4Unity components through a submodule reference to their latest versions.

### Oculus VR Unity Integration
The project works with the latest OVR Runtime (0.8.x) and the Oculus Utilities for Unity3D

# First Steps

0. Make a branch for your investigations! :)
1. Take a look at the Assets/Paradigm directory.
2. Load the 'SearchAndFind' scene.
3. Take a look at scene hierarchy and the top level elements.


## major issues
Currently only Windows >= 7 is supported to run the project! 

Reasons:
 - current version of OVR SDK and Runtime supports only Windows :/
 - the current maintainer of the project has no Mac environment available to test and build anything...
 

# Recommendations
* using Visual studio code https://code.visualstudio.com/Docs/runtimes/unity