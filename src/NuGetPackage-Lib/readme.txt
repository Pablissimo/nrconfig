NRConfig Library - http://pablissimo.github.com/nrconfig
=====================================================

Provides your project with the [NRConfig.Instrument] attribute that in combination
with the nrconfig.exe tool (separate NuGet package, search 'NRConfig Tool').

Install the NRConfig.Tool package, mark up your classes and methods then add
a post-build step to your project:

$(SolutionDir)packages\NRConfig.Tool.1.0.0.0\tools\nrconfig.exe /f all /i $(TargetPath) /o $(TargetDir)$(TargetName).NewRelic.xml

or run the tool manually as required.

You can also download the latest version of the tool as a standalone executable
that doesn't get installed via NuGet from

http://pablissimo.github.com/nrconfig