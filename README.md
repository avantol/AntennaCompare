# AntennaCompare
Compare efficiency of two HF (or VHF) antennas transmitting FT8 simultaneously on practically the same frequency, then automatically analyze the PSKReporter SNR report data.
<br><br>AntennaCompare analyzes the <i>simultaneous</i> signal reports from around the world, reducing the propagation effects to those that are caused by the antennas, making signal report comparisons meaningful.
<br><br>Why use FT8 and not WSPR? Simple, I'm not seeing nearly as many DX stations reporting WSPR as FT8... that makes AntennaCompare perfect for optimizing your new DX creation. You'll find out if your new antenna is more effective than your old antenna... in dB, to several decimal places.
<br><br>You get VERY useful data (better than YouTubers saying "It must be good cuz I got some QSOs"), so leave the old antenna up until you can run this comparison... THEN take down the loser after the shootout.
<br><br>Easy to use: simply run FT8 (using WSJT-X or similar) on two transmitters with different call signs, with one transmitter connected to a reference antenna and the other transmitter connected to the antenna under test, at equal power.
<br><br>For call signs, use your call sign, and a variation on your call sign by adding a suffix, like /1 or /P.
<br><br>Your grid identifier is required in the messages, otherwise PSKReporter won't report your signals.
<br><br>Set up each transmitter's message to "reply" to the other, which tends to avoid attracting replies from other stations, for example:
<br>For antenna #1: WM8Q WM8Q/P DN61
<br>For antenna #2: WM8Q/P WM8Q DN61
<br><br>Transmit until PSKReported shows a significant number of spots, then select 'Compare'.
<br>PSKReporter has a query rate limit, so if 'Compare' fails, simply waiy a few minutes and try again.
<br><br><img src="https://github.com/avantol/AntennaCompare/blob/main/AntennaCompare.JPG">
