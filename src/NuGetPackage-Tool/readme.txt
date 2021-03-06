﻿NRConfig Tool - http://pablissimo.github.com/nrconfig
=====================================================

Adds the nrconfig.exe tool to your solution, which can be used to generate
custom New Relic instrumentation configuration files.

You can then either add a post-build step or manually run the tool.
Alternatively there is an MSBuild task version of the tool that you can
find on NuGet as 'NRConfig.MSBuild' that will automatically generate
instrumentation configuration for the project it's added to.

If you're going the manual route, the following post-build will 
process the target assembly and output a configuration file with its name 
suffixed with '.NewRelic.xml':

$(SolutionDir)packages\NRConfig.Tool.1.5.0.0\tools\nrconfig.exe /f all /i $(TargetPath) /o $(TargetDir)$(TargetName).NewRelic.xml

It will add every public method, constructor and property accessor to your
instrumentation file - you can modify this by changing the value of the /f
parameter - see the nrconfig website (http://pablissimo.github.com/nrconfig) 
for details.

You can optionally install the NRConfig Library NuGet package to individual
projects which allows you to mark-up your assemblies, classes and methods
with an [Instrument] attribute to give finer-grained control of what ends
up being instrumented.

If you mark up your assembly in this way, remove the /f from the post-build
step command:

$(SolutionDir)packages\NRConfig.Tool.1.5.0.0\tools\nrconfig.exe /i $(TargetPath) /o $(TargetDir)$(TargetName).NewRelic.xml