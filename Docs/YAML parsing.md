YAML parsing
==============

Documents are indicated by 3 minus signs: ---
Ampersand indicates a variable definition. the variable can then be referred to by an asterisk. Primitives are mapping, sequence, and scalar.

###Unity Yaml format

- defined [here](http://docs.unity3d.com/Manual/FormatDescription.html)
- seems to be propriatory. parsers throw all tag definitions away when starting a new document. Unity yaml keeps referring to the !u! tag.
- some more [findings](https://stackoverflow.com/questions/21473076/pyyaml-and-unusual-tags)
- document anchors are class ids, [here](http://docs.unity3d.com/Manual/ClassIDReference.html) is the table
- fileIds are generated [like this](http://forum.unity3d.com/threads/yaml-fileid-hash-function-for-dll-scripts.252075/#post-1695479)

Any {fileID: 809006537} is an Object reference. If there is only a fileID in the mapping, then it's a local reference, the target node has an anchor with the matching fileID. If the mapping contains a guid, then it's an external file!

###YamlDotNet

- yamlStream.Load populates yamlStream.Documents
- each document contains a node tree
- to fix Unity's tag issue, I made a change that defines the !u! tag globally

###Resolving object references

The function should map an enumeration of fileId/guid pairs and a scene yaml string to an enumeration of ReferencePaths. Path entries should be name/class pairs, like Intro/Scene -> Controller/GameObject -> Camera/Camera

Does the scene yaml contain the scene name? no it does not. so add it to the returned paths.
How do we get the guid and fileId of an object?

