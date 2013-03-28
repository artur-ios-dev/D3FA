using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using gma.System.Windows;
using System.Runtime.InteropServices;


namespace D3FA
{
    public partial class Main : Form
    {
        int runTimer = 0;
        int totalRunTimer = 0;
        int legendariesCounter = 0;
        Double totalXPEarned = 0;
        Double totalGoldEarrned = 0;
        int numberOfRuns = 0;
        int elitesCounter = 0;
        int demonicsCounter = 0;
        int lastRunEss = 0;

        UserActivityHook actHook;
        Boolean isShiftDown;
        Boolean isCtrlDown;
        Boolean isAltDown;

        private static int xpToNextLevel(int level)
        {
            if (level == 0)
                return 7200000;
            if (level <= 59)
                return xpToNextLevel(level - 1) + 1440000;
            if (level <= 69)
                return xpToNextLevel(level - 1) + 2880000;
            if (level <= 79)
                return xpToNextLevel(level - 1) + 5040000;
            if (level <= 89)
                return xpToNextLevel(level - 1) + 6480000;
            if (level <= 99)
                return xpToNextLevel(level - 1) + 8640000;
            else
                return 0;
        }

        public Main()
        {
            InitializeComponent();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            comboBoxStatistics.Text = "All";

            isShiftDown = false;
            isCtrlDown = false;
            isAltDown = false;

            //Loading settings
            menuBoxOnTop.Checked = Properties.Settings.Default.OnTop;
            menuBoxBorderless.Checked = Properties.Settings.Default.Borderless;
            menuBoxOpacity.Checked = Properties.Settings.Default.Opacity;
            menuBoxLegendaries.Checked = Properties.Settings.Default.Legendaries;
            menuBoxGold.Checked = Properties.Settings.Default.Gold;
            menuBoxExp.Checked = Properties.Settings.Default.Experience;
            textBoxStartXP.Text = Properties.Settings.Default.StartXP;
            textBoxStartGold.Text = Properties.Settings.Default.StartGold;
            textBoxFileName.Text = Properties.Settings.Default.Filename;
            textBoxRunName.Text = Properties.Settings.Default.Runname;
            checkBoxSaveToFile.Checked = Properties.Settings.Default.Savetofile;
            checkBoxStatistics.Checked = Properties.Settings.Default.Statistics;
            textBoxParagonLevel.Value = Properties.Settings.Default.Paragon;
            menuBoxDE.Checked = Properties.Settings.Default.DE;

            if (menuBoxGold.Checked || menuBoxLegendaries.Checked || menuBoxExp.Checked || menuBoxDE.Checked)
            {
                panelSave.Visible = true;
            }
        }

        private void buttonMoreOptions_Click(object sender, EventArgs e)
        {
            Point point = new Point(0, buttonMoreOptions.Height);
            point = buttonMoreOptions.PointToScreen(point);
            menuMore.Show(point);
        }

        private void checkBoxSaveToFile_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSaveToFile.Checked)
            {
                panelSaveToFile.Visible = true;
            }
            else
            {
                panelSaveToFile.Visible = false;
            }
        }

        private void checkBoxCompact_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCompact.Checked)
            {
                groupStatistics.Visible = false;
                groupLeveling.Visible = false;
                groupGold.Visible = false;
                panelSave.Visible = false;
                if (checkBoxStatistics.Checked) checkBoxStatistics.Checked = false;
            }
            else
            {
                if (checkBoxStatistics.Checked) groupStatistics.Visible = true;
                groupLeveling.Visible = true;
                if (menuBoxGold.Checked) groupGold.Visible = true;
                if (menuBoxGold.Checked || menuBoxLegendaries.Checked || menuBoxExp.Checked || menuBoxDE.Checked)
                {
                    panelSave.Visible = true;
                }
                else panelSave.Visible = false;

            }
        }

        private void checkBoxStatistics_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxStatistics.Checked)
            {
                groupStatistics.Visible = true;
                if (checkBoxCompact.Checked) checkBoxCompact.Checked = false;
            }
            else
            {
                groupStatistics.Visible = false;
            }
        }

        private void buttonSaveToFile_Click(object sender, EventArgs e)
        {
            if (runTimer > 0)
            {
                numberOfRuns++;
                totalRunTimer += runTimer;

                TimeSpan timeSpan = TimeSpan.FromSeconds(totalRunTimer);

                labelTime1.Text = labelRunTimer.Text; // last run time
                labelTime2.Text = timeSpan.ToString(@"hh\:mm\:ss"); ; //total runs time

                //exp info
                if (menuBoxExp.Checked)
                {
                    int paragonLevel = (int)textBoxParagonLevel.Value;
                    Double startXP;
                    Double endXP;
                    if (Double.TryParse(textBoxStartXP.Text.Replace(',', '.'), out startXP)
                        && Double.TryParse(textBoxEndXP.Text.Replace(',', '.'), out endXP))
                    {

                        if (startXP < 0) startXP = -startXP;
                        if (endXP < 0) endXP = -endXP;

                        Double xpEarnedLastRun = endXP - startXP;
                        if (xpEarnedLastRun < 0 && paragonLevel == 100) MessageBox.Show("End XP can not be smaller than Start XP if you are at 100 paragon level.");
                        else
                        {
                            if (xpEarnedLastRun < 0) //level up
                            {
                                Double xpNextLevel = xpToNextLevel(paragonLevel) / 1000000.0; // millions
                                xpEarnedLastRun += xpNextLevel;
                                totalXPEarned += xpEarnedLastRun;
                                textBoxParagonLevel.Value++;
                            }
                            else // no level up
                            {
                                totalXPEarned += xpEarnedLastRun;
                            }

                            labelXPGained1.Text = String.Format("{0:0.00}", xpEarnedLastRun) + " m"; // xp gained last run
                            labelXPGained2.Text = String.Format("{0:0.00}", totalXPEarned) + " m"; //xp gained session

                            // xp gained last run per second
                            Double xpPerH = xpEarnedLastRun / runTimer;
                            xpPerH *= 60; // per minute
                            labelXPm1.Text = String.Format("{0:0.00}", xpPerH) + " m";
                            xpPerH *= 60; //per hour
                            labelXPh1.Text = String.Format("{0:0.00}", xpPerH) + " m";

                            // xp gained session per second
                            xpPerH = totalXPEarned / totalRunTimer;
                            xpPerH *= 60; // per minute
                            labelXPm2.Text = String.Format("{0:0.00}", xpPerH) + " m";
                            xpPerH *= 60; //per hour
                            labelXPh2.Text = String.Format("{0:0.00}", xpPerH) + " m";
                            xpPerH = totalXPEarned / numberOfRuns; //per run
                            labelXPr2.Text = String.Format("{0:0.00}", xpPerH) + " m";

                            // paragon info
                            Double xpNextParagon = xpToNextLevel(paragonLevel) / 1000000.0;
                            Double xp100Paragon = 0;
                            for (int i = paragonLevel; i < 100; i++) xp100Paragon += (xpToNextLevel(i) / 1000000.0);
                            if (xpNextParagon > endXP)
                            {
                                xpNextParagon -= endXP;
                                xp100Paragon -= endXP;

                                // last run speed
                                xpPerH = xpEarnedLastRun / runTimer; // last run speed xp per second
                                timeSpan = TimeSpan.FromSeconds(xpNextParagon / xpPerH); // how many second to next level
                                labelNextLevel1.Text = timeSpan.ToString(@"hh\:mm\:ss");

                                timeSpan = TimeSpan.FromSeconds(xp100Paragon / xpPerH); // how many second to 100 level
                                label100level1.Text = timeSpan.ToString("%d") + "d " + timeSpan.ToString(@"hh\:mm\:ss");

                                // session speed
                                xpPerH = totalXPEarned / totalRunTimer; // session speed xp per second
                                timeSpan = TimeSpan.FromSeconds(xpNextParagon / xpPerH); // how many second to next level
                                labelNextLevel2.Text = timeSpan.ToString(@"hh\:mm\:ss");

                                timeSpan = TimeSpan.FromSeconds(xp100Paragon / xpPerH); // how many second to 100 level
                                label100level2.Text = timeSpan.ToString("%d") + "d " + timeSpan.ToString(@"hh\:mm\:ss");
                            }

                            textBoxStartXP.Text = textBoxEndXP.Text; // change End XP to Start XP
                            textBoxEndXP.Text = ""; // clear End XP

                            //saving to file
                            if (checkBoxSaveToFile.Checked)
                            {
                                String fileName;
                                if (textBoxFileName.Text != "") fileName = textBoxFileName.Text + ".txt";
                                else fileName = "data.txt";

                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, true))
                                {
                                    String runName;
                                    if (textBoxRunName.Text != "") runName = textBoxRunName.Text;
                                    else runName = "Run " + numberOfRuns;
                                    sw.WriteLine(runName + "|" + runTimer + "|" + xpEarnedLastRun);
                                }
                            }
                        }
                    }
                    else MessageBox.Show("Something is wrong with Start XP and End XP boxes.");
                }
                // gold info
                if (menuBoxGold.Checked)
                {
                    Double startGold;
                    Double endGold;
                    if (Double.TryParse(textBoxStartGold.Text.Replace(',', '.'), out startGold)
                        && Double.TryParse(textBoxEndGold.Text.Replace(',', '.'), out endGold))
                    {
                        Double goldEarnedLastRun = endGold - startGold;
                        totalGoldEarrned += goldEarnedLastRun;

                        labelGoldEarned1.Text = String.Format("{0:0.00}", goldEarnedLastRun) + " m";
                        labelGoldEarned2.Text = String.Format("{0:0.00}", totalGoldEarrned) + " m";

                        // gold gained last run per second
                        Double goldPerH = goldEarnedLastRun / runTimer;
                        goldPerH *= 3600; //per hour
                        labelGoldh1.Text = String.Format("{0:0.00}", goldPerH) + " m";

                        // gold gained session per second
                        goldPerH = totalGoldEarrned / totalRunTimer;
                        goldPerH *= 3600; //per hour
                        labelGoldh2.Text = String.Format("{0:0.00}", goldPerH) + " m";

                        textBoxStartGold.Text = textBoxEndGold.Text; // change End Gold to Start Gold
                        textBoxEndGold.Text = ""; // clear End Gold
                    }
                }

                //demonics
                if (menuBoxDE.Checked)
                {
                    Double essPerH;

                    if (Int32.TryParse(labelDemonicEss1.Text.Replace('-', '0'), out lastRunEss))
                    {
                        lastRunEss = demonicsCounter - lastRunEss;
                        labelDemonicEss1.Text = lastRunEss.ToString();

                        essPerH = (double)lastRunEss / runTimer;
                        essPerH *= 3600;
                        labelDemonich1.Text = String.Format("{0:0.00}", essPerH);

                    }

                    labelDemonicEss2.Text = demonicsCounter.ToString();

                    essPerH = (double)demonicsCounter / totalRunTimer;
                    essPerH *= 3600;
                    labelDemonich2.Text = String.Format("{0:0.00}", essPerH);


                }

                // legendaries info
                if (menuBoxLegendaries.Checked)
                {
                    labelLegs2.Text = legendariesCounter.ToString();

                    // session per hour
                    Double legsPerH = (double)legendariesCounter / totalRunTimer;
                    legsPerH *= 3600;
                    labelLegsh2.Text = String.Format("{0:0.00}", legsPerH);
                }

                labelRunTimer.Text = "00:00:00"; // clear timer
                runTimer = 0;

                if (buttonStop.Text == "Reset")
                {
                    buttonStop.Enabled = false;
                    buttonStop.Text = "Pause";
                    buttonStart.Text = "Start";
                }
            }
            else MessageBox.Show("Saving 0 seconds Run does not make any sense.");
        }

        private void buttonAddLegendaries_Click(object sender, EventArgs e)
        {
            legendariesCounter++;
            labelLegendariesCounter.Text = legendariesCounter.ToString();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            timerRun.Start();
            buttonStop.Enabled = true;

            if (buttonStart.Text == "Start")
            {
                buttonStart.Text = "Resume";
                buttonStart.Enabled = false;
            }
            else
            {
                timerRun.Start();
                buttonStart.Enabled = false;
                buttonStop.Text = "Pause";

            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            timerRun.Stop();
            if (buttonStop.Text == "Pause")
            {
                buttonStop.Text = "Reset";
                buttonStart.Enabled = true;
            }
            else
            {
                runTimer = 0;
                labelRunTimer.Text = "00:00:00";

                buttonStop.Enabled = false;
                buttonStop.Text = "Pause";
                buttonStart.Text = "Start";
            }
        }

        private void timerRun_Tick(object sender, EventArgs e)
        {
            runTimer++;
            TimeSpan ts = TimeSpan.FromSeconds(runTimer);
            labelRunTimer.Text = ts.ToString();
        }

        private void timerOpacity_Tick(object sender, EventArgs e)
        {
            if (menuBoxOpacity.Checked)
            {
                this.Opacity = this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)) ? 0.99 : 0.75;
            }
            else this.Opacity = 0.99;
        }

        private void menuBoxOpacity_CheckedChanged(object sender, EventArgs e)
        {
            if (menuBoxOpacity.Checked)
            {
                timerOpacity.Start();
            }
            else
            {
                timerOpacity.Stop();
                this.Opacity = 0.99;
            }
        }

        private void menuBoxBorderless_CheckedChanged(object sender, EventArgs e)
        {
            if (menuBoxBorderless.Checked)
            {
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            }
            else
            {
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            }
        }

        private void menuBoxOnTop_CheckedChanged(object sender, EventArgs e)
        {
            if (menuBoxOnTop.Checked)
            {
                TopMost = true;
            }
            else
            {
                TopMost = false;
            }
        }

        private void menuBoxLegendaries_CheckedChanged(object sender, EventArgs e)
        {
            if (menuBoxLegendaries.Checked)
            {
                buttonAddLegendaries.Visible = true;
                labelLegendariesCounter.Visible = true;
            }
            else
            {
                buttonAddLegendaries.Visible = false;
                labelLegendariesCounter.Visible = false;
            }

            if (menuBoxGold.Checked || menuBoxLegendaries.Checked || menuBoxExp.Checked || menuBoxDE.Checked)
            {
                panelSave.Visible = true;
            }
            else panelSave.Visible = false;
        }

        private void menuBoxGold_CheckedChanged(object sender, EventArgs e)
        {
            if (menuBoxGold.Checked)
            {
                groupGold.Visible = true;
            }
            else
            {
                groupGold.Visible = false;
            }

            if (menuBoxGold.Checked || menuBoxLegendaries.Checked || menuBoxExp.Checked || menuBoxDE.Checked)
            {
                panelSave.Visible = true;
            }
            else panelSave.Visible = false;
        }

        private void menuMore_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
            }
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void comboBoxStatistics_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = comboBoxStatistics.SelectedItem;
            if (selectedItem.ToString() == "All")
            {
                tableLayoutExperienceLastRun.Visible = true;
                tableLayoutExperienceSession.Visible = true;
                tableLayoutGoldLastRun.Visible = true;
                tableLayoutGoldSession.Visible = true;
            }
            else if (selectedItem.ToString() == "Experience")
            {
                tableLayoutExperienceLastRun.Visible = true;
                tableLayoutExperienceSession.Visible = true;
                tableLayoutGoldLastRun.Visible = false;
                tableLayoutGoldSession.Visible = false;
            }
            else if (selectedItem.ToString() == "Gold+Items")
            {
                tableLayoutExperienceLastRun.Visible = false;
                tableLayoutExperienceSession.Visible = false;
                tableLayoutGoldLastRun.Visible = true;
                tableLayoutGoldSession.Visible = true;
            }
        }

        private void buttonLoadData_Click(object sender, EventArgs e)
        {
            System.IO.Stream myStream = null;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog.OpenFile()) != null)
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(myStream))
                        {
                            //reading file
                            while (!reader.EndOfStream)
                            {
                                String line = reader.ReadLine();
                                String[] column = line.Split('|');
                                if (column.Length < 3) column = line.Split('#');
                                if (column.Length >= 3)
                                {
                                    // column[0] = run name, column[1] = time, column[2] = exp;
                                    totalRunTimer += Convert.ToInt32(column[1]);
                                    Double loadXp;
                                    Double.TryParse(column[2], out loadXp);
                                    totalXPEarned += loadXp;

                                    numberOfRuns++;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }

            TimeSpan timeSpan = TimeSpan.FromSeconds(totalRunTimer);

            labelTime2.Text = timeSpan.ToString(@"hh\:mm\:ss"); ; //total runs time
            labelXPGained2.Text = String.Format("{0:0.00}", totalXPEarned) + " m"; //xp gained session

            // xp gained session per second
            Double xpPerH = totalXPEarned / totalRunTimer;
            xpPerH *= 60; // per minute
            labelXPm2.Text = String.Format("{0:0.00}", xpPerH) + " m";
            xpPerH *= 60; //per hour
            labelXPh2.Text = String.Format("{0:0.00}", xpPerH) + " m";
            xpPerH = totalXPEarned / numberOfRuns; //per run
            labelXPr2.Text = String.Format("{0:0.00}", xpPerH) + " m";
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            runTimer = 0;
            totalRunTimer = 0;
            legendariesCounter = 0;
            totalXPEarned = 0;
            totalGoldEarrned = 0;
            numberOfRuns = 0;
            elitesCounter = 0;
            demonicsCounter = 0;
            lastRunEss = 0;

            labelTime1.Text = "--"; labelTime2.Text = "--";
            labelXPGained1.Text = "--"; labelXPGained2.Text = "--";
            labelXPh1.Text = "--"; labelXPh2.Text = "--";
            labelXPm1.Text = "--"; labelXPm2.Text = "--";
            labelNextLevel1.Text = "--"; labelNextLevel2.Text = "--";
            label100level1.Text = "--"; label100level2.Text = "--";
            labelGoldEarned1.Text = "--"; labelGoldEarned2.Text = "--";
            labelGoldh1.Text = "--"; labelGoldh2.Text = "--";
            labelLegs2.Text = "--"; labelLegsh2.Text = "--";
            labelXPr2.Text = "--";
            labelLegendariesCounter.Text = "0";
            labelDemonics.Text = "0"; labelElites.Text = "0";
        }

        private void menuItemHelp_Click(object sender, EventArgs e)
        {
            using (About box = new About())
            {
                box.ShowDialog(this);
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.StartXP = textBoxStartXP.Text;
            Properties.Settings.Default.Paragon = (int)textBoxParagonLevel.Value;
            Properties.Settings.Default.StartGold = textBoxStartGold.Text;
            Properties.Settings.Default.Filename = textBoxFileName.Text;
            Properties.Settings.Default.Runname = textBoxRunName.Text;
            Properties.Settings.Default.Statistics = checkBoxStatistics.Checked;
            Properties.Settings.Default.OnTop = menuBoxOnTop.Checked;
            Properties.Settings.Default.Borderless = menuBoxBorderless.Checked;
            Properties.Settings.Default.Opacity = menuBoxOpacity.Checked;
            Properties.Settings.Default.Legendaries = menuBoxLegendaries.Checked;
            Properties.Settings.Default.Gold = menuBoxGold.Checked;
            Properties.Settings.Default.Experience = menuBoxExp.Checked;
            Properties.Settings.Default.DE = menuBoxDE.Checked;

            Properties.Settings.Default.Save();
        }

        private void menuItemAbout_Click(object sender, EventArgs e)
        {
            using (Help box = new Help())
            {
                box.ShowDialog(this);
            }
        }

        private void menuBoxExp_CheckedChanged(object sender, EventArgs e)
        {
            if (menuBoxExp.Checked)
            {
                groupLeveling.Visible = true;
            }
            else
            {
                groupLeveling.Visible = false;
            }

            if (menuBoxGold.Checked || menuBoxLegendaries.Checked || menuBoxExp.Checked || menuBoxDE.Checked)
            {
                panelSave.Visible = true;
            }
            else panelSave.Visible = false;
        }

        private void menuItemReset_Click(object sender, EventArgs e)
        {
            buttonReset.PerformClick();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            elitesCounter++;
            labelElites.Text = elitesCounter.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            demonicsCounter++;
            labelDemonics.Text = demonicsCounter.ToString();
        }

        private void menuBoxDE_CheckedChanged(object sender, EventArgs e)
        {
            if (menuBoxDE.Checked)
            {
                panelDemonicElite.Visible = true;
            }
            else
            {
                panelDemonicElite.Visible = false;
            }

            if (menuBoxGold.Checked || menuBoxLegendaries.Checked || menuBoxExp.Checked || menuBoxDE.Checked)
            {
                panelSave.Visible = true;
            }
            else panelSave.Visible = false;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // hang on events
            //actHook.OnMouseActivity += new MouseEventHandler(MouseMoved);
            //actHook.KeyPress += new KeyPressEventHandler(MyKeyPress);
        }

        public void MyKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
            {
                isShiftDown = true;
            }
            if (e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu)
            {
                isShiftDown = true;
            }
            if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
            {
                isShiftDown = true;
            }

            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
            {
                isCtrlDown = true;
            }
            if (e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu)
            {
                isCtrlDown = true;
            }
            if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
            {
                isCtrlDown = true;
            }

            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
            {
                isAltDown = true;
            }
            if (e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu)
            {
                isAltDown = true;
            }
            if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
            {
                isAltDown = true;
            }

            bool mod1 = false, mod2 = false, mod3 = false, mod4 = false, mod5 = false;

            if (Properties.Settings.Default.startTimerMod == "Shift") mod1 = isShiftDown;
            else if (Properties.Settings.Default.startTimerMod == "Ctrl") mod1 = isCtrlDown;
            else if (Properties.Settings.Default.startTimerMod == "Alt") mod1 = isAltDown;

            if (Properties.Settings.Default.stopTimerMod == "Shift") mod2 = isShiftDown;
            else if (Properties.Settings.Default.stopTimerMod == "Ctrl") mod2 = isCtrlDown;
            else if (Properties.Settings.Default.stopTimerMod == "Alt") mod2 = isAltDown;

            if (Properties.Settings.Default.addLegMod == "Shift") mod3 = isShiftDown;
            else if (Properties.Settings.Default.addLegMod == "Ctrl") mod3 = isCtrlDown;
            else if (Properties.Settings.Default.addLegMod == "Alt") mod3 = isAltDown;

            if (Properties.Settings.Default.addDEMod == "Shift") mod4 = isShiftDown;
            else if (Properties.Settings.Default.addDEMod == "Ctrl") mod4 = isCtrlDown;
            else if (Properties.Settings.Default.addDEMod == "Alt") mod4 = isAltDown;

            if (Properties.Settings.Default.addEKMod == "Shift") mod5 = isShiftDown;
            else if (Properties.Settings.Default.addEKMod == "Ctrl") mod5 = isCtrlDown;
            else if (Properties.Settings.Default.addEKMod == "Alt") mod5 = isAltDown;


            if (e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.startTimerBind) && mod1)
            {
                buttonStart.PerformClick();
            }
            else if (e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.stopTimerBind) && mod2)
            {
                buttonStop.PerformClick();
            }
            else if (e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.addLegBind) && mod3)
            {
                buttonAddLegendaries.PerformClick();
            }
            else if (e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.addDEBind) && mod4)
            {
                button1.PerformClick();
            }
            else if (e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.addEKBind) && mod5)
            {
                button2.PerformClick();
            }
        }

        public void MyKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
            {
                isShiftDown = false;
            }
            if (e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu)
            {
                isShiftDown = false;
            }
            if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
            {
                isShiftDown = false;
            }

            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
            {
                isCtrlDown = false;
            }
            if (e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu)
            {
                isCtrlDown = false;
            }
            if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
            {
                isCtrlDown = false;
            }

            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
            {
                isAltDown = false;
            }
            if (e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu)
            {
                isAltDown = false;
            }
            if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
            {
                isAltDown = false;
            }
        }

        private void menuBoxGlobal_CheckedChanged(object sender, EventArgs e)
        {
            if (menuBoxGlobal.Checked)
            {
                actHook = new UserActivityHook(); // crate an instance with global hooks
                actHook.Start();
                actHook.KeyUp += new KeyEventHandler(MyKeyUp);
                actHook.KeyDown += new KeyEventHandler(MyKeyDown);
            }
            else
            {
                actHook.Stop();
            }
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (!menuBoxGlobal.Checked)
            {
                if ( e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.startTimerBind) &&
                    e.Modifiers == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.startTimerMod) ) buttonStart.PerformClick();
                else if (e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.stopTimerBind) &&
                    e.Modifiers == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.stopTimerMod)) buttonStop.PerformClick();
                else if (e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.addLegBind) &&
                    e.Modifiers == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.addLegMod)) buttonAddLegendaries.PerformClick();
                else if (e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.addDEBind) &&
                    e.Modifiers == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.addDEMod)) button1.PerformClick();
                else if ( e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.addEKBind) &&
                    e.Modifiers == (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.addEKMod) ) button2.PerformClick();
            }
        }
    }
}
