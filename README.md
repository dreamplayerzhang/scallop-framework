# scallop-framework
(Automatically exported from code.google.com/p/scallop-framework)

Home page
-------------
http://www.ee.oulu.fi/research/imag/scallop/
Documentation: http://www.ee.oulu.fi/research/imag/scallop/doc/Index.html

Introduction
-------------
Scallop framework aims to provide the user with simple and configurable interfaces for accessing sensors and communicating with other nodes. Various sensor and network types can be used, enabling node heterogeneity. Uniform event based interfaces are provided for both sensors and networks, with bindings for Microsoft .NET Framework (.NET) languages and we have plans for C++ support. Node deployment and runtime configuration is simplified through the use of XML configuration files.

The framework has been designed and implemented with the following guidelines in mind:

- Allow researchers to focus on machine vision algorithms by providing access to sensors and communication networks.
- Allow new sensor and network types to be implemented easily by defining an interface that implementations must conform to.
- Keep the interfaces simple and uniform over different types of sensors and networks.
- Separate sensors and communication from machine vision algorithms.
- Allow customisations by releasing/open sourcing the code.
- The .NET Framework was chosen as the implementation platform due to it's increasingly widespread use, availability of tools and built-in support for networking, databases and other needed resources. User code is expected to run on PC workstations (for ease of development), a platform for which the .NET Framework is well suited for. This environment lends itself well to rapid prototyping and development.

> **Note:**
>As of version R2009a, MATLAB includes an interface to the .NET Framework. In consequence of this, it is now possible to use the Scallop framework even in MATLAB environment. The source code package includes an example of using MATLAB to access frames from an Axis IP camera and doing some image processing.
