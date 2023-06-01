## ChatGPT Windows
This tool uses the pawan.krd reverse proxy, you will need an api key from their [discord](https://discord.gg/pawan-krd-1055397662976905229)


## Usage:
Now that you have your api key and builded the app (in release), you will need to create 2 new files in the release build directory:

api_key.txt (containing your api key)<br>
username.txt (how the ai should call you)


Now you can open the GPT.exe file and toggle the chat window with Windows Key + F12, modify the source code if this keybind is already in use by other apps

## Known Issues:
Can't prompt with multi-line text, pasting multi-line text doesnt work either, it will only paste the first line.
Sometimes crashes when getting a response, this will be fixed soon.

## Todo:
Use a .env file for options instead of 2 text files
Add support for multi-line text
