# Build Java Source Code from Maven package
**IMPORTANT** This feature is still under development.

## Introduction
This tool supports generating document from Java source code before, but it has some drawbacks:
1. Test code should be excluded explicitly in configuration. Document of test cases makes no sense for end users.
2. Publishing from latest source code may mismatch with already published package.

In Java world, packages are usually published using [Maven](https://maven.apache.org/index.html), to [Maven repository](https://search.maven.org/#search%7Cga%7C1%7C). To address the above problems, code2yaml also supports generating YAMLs from `.jar` Maven package directly.

Actually the Maven **source** package is a zip file containing all `.java` source codes.

## Usage
```json
{
  "input_path_maven": [ "./azure-batch-2.1.0-sources.jar" ],
  "output_path": "./output",
  "language": "java"
}
```
When configured, this tool will build all `.java` source code in this `.jar` file. This jar file can be download by clicking `sources.jar` in Maven repository.

Note that the key `input_path` can also exist when `input_path_maven` exists. All the referenced source code will be built. However, please ensure not to include the same source code twice in both `input_path` and `input_path_maven`.

# How to link to source code?
For each Maven package, we assume it correspond to a GitHub repo with a specified tag. As pure Java source code in `.jar` doesn't contain git information, additional information is needed.

In Maven package, `pom.xml` is the main configuration file. As it contains the git information and already exists along with a Maven package, it's an ideal source of git information to utilize.

To make the git information appear in YAMLs, the fowllowing steps is required:
1. Also download `pom.xml` by saving link `pom` in Maven repository. Rename this `pom.xml` to **the same name** as source pacage, e.g. `azure-batch-2.1.0-sources.xml`. Put it along with `azure-batch-2.1.0-sources.jar`
2. Make sure the following keys exist in `pom.xml`: 
    * `/scm/connection`,
    * `/scm/tag`
  The definition of these keys can be found [here](https://maven.apache.org/pom.html#SCM).

With this side by side `.xml`, code2yaml will try matching the source code in `.jar` with files in git repo. Git information will be generated if there's a sucessful match. Otherwise, no git information will be generated even the `pom.xml` is provided.

# What if the required keys is missing in `pom.xml`
A modified or even fake `pom.xml` is also acceptable as long as the required keys have correct value.

# What if there's no tag added in git repo corresponding to a specified Maven package?
A even dirty workaround for this situation is to fill a commit id into `/scm/tag`. It can also serve as a snapshot of the repo, so that the generated git information in YAML can point to this commit id.
It's still highly recommended to add a tag to repo when a Maven package is released.