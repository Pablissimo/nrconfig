NRConfig Tool - http://pablissimo.github.com/nrconfig
=====================================================

Adds the nrconfig.exe tool to your solution, which can be used to generate
custom New Relic instrumentation configuration files.

You can then either add a post-build step or manually run the tool. The
following post-build will process the target assembly and output a
configuration file with its name suffixed with '.NewRelic.xml':

nrconfig.exe /i $(TargetPath) /f all /o $(TargetDir)$(TargetName).NewRelic.xml

It will add every public method, constructor and property accessor to your
instrumentation file - you can modify this by changing the value of the /f
parameter - see the nrconfig website for details.

You can optionally install the NRConfig Library NuGet package to individual
projects which allows you to mark-up your assemblies, classes and methods
with an [Instrument] attribute to give finer-grained control of what ends
up being instrumented.

If you mark up your assembly in this way, remove the /f from the post-build
step command:

nrconfig.exe /i $(TargetPath) /o $(TargetDir)$(TargetName).NewRelic.xml