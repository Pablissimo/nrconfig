NRConfig Library - http://pablissimo.github.com/nrconfig
=====================================================

Provides your project with the [NRConfig.Instrument] attribute that in combination
with the nrconfig.exe tool (separate NuGet package, search 'NRConfig Tool').

Install the NRConfig.Tool package, mark up your classes and methods then add
a post-build step to your project:

nrconfig.exe /i $(TargetPath) /o $(TargetDir)$(TargetName).NewRelic.xml