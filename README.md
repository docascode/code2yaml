# code2yaml
A tool to extract metadata from code and export as yaml files

## configure code2yaml
To use the tool, you need to provide a config file `code2yaml.json`.

Here is a simple `code2yaml.json`.

```json
{
  "input_path": "./azure-sdk-for-java",
  "output_path": "./output",
  "exclude_paths": ["./azure-sdk-for-java/azure-samples"],
  "language": "java"
}
```

* `input_paths`: an array of input paths.
* `output_path`: output path
* `exclude_paths`: an array of exclude paths. Code in the paths wouldn't be extracted metadata.
* `language`: it now supports `cplusplus`, `java`.

> *Note*

> all the paths(path in `input_paths`, `exclude_paths` or `output_path`) are either absolute path or path relative to code2yaml.json

The above sample indicates that we need `code2yaml` to extract metadata from all code under `azure-sdk-for-java` and save the results(metadata files which end with `.yml`) into folder `output`, except for code under folder `azure-sdk-for-java/azure-samples`.
The folder structure is like below:

```
Folder  
|   code2yaml.json   
|
+---azure-sdk-for-java
|   ...
|   +---azure-samples
|
|---output
```
## run code2yaml
1. build the solution.
   open cmd shell. `build.cmd`
2. `code2yaml.exe code2yaml.json`

