using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Linq;
using System.Threading;
//using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace AntennaCompare
{
    public partial class Form1 : Form
    {
        readonly string debugFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\SignalCompare";
        string call1;
        string call2;
        string mode;
        string band;
        int maxTimeDiff = 10;
        int txCycles = 1;
        float periodHrs;
        double minDistKm;
        int azimuth;
        int beamWidth;
        Dictionary<string, List<(int, int, string, string)>> callDict1;
        Dictionary<string, List<(int, int, string, string)>> callDict2;
        bool busy;
        bool httpErr;
        readonly string[] bands = new string[12] { "2m", "6m", "10m", "12m", "15m", "17m", "20m", "30m", "40m", "60m", "80m", "160m" };

        public Form1()
        {
            InitializeComponent();
            string allVer = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            Version v;
            Version.TryParse(allVer, out v);
            Text = $"Antenna Compare v{v.Major}.{v.Minor}";
            checkBox2_CheckedChanged(null, null);
            checkBox1_CheckedChanged(null, null);
            comboBox1.Items.AddRange(bands);
        }

        async void button1_Click(object sender, EventArgs e)
        {
            maxTimeDiff = 10;      //secs

            call1 = textBox6.Text.Trim().ToUpper();
            if (call1 == "" || call1.Length < 3)
            {
                resultText.Text = "Enter 'Call sign 1'";
                return;
            }
            textBox6.Text = call1;

            call2 = textBox1.Text.Trim().ToUpper();
            if (call2 == "" || call2.Length < 3)
            {
                resultText.Text = "Enter 'Call sign 2'";
                return;
            }
            textBox1.Text = call2;

            if (call1 == call2)
            {
                resultText.Text = "Call signs must be different";
                return;
            }

            if (radioButton3.Checked)
            {
                mode = "FT8";
            }
            else
            {
                mode = "FT4";
            }

            if (checkBox1.Checked)
            {
                int maxDist = radioButton1.Checked ? 12500 : 20000;
                if (!Int32.TryParse(textBox2.Text.Trim(), out int minDist) || minDist < 0 || minDist > maxDist)
                {
                    resultText.Text = $"Enter a number for DX 'Minimum Distance' between 0 and {maxDist}";
                    return;
                }
                if (radioButton1.Checked)
                {
                    minDistKm = minDist * 1.60934;
                }
                else
                {
                    minDistKm = minDist;
                }
            }
            else
            {
                minDistKm = 0;
            }

            if (checkBox2.Checked)
            {
                if (!Int32.TryParse(textBox3.Text.Trim(), out azimuth) || azimuth < 0 || azimuth > 359)
                {
                    resultText.Text = "Enter a number between 0 and 359 for 'Azimuth'";
                    return;
                }
                if (!Int32.TryParse(textBox4.Text.Trim(), out beamWidth) || beamWidth < 0 || beamWidth > 360)
                {
                    resultText.Text = "Enter a number between 0 and 360 for 'Width'";
                    return;
                }
            }
            else
            {
                beamWidth = 360;
            }

            var i = comboBox1.SelectedIndex;
            if (i < 0)
            {
                resultText.Text = "Select a Band";
                return;
            }
            band = bands[i];

            if (!Single.TryParse(textBox5.Text.Trim(), out periodHrs) || periodHrs < 0 || periodHrs > 24)
            {
                resultText.Text = "Enter a number of hours between 0.0 and 24";
                return;
            }

            if (oneButton.Checked)
            {
                if (!Int32.TryParse(textBox7.Text.Trim(), out txCycles) || txCycles < 0 || txCycles > 10)
                {
                    resultText.Text = "Enter a number of Tx cycles between 1 and 10";
                    return;
                }
                int cycleTime = radioButton3.Checked ? 30 : 15;
                maxTimeDiff = (txCycles * (2 *cycleTime)) + 10;      //secs
            }

            listBox1.Items.Clear();

            int minAz = azimuth - (beamWidth / 2);
            if (minAz < 0) minAz += 360;
            int maxAz = azimuth + (beamWidth / 2);
            if (maxAz > 359) maxAz -= 360;
#if DEBUG
            if (!Directory.Exists(debugFilePath)) Directory.CreateDirectory(debugFilePath);
#endif
            callDict1 = new Dictionary<string, List<(int, int, string, string)>>();
            callDict2 = new Dictionary<string, List<(int, int, string, string)>>();

            button1.Enabled = false;
            resultText.Text = "";

            try
            {
                busy = true;
                httpErr = false;

                await Task.Run(() => GetReceptionReports(call1, callDict1));

                while (busy)
                {
                    Thread.Sleep(500);
                }

                if (!httpErr)
                {
                    if (callDict1.Count == 0)
                    {
                        resultText.Text = $"No spots found for {call1}";
                        return;
                    }

                    resultText.Text = $"{callDict1.Count} spots found for {call1}";
                    Thread.Sleep(3000);         //keeps PSKReporter happy

                    busy = true;
                    httpErr = false;

                    await Task.Run(() => GetReceptionReports(call2, callDict2));

                    while (busy)
                    {
                        Thread.Sleep(500);
                    }

                    if (!httpErr)
                    {
                        if (callDict2.Count == 0)
                        {
                            resultText.Text = $"No spots found for {call2}";
                            return;
                        }

                        resultText.Text = $"{callDict2.Count} spots found for {call2}";
                        Thread.Sleep(2000);     //let # of spots show

                        float snrAvg = 0;
                        int nAvg = 0;
                        bool match = false;

                        foreach (var entry in callDict1)
                        {
                            var l2 = new List<(int, int, string, string)>();
                            if (callDict2.TryGetValue(entry.Key, out l2))
                            {
                                int timeDiff = int.MaxValue;
                                int snr = int.MaxValue;
                                var tpl2 = (0, 0, "", "");
                                foreach (var tpl1 in entry.Value)
                                {
                                    foreach (var tpl in l2)
                                    {
                                        int t = Math.Abs(tpl1.Item2 - tpl.Item2);
                                        if (t < timeDiff)
                                        {
                                            timeDiff = t;
                                            tpl2 = tpl;
                                            snr = tpl1.Item1 - tpl.Item1;
                                        }
                                    }
                                    if (timeDiff <= maxTimeDiff)
                                    {

                                        var d = MaidenheadLocator.Distance(tpl2.Item4, tpl2.Item3);      //km
                                        var a = MaidenheadLocator.Azimuth(tpl2.Item4, tpl2.Item3);

                                        int bMin = minAz;
                                        var ta = a;
                                        if (minAz > maxAz)
                                        {
                                            if (a > minAz) ta = a - 360;
                                            bMin = minAz - 360;
                                        }

                                        if (d >= minDistKm && ((ta >= bMin && ta <= maxAz) || beamWidth >= 359))
                                        {
                                            if (radioButton1.Checked)
                                            {
                                                d /= 1.60934;
                                            }
                                            listBox1.Items.Add($"{entry.Key.PadRight(10, ' ')}{snr.ToString().PadLeft(6, ' ')}{d.ToString("F0").PadLeft(7, ' ')}{a.ToString("F0").PadLeft(5, ' ')}");
                                            snrAvg += snr;
                                            nAvg++;
                                            match = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (match)
                        {
                            snrAvg /= nAvg;
                            if (snrAvg >= 0)
                            {
                                resultText.Text = $"{call1} better than {call2} by {snrAvg.ToString("F2")} db (avg).{System.Environment.NewLine}{nAvg} matching spots processed.";
                            }
                            else
                            {
                                resultText.Text = $"{call2} better than {call1} by {(Math.Abs(snrAvg)).ToString("F2")} db (avg).{System.Environment.NewLine}{nAvg} matching spots processed.";
                            }
                        }
                        else
                        {
                            resultText.Text = "No matching spots found";
                        }
                    }
                }
            }
            finally
            {
                button1.Enabled = true;
            }
        }

        async Task GetReceptionReports(string call, Dictionary<string, List<(int, int, string, string)>> callDict)
        {
            using (HttpClient client = new HttpClient())
            {
                int lineCount = 0;
                string line = "";
                List<string> lines = new List<string>();
                bool endDetected = false;

                try
                {
                    resultText.Invoke((MethodInvoker)delegate {resultText.Text = $"Querying {call}...";});
                    var period = (periodHrs * -3600).ToString("F0");
                    client.DefaultRequestHeaders.ConnectionClose = true;
                    client.Timeout = new TimeSpan(0, 0, 10);
                    using (HttpResponseMessage response = await client.GetAsync($"https://retrieve.pskreporter.info/query?senderCallsign={call}&flowStartSeconds={period}", HttpCompletionOption.ResponseContentRead))
                    {
                        response.Version = new Version("1.0");
                        using (HttpContent content = response.Content)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                resultText.Invoke((MethodInvoker)delegate {resultText.Text = $"{call} spots found";});
                                byte[] ba = await content.ReadAsByteArrayAsync();
                                string str = Encoding.UTF8.GetString(ba, 0, ba.Length);
                                StringReader sr = new StringReader(str);

                                while ((line = sr.ReadLine()) != null)
                                {
                                    lines.Add(line);
                                    //if (lineCount == 0 && !line.Contains("xml version"))
                                    lineCount++;

                                    var sender = GetString("senderCallsign", line);
                                    if (sender == call)
                                    {
                                        var recvr = GetString("receiverCallsign", line);
                                        var snr = GetString("sNR", line);
                                        var sec = GetString("flowStartSeconds", line);
                                        var f = GetString("frequency", line);
                                        var b = "unk";
                                        if (f != null)
                                        {
                                            double freq = Convert.ToDouble(f);
                                            b = FreqToBand(freq / 1e6);
                                        }
                                        var m = GetString("mode", line);
                                        var rl = GetString("receiverLocator", line);
                                        var sl = GetString("senderLocator", line);
                                        if (recvr != null && snr != null && sec != null && rl != null && rl.Length >= 4 && sl != null && sl.Length >= 4 && m == mode && b == band)
                                        {
                                            var s = Convert.ToInt32(snr);
                                            var t = Convert.ToInt32(sec);
                                            var l = new List<(int, int, string, string)>();
                                            rl = rl.Substring(0, 4);
                                            sl = sl.Substring(0, 4);

                                            if (!callDict.ContainsKey(recvr))
                                            {
                                                l.Add((s, t, rl, sl));
                                                callDict.Add(recvr, l);
                                            }
                                            else
                                            {
                                                callDict.TryGetValue(recvr, out l);
                                                l.Add((s, t, rl, sl));
                                            }
                                        }
                                    }

                                    if (line.Contains("</receptionReports>"))
                                    {
                                        endDetected = true;
                                        break;
                                    }
                                }
#if DEBUG
                                var fname = call.Replace('/', '_');
                                var debugFilePathNameExt = $"{debugFilePath}\\{fname}.txt";
                                File.WriteAllLines(debugFilePathNameExt, lines);
#endif
                                if (!endDetected)
                                {
                                    httpErr = true;
                                    resultText.Invoke((MethodInvoker)delegate {resultText.Text = "Unexpected response format, try again";});
                                }
                            }
                            else
                            {
                                httpErr = true;
                                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                                {
                                    resultText.Invoke((MethodInvoker)delegate {resultText.Text = $"Too many requests to PSKReporter.{System.Environment.NewLine}Wait and try again."; });
                                }
                                else
                                {
                                    resultText.Invoke((MethodInvoker)delegate {resultText.Text = $"Query failure, status code: {response.StatusCode}";});
                                }
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    httpErr = true;
                    resultText.Invoke((MethodInvoker)delegate {resultText.Text = $"Query failure: {err.ToString()}";});
                }

                busy = false;
            }
        }

        string GetString(string name, string line)
        {
            //asd name="WM8Q/P" zx
            //01234567890123456789
            //    i     ---6--j

            var s = $"{name}=\"";
            var l = s.Length;
            int i = line.IndexOf(s);
            if (i < 0) return null;
            int j = line.IndexOf("\"", i + l + 1);
            if (j < 0) return null;
            var ret = line.Substring(i + l, j - i - l);
            return ret;
        }

        string FreqToBand(double freq)
        {
            if (freq >= 0.1357 && freq <= 0.1378) return "2200m";
            if (freq >= 0.472 && freq <= 0.479) return "630m";
            if (freq >= 1.8 && freq <= 2.0) return "160m";
            if (freq >= 3.5 && freq <= 4.0) return "80m";
            if (freq >= 5.35 && freq <= 5.37) return "60m";
            if (freq >= 7.0 && freq <= 7.3) return "40m";
            if (freq >= 10.1 && freq <= 10.15) return "30m";
            if (freq >= 14.0 && freq <= 14.35) return "20m";
            if (freq >= 18.068 && freq <= 18.168) return "17m";
            if (freq >= 21.0 && freq <= 21.45) return "15m";
            if (freq >= 24.89 && freq <= 24.99) return "12m";
            if (freq >= 28.0 && freq <= 29.7) return "10m";
            if (freq >= 50.0 && freq <= 54.0) return "6m";
            if (freq >= 144.0 && freq <= 148.0) return "2m";
            return "";
        }

        private void radioButton3_Click(object sender, EventArgs e)
        {
            radioButton3.Checked = true;
            radioButton4.Checked = false;
        }

        private void radioButton4_Click(object sender, EventArgs e)
        {
            radioButton3.Checked = false;
            radioButton4.Checked = true;
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton2.Checked = false;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            label5.Enabled = textBox2.Enabled = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            label6.Enabled = textBox3.Enabled = label7.Enabled = textBox4.Enabled = checkBox2.Checked;
        }

        private void twoButton_Click(object sender, EventArgs e)
        {
            oneButton.Checked = false;
        }

        private void oneButton_Click(object sender, EventArgs e)
        {
            twoButton.Checked = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            twoButton.Checked = true;
            textBox7.Text = "1";
        }

        private void twoButton_CheckedChanged(object sender, EventArgs e)
        {
            label13.Enabled = textBox7.Enabled = label14.Enabled = oneButton.Checked;
        }

        private void oneButton_CheckedChanged(object sender, EventArgs e)
        {
            label13.Enabled = textBox7.Enabled = label14.Enabled = oneButton.Checked;
        }
    }
}
