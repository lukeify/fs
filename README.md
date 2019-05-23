# fs

![David (path)](https://img.shields.io/david/lukeify/fs.svg?path=client)
[![GitHub license](https://img.shields.io/github/license/lukeify/fs.svg)](https://github.com/lukeify/fs/blob/master/LICENSE)[![Twitter URL](https://img.shields.io/twitter/url/http/shields.io.svg?style=social)](https://fs.lukeify.com)

![](assets/hero.png)

Personal arbitrary file uploading & hosting on a .NET Core server, with an administration interface written in Vue. Files served statically, with a RethinkDb backing store containing file metadata.

## User Instructions

Once running upload any number of files to the server by clicking or dragging files into the dropzone. They will be automatically uploaded, and once complete, will appear in the files array beneath the dropzone. Hover over a file and click the copy icon to copy a direct link to the file to your clipboard.

## Getting Started

The following guides provides instructions on how to run `fs` both for development, and in production behind a nginx reverse proxy, on Ubuntu 18.04 LTS.

### Prerequisites

You will need `yarn` installed, along with `dotnet` in both your local development environment & in production. Yarn can be installed by following instructions from [https://yarnpkg.com/en/docs/install](https://yarnpkg.com/en/docs/install). To install dotnet, follow Microsoft's instructions at [https://dotnet.microsoft.com/download/linux-package-manager/ubuntu18-04/sdk-current](https://dotnet.microsoft.com/download/linux-package-manager/ubuntu18-04/sdk-current).

You will also need to install `rethinkdb`. Note that `rethinkdb` is only available as debian package up to Ubuntu 18.04, and only as a downloadable debian package on GitHub. To install rethinkdb, visit [https://github.com/srh/rethinkdb/releases/tag/v2.3.6.srh.1](https://github.com/srh/rethinkdb/releases/tag/v2.3.6.srh.1) and copy the appropriate `.deb` link, then from your machine:

```bash
wget https://github.com/srh/rethinkdb/releases/download/v2.3.6.srh.1/rethinkdb_2.3.6.srh.1.0bionic_amd64.deb
sudo dpkg -i rethinkdb_2.3.6.srh.1.0bionic_amd64.deb
```

In production, I recommend `pm2` to run both `rethinkdb` & the main server application. To install `pm2`:

```bash
yarn global add pm2
```

### Installation

```bash
git clone https://github.com/lukeify/fs
cd fs
```

### Building

To run the database locally:

```bash
rethinkdb
```

Then, to initialise the client development server:

```bash
cd client
yarn run serve
```

This will start the client in development mode at `http://localhost:8080`. Before you can run the server, check your `./appsettings.json` and `./appsettings.Development.json` files contain the appropriate configuration. We must also add a user secret to represent the key used to login.

```bash
dotnet user-secret set Filesystem:Key=yourkeyhere
```

To run the server:

```bash
dotnet run
```

This will initialise the dotnet server on port 5000. You can then access the functional application at `http://localhost:8080`.

## Deployment

To build the client for production. This will create an output in the `./wwwroot` directory of the application.

```bash
cd client
yarn install
yarn run build
```

To begin the server setup, validate your production configuration of the application by editing `./appsettings.json` and `./appsettings.Production.json` to your desired values. Build for production:

```bash
dotnet publish -c Release
```

Finally, launch both the database and server:

```bash
pm2 start rethinkdb
pm2 start dotnet -- run bin/Release/netcoreapp21/fs.dll --launch-profile Production Filesystem:Key=yourkeyhere
```

Configure `nginx` as a reverse proxy:

```bash
cp fs.lukeify.com.conf /etc/nginx/sites-available
cd /etc/nginx/sites-available
ln -s fs.lukeify.com.conf /etc/nginx/sites-enabled
sudo nginx -t
sudo systemctl restart nginx
```

## Built With

* Yarn (Package Manager)
* Vue (Client)
* TypeScript (Client lang)
* .NET Core 2.1 (Server)
* C# (Server lang)
* RethinkDB (Database)
* Nginx (Reverse Proxy)
* pm2 (Process Manager)

## Versioning

This site adheres to [semantic versioning](https://semver.org).

## Author

Luke Davia.

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

This repository is licensed under the [MIT License](LICENSE). The name *lukeify*, and any content posted on fs.lukeigy.com are property of the author. For more on this license, [read the summary on tldrlegal.com](https://tldrlegal.com/license/mit-license).