pub:
	rm -rf out/
	dotnet publish -c Release -o out/

aot:
	rm -rf out/
	dotnet publish -c Release -o out/ -p:PublishSingleFile=false -p:PublishAot=true
