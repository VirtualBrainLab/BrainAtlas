# BrainGlobe Atlas -> BrainAtlas Unity Format

Each atlas (e.g. CCF) in BrainGlobe has a shape (ap, dv, ml) and a resolution (25um), a list of brain regions with acronyms ("grey") and ID #s (8). The origin coordinate is always the "front, top, left" coordinate. Each brain region also has a mesh file attached to it. 

## Create metadata files

We need to define for each atlas a base CoordinateSpace metadata file and a CSV file that will contain the minimal region data and colors.

## Download atlas annotation file

We'll download the annotation file locally and save it in a format readable by Unity (raw bytes)