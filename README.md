# opener.cs

File opener in C# witj JSON configuration.
Program allows to define the default programs for MimeTypes by regex. 
See the example config, which should be placed in ~/.config/jajaro.json.
It sets priority from down to top so you can e.g. set neovim at top for any files to open all files which can't be opened by image viewers or atool.
This way you don't need to explicitly define it for every possible text or code filetype.

Also it can launch the defined terminal if you are not already there if you set the `Term` to `true`
TODO:
- [x] add globs

