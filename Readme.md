nrconfig
========

Tool to automate generation of New Relic custom instrumentation XML files for .NET projects

## Overview
nrconfig is a command-line tool intended to support creating and maintaining [custom instrumentation](https://newrelic.com/docs/dotnet/CustomInstrumentation.html) files for use by New Relic.

**Note** I'm not affiliated with New Relic, Inc beyond being a happy customer of theirs and this tool is in no way endorsed by them. If there're issues with the tool or questions about the source, come to me and not their (lovely) support team as they'll not know what you're talking about.

## Usage

### Mark up your methods for fine-grained control
If you want to instrument specific methods or all of the methods within specific classes then the best approach is to mark them up with Instrument attributes.

* Install the nrconfig NuGet package into the projects you wish to instrument
* Annotate at the assembly, class or method level with the [Instrument] attribute
* Run nrconfig against the assemblies in question

```
nrconfig /i *.dll
```

#### [Instrument] attribute
You can apply the Instrument attribute at the assembly, class or method/property level. You need not supply any parameters, though you can configure the output of the tool by optionally setting:

* **InstrumentationScopes** - a combination of flags that determines what targets should be considered or rejected at the level of the [Instrument] attribute and below
    - **None** - nothing from this point onwards should be instrumented
    - **PublicProperties**
    - **NonPublicProperties**
    - **Properties** - all property accessor methods should be instrumented irrespective of visibility (equivalent to specifying both PublicProperties | NonPublicProperties)
    - **PublicMethods**
    - **NonPublicMethods**
    - **Methods** - all methods should be instrumented irrespective of visibility (equivalent to PublicMethods | NonPublicMethods)
    - **PublicConstructors**
    - **NonPublicConstructors**
    - **Constructors** - all constructors should be instrumented irrespective of visibility (equivalent to PublicConstructors | NonPublicConstructors)
    - **All** - every method, property accessor method and constructor should be instrumented (equivalent to Properties | Methods | Constructors)
* **Name** - the name of the tracerFactory; for example "NewRelic.Agent.Core.Tracer.Factories.BackgroundThreadTracerFactory", or "NewRelic.Agent.Core.Tracer.Factories.IgnoreTransactionTracerFactory"
* **MetricName** - the name of the metric
* **TransactionNamingPriority** - priority for transaction naming, which can be in the range "1" to "7" (with "7" being highest-priority)
* **IncludeCompilerGeneratedCode** - determines whether classes and methods marked with the [CompilerGenerated] attribute should be instrumented or excuded
Available items:
* **Name** - name for the tracerFactory (transaction "NewRelic.Agent.Core.Tracer.Factories.BackgroundThreadTracerFactory" and ignore "NewRelic.Agent.Core.Tracer.Factories.IgnoreTransactionTracerFactory")
* **MetricName** - name for metric
* **TransactionNamingPriority** - priority for transaction naming. Valid values are "1" to "7", where "7" takes precedence over "1" to "6".

The actual settings used when considering any given method for inclusion are a combination of the settings specified using [Instrument] attributes at the method, class and assembly levels.

* If you specify a setting at two levels, the one at the lower level takes precedence
    - e.g. Specifying InstrumentationScopes="All" at the class level, then InstrumentationScopes="None" on a method within that class will exclude that one method from instrumentation
* If you do not specify a particular value for a setting at a lower level but do at a higher level, the higher level value is inherited
    - e.g. Specifying Name at an assembly level, but not specifying it at the class level will cause methods within the class to be instrumented using the factory named at the assembly level

### Generate instrumentation files for unadorned assemblies, including the BCL
If you can't or don't want to change your code, or want to quickly get a baseline instrumentation configuration file that you can then tweak manually then you can use the /f flag.

```
nrconfig /i *.dll /f [filters]
```

where [filters] is one or more of:

* all
* methods
* properties
* constructors

separated by spaces, and where each of those can be followed immediately by a + or - to limit processing to public or non-public targets only. So, to instrument all public methods and properties in any class in an assembly:

````
nrconfig /i MyAssembly.dll /f methods+ properties+
````

while to instrument all methods, whether public or private:

````
nrconfig /i MyAssembly.dll /f methods+-
````

### Merge multiple configuration files
The filter format is pretty basic, and you might want to use different filters for different assemblies. You can do that by running nrconfig multiple times with different output files specified, then merge them with the /m flag:

````
REM Instrument all methods in FirstAssembly, 
REM and public ones in SecondAssembly
nrconfig /i FirstAssembly.dll /f methods+- /o FirstAssembly.NewRelic.xml
nrconfig /i SecondAssembly.dll /f methods+ /o SecondAssembly.NewRelic.xml

REM Merge the two files
nrconfig /m /i *.NewRelic.xml /o MergedInstrumentation.xml
````

### Options

#### /i &lt;file_1&gt; [file_2] ... [file_n]
Specifies the input files to be processed. These can either be specific paths, include wildcard strings in the filename portion or include environment variables.

If used in conjunction with the /m switch the files specified by /i must be XML documents previously generated by nrconfig.

#### /o &lt;outputpath&gt;
Specifies the output filename. If /o isn't specified, a file called CustomInstrumentation.xml will be generated in the current directory.

#### /f &lt;filter1&gt; [filter2]
Specifies that all methods, constructors and properties matched by the filters should be instrumented even if they haven't been annotated by [Instrument] attributes. Each filter must be from the set:

* all
* properties
* methods
* constructors

and each can be immediately followed by + or - (or both) signifying public or non-public respectively. If neither + nor - is specified, + is assumed.

#### /w &lt;typename_1&gt; ... [typename_n]
Specifies that only types whose full names match those specified should be included - only used in conjunction with /f flag. Supports wildcards, for example:

* *Repository would match all types whose full name ends with 'Repository'
* System.Web.* would match all types in the System.Web namespace

#### /m
Merges input instrumentation files into a single output file.

#### /v
Verbose mode, shows a little more diagnostic information during the run.

#### /debug 
Extra-verbose mode, generates a lot of console output but useful if you're about to report a bug.

## Licence
nrconfig is licensed under the [BSD 2-Clause Licence](http://opensource.org/licenses/BSD-2-Clause):

Copyright (c) 2013, Paul O'Neill, pablissimo.com
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
