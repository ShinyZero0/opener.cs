# opener.cs

File opener in C# witj JSON configuration.
Program allows to define the default programs for MimeTypes by regex. 
See the example config, which should be placed in ~/.config/jajaro.json.
It checks filename matches from down to top and from regex to glob so you can e.g. set neovim at top for any files to open all files which can't be opened by the rest of handlers.
This way you don't need to explicitly define it for every possible text or code filetype.
The example config file can be found [there](stuff/associations.json)

Also it can launch the defined terminal if you are not already there if you set the `Term` to `true`
TODO:
- [x] add globs

