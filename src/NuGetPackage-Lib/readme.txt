NRConfig Library - http://pablissimo.github.com/nrconfig
========================================================

This library lets you annotate assemblies, types and methods with an
[Instrument] attribute that lets the NRConfig.Tool package pick them up and 
include them in a New Relic custom instrumentation configuration file.

You'll also need either the NRConfig.Tool package with post-build events set 
up, or the NRConfig.MSBuild package installed into each project you want to 
annotate - that's the easiest solution.

IMPORTANT! Upgrading from 1.2.0? You'll also need to update NRConfig.Tool to 
the latest version