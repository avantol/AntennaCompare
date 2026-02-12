# AntennaCompare
Compare efficiency of two HF (or VHF) antennas transmitting FT8 simultaneously* on practically the same frequency, then automatically analyze the PSKReporter SNR report data.
<br><br>AntennaCompare analyzes the <i>simultaneous</i> signal reports* from around the world, reducing the propagation effects to those that are caused by the antennas, making signal report comparisons meaningful. It's available as a <a href="https://github.com/avantol/AntennaCompare/releases/latest">Windows desktop app</a> or a <a href="https://antenna-compare-web.vercel.app/">web app</a>.
<br><br>Why use FT8 and not WSPR? Simple, I'm not seeing nearly as many DX stations reporting WSPR as FT8... that makes AntennaCompare perfect for optimizing your new DX creation. You'll find out if your new antenna is more effective than your old antenna... in dB, to several decimal places.
<br><br>You get VERY useful data (<i>much</i> better than the typical YouTubers saying <i>"It must be good cuz I got some QSOs"</i>), so leave the old antenna up until you can run this comparison... THEN take down the loser after the shootout.
<br><br><b>Easy to use:</b> simply run FT8 (using WSJT-X or similar) on two transmitters with different call signs, with one transmitter connected to a reference antenna and the other transmitter connected to the antenna under test, at equal power.
<br>(*There is an option for using only one transmitter and an antenna switch, not optimal but still usable).
<br><br>For the two call signs: Use your call sign, and create a variation on your call sign by adding a suffix, like /1 or /P.
<br><br>Your grid identifier is required in the FT8 messages, otherwise PSKReporter won't report your signals.
<br>Set up each transmitter's message to reply to the other, which tends to avoid attracting replies from other stations, for example:
<br>For antenna #1: &nbsp;&nbsp; WM8Q/P <b>WM8Q</b> DN61
<br>For antenna #2: &nbsp;&nbsp; WM8Q <b>WM8Q/P</b> DN61
<br><br>(a) If using two transmitters: Transmit with both <b>on the same time slot</b>.
<br><br>(a) If using one transmitter: Transmit for 4 consecutive cycles using one antenna, switch to the other antenna, and transmit for 4 consecutive cycles.
<br><br>Repeat (a) or (b) until PSKReporter shows a significant number of spots, then select 'Compare'.
<br>(PSKReporter has a query rate limit, so if 'Compare' fails, simply wait a few minutes and try again)
<br><br>
<a href="https://github.com/avantol/AntennaCompare/releases/latest">
<img src="https://github.com/avantol/AntennaCompare/blob/main/AntennaCompare.JPG"></a>
...........
<a href="https://antenna-compare-web.vercel.app/">
<img src="https://github.com/avantol/AntennaCompare/blob/main/AntennaCompare-web.jpg"></a>
