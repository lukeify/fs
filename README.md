# Fs

Personal arbitrary file uploading & hosting on a .NET Core server, with an administration interface written in Vue. Files served statically, with a RethinkDb backing store containing file metadata.

## User Instructions

Once running upload any number of files to the server by clicking or dragging files into the dropzine. They will be automatically uploaded, and once complete, will appear in the files array beneath the dropzone. Hover over a file and click the copy icon to copy a direct link to the file to your clipboard.

## Getting Started

### Prerequisites

### Installation

### Building

## Tests

## Deployment

## Built With

## Contributing

## Versioning

## Author

## Notes

### Todos

Convert File to ontology with appropriate methods branching from each Model (Image, Video, etc).

### Conventions for variable naming

This application makes extensive use of filesystem-related terminology. For clarity sake, the following suffixes shall be applied to variables to indicate their meaning.

1. `*Dir` shall refer to a directory path, without a stem file.
2. `*Path` shall refer to a combination of directory path & file, that results in a file name & extension.
3. `*Name` shall refer to a stem file name & extension, without an attached directory.
4. `*Extension` shall refer to just a file extension string.
5. `*NameWithoutExtension` shall refer to a stem file name, without an extension.

## License