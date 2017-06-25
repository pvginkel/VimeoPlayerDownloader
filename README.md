# Instructions

* Download the "VimeoPlayerDownload.v1.zip" file from the Releases tab above and extract it into a local folder;
* Go to the web page that's showing the Vimeo player and view the source (Ctrl+U in Chrome);
* Find the URL to the Vimeo player:
  * Click Ctrl+F;
  * Type "player.vimeo";
  * Copy the URL until the first question mark or double quotes;
* Open a command prompt and browse to the directory you've extracted the executable to;
* Type the following:
  * `VimeoPlayerDownload.exe <player url> <file name>`, e.g.:
  * `VimeoPlayerDownload.exe "https://player.vimeo.com/video/000000000" "Movie.mp4"`;
* Type Enter.

If the audio and video are split, two files will be created. In the example above, the
audio stream would be named "Movie-audio.mp4". You then still need a way to merge the
two streams back. This can be done e.g. with `ffmpeg`:

    ffmpeg -i Movie.mp4 -i Movie-audio.mp4 -c:v copy -c:a copy Movie-merged.mp4
