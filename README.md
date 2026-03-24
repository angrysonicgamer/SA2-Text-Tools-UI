# SA2 Text Tools UI

Upgraded version of my [SA2 Text Tools](https://github.com/angrysonicgamer/SA2-Text-Tools) with simple UI written in C#/WPF. Basically an alternative to SA Tools but for personal use, at least for now.

## Usage
The tools are meant to work with Sonic Adventure 2 event text and message text files. You can also import and export data in JSON files but note that JSON's exported from the console variant are not fully compatible.

The tools support Windows-1251 (Cyrillic), Windows-1252 (Latin) and Shift-JIS (Japanese) encodings and also allow setting a custom codepage. Both little endian (used in Dreamcast version files) and big endian (files from SA2B port) byte orders are supported, with an option to auto-detect endianness.

Both event and message file editors support 3 languages (English, Russian and Japanese) that can be changed on the fly.
