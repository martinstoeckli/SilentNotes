# VanillaCloudStorageClient

The VanillaCloudStorageClient project allows to store files on cloud storage servers with OAuth2
authorization, like Dropbox and Google-Drive, but also supports other protocols like FTP and WebDav.
It is meant for installable app clients which want to access those storage providers.

The library is very light-weight with a minimum of dependencies. It is written in pure Dotnet and
is a cross-platform DotnetStandard 2.0 project.

UnitTests are available for all cloud storage providers, they can be switched to integration tests
with the "DoRealWebRequests" flag, to make real HTTP requests to the storage provider.

## Credits

* The clean and minimalistic [Flurl](https://flurl.dev/) library helped a lot to keep the HTTP requests readable.
