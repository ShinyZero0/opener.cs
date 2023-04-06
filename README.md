# jajaro.cs

Rewrite of [jaro](https://github.com/isamert/jaro) in C# witj JSON configuration.
Program allows to define the default programs for MimeTypes by regex. 
See the example config, which should be placed in ~/.config/jajaro.json.
It fallbacks to the first defined common rules so you can e.g. set neovim to open all files which can't be opened by image viewers or atool.
This way you don't need to explicitly define it for every `application/something`.

Also it can launch the defined terminal if you are not already there if you set the `Term` to `true`

