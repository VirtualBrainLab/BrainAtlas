# BrainAtlas

This repository is a wrapper around the [BrainGlobe Atlas API](https://github.com/brainglobe/bg-atlasapi). The pipeline files download the atlases from BrainGlobe and re-package them as Asset Bundles. The Addressables in the Unity project can then be built and deployed on a remote server for use by other projects.

The [AddressablesRemoteLoader](https://github.com/dbirman/vbl-core/blob/main/Scripts/Addressables/AddressablesRemoteLoader.cs) class found in the vbl-core repository provides access in client code to the Addressables bundles. (todo: re-name "BrainAtlasLoader")
