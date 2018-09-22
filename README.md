<div class="container text-center">
    <h1 class="text-center"> 90s Video</h1>
    <h3><i>Application to view 90's popular songs by combining Internet Radio Streaming and YouTube</i></h3>
</div>


<div>
    <br />
    <p>
        First I'd like to express my gratitude to:
        <ul>
            <li><strong><a href="http://www.90s90s.de" target="_blank">www.90s90s.de</a></strong> - internet radio for a very great selection of songs.</li>
            <li>
                Alexey "Tyrrrz" Golub for developing and sharing his work - <strong><a href="https://github.com/Tyrrrz/YoutubeExplode" target="_blank">YoutubeExplode</a></strong>
                - library that allows the downloading of videos from YouTube.
            </li>
        </ul>
    </p>
    <p>
        The original concept of my application was to be able to play a video clip of the song that is being played on <a href="http://www.90s90s.de" target="_blank">www.90s90s.de</a>
        radio as an embedded video. A video should start as soon as the application launched and continue into the next in queue. There should be a list of videos that
        can be re-played and paused on demand.
        <br />
        After trying to implement <a href="https://developers.google.com/youtube/iframe_api_reference" target="_blank">YouTube IFrame player</a> and finding that many video clips on
        YouTube are restricted to be played as embedded and the only way to view them is through
        a browser on the YouTube site. I've decided to find a way to download video clips to the computer. Fortunately, "the Wheel" has already been already invented in the form of
        <a href="https://github.com/Tyrrrz/YoutubeExplode" target="_blank">YoutubeExplode</a>.
        <br />
        Downloading video approach would require to display of download progress and handling files/list entries in case the space on the computer is limited.
    </p>
</div>

<div>
    <div class="col-md-8">
    <h3>Screenshot</h3>
        <img src="https://github.com/MaxiGen78/90sVideo/blob/master/90s-2.jpg" />
    </div>

<div>
    <h3>Download</h3>
        <ul>
            <li>
             <strong><a href="https://romaxsolutions.net/Download/90s.msi">Download</a></strong> v1.0.0.2 (requires <a href="https://www.microsoft.com/en-gb/download/details.aspx?id=49981" target="_blank">Microsoft .NET Framework 4.6.1</a> and 
                     <a href="https://www.codecguide.com/download_k-lite_codec_pack_basic.htm" target="_blank">K-Lite Codec</a> to play webm extensions)</li> 

</div>


<h3>Features</h3>
<ul>
            <li>Plays video clips that correspond to the songs played on the internet radio</li>
            <li>Fallbacks to a different channel if the next song is unavailable (radio channel news or difference in media playtime length)</li>
            <li>Downloads video clips to the computer, showing progress percentage</li>
            <li>Implements pause and resume of a currently played video</li>
            <li>Implements mute and sound volume of the currently played song</li>
            <li>Shows current folder size usage by downloaded videos (serves as a cache)</li>
            <li>Setting download folder path and the folder's size limit (mouse right-click on the information panel at the bottom right hand corner) </li>
            <li>Panels autohide (uncheck corresponding tickbox)</li>
            <li>Toggle fullscreen on mouse double-click</li>
            <li>Persisting user settings</li>
            <li> <i>To be added in future: replacing a downloaded video with another more suitable through the list interface and persisting changes.</i></li>
</ul>


<h3>Instructions and special notes</h3>
        <ul>
            <li>Please check if Microsoft .NET Framework 4.6.1 or higher is installed (<a href="https://www.microsoft.com/en-gb/download/details.aspx?id=49981" target="_blank">Download Microsoft .NET Framework 4.6.1)</a></li>
            <li>
                If the application just shows a black screen - check a codec that can play ".webm" extensions is installed
                (you can download basic K-LIte Codec <a href="https://www.codecguide.com/download_k-lite_codec_pack_basic.htm" target="_blank">here</a>)
            </li>
            <li>
                To change download folder or set its size please right click on the information panel at the bottom right hand corner. It is strongly advised to select an empty folder initially
                and an adequate folder size (recommended 5+ GB).
            </li>
            <li>If the folder size limit is exceeded, the application will attempt to delete the oldest files within the folder until there is enough space.</li>
            <li>Please note this application is made for my own entertainment purposes only.</li>
            <li>The original content (music, videos and radio streams) belong to their rightful owners.</li>
            <li>Use this software at your own risk. I have no liability for any loss or damage suffered as a result of the use, misuse or reliance on the information and content on this website.</li>

 

