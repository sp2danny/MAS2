﻿using MirrorAlignmentSystem.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MirrorAlignmentSystem
{
    /// <summary>
    ///  This class Handles the Graphic User Interface and everything associated with it
    /// </summary>
    public partial class MainWindow : Form
    {
        string Alignment = "over";
        bool manualAlignment = true;
        delegate void SetTextCallback(int i);
        delegate void SetEnableCallback(int i);
        delegate void ShowFPSCallback(float FPS);
        delegate void SetDirLeftRightLabelCallback(string dir);
        delegate void SetDirUpDownLabelCallback(string dir);
        delegate void SetImagesCallback();
        //delegate void SetBlackBackgroundImageCallback(Bitmap image);
        //delegate void SetBitmapBackgroundImageCallback(Bitmap image);
        delegate void SetOverviewImageCallback(Bitmap image);
        delegate void SetCalibrationImageCallback(Bitmap image);
        delegate void SetBlackHoleImageCallback(Bitmap image);
        delegate void SetCombinedImageCallback(Bitmap image);
        delegate void SetLeftImageCallback(Bitmap image);
        delegate void SetRightImageCallback(Bitmap image);
        delegate void SetUpImageCallback(Bitmap image);
        delegate void SetDownImageCallback(Bitmap image);
        delegate void SetMotor1Callback(string motor1Pos);
        delegate void SetOnOffButtonCallback(string onOff);
        delegate void SetMotor1AlarmCallback(string motor1Alarm);
        delegate void SetMotor1SPReachedCallback(string motor1SPReached);
        delegate void SetMotor2Callback(string motor2Pos);
        delegate void SetOnOff2ButtonCallback(string onOff);
        delegate void SetMotor2AlarmCallback(string motor2Alarm);
        delegate void SetMotor2SPReachedCallback(string motor2SPReached);
        delegate void SetMotor1MDIActiveCallback(string motor1MDIActive);
        delegate void SetMotor2MDIActiveCallback(string motor2MDIActive);
        delegate void SetCoMLabelActiveCallback(string CoM, Point realCoMInput);
        delegate void SetvisibilityCallback(bool mode);
        delegate void UpdateLabelCallback(string segnum,
            double align_x,
            double align_y,
            double rot_x,
            double rot_y,
            double rot_z);

        string valueDiscIDTextBox = "NOT SET";
        string valueZdist;
        string valueSegmentNumberTextbox;
        string valueLiveCheckState = "Checked";
        string valueCalBackCheckState = "Checked";

        string finePBshow;

        Point realCoM;

        string currentUser = "Admin";
        string currentUserType = "Admin";

        bool settingsUpdated = false;
        bool manualUpdateOverview = false;

        CameraController cameraController;
        CheckAllForm caf = new CheckAllForm();

        public List<Segment> LoadedSegments { get; set; }

        /// <summary>
        /// Gets the caf.
        /// </summary>
        /// <returns></returns>
        public CheckAllForm getCAF()
        {
            return caf;
        }

        /// <summary>
        /// The class constructor
        /// </summary>
        public MainWindow(CameraController inputCamera)
        {
            InitializeComponent();

            cameraController = inputCamera;

            CurrentUser.SetCurrentUser("Admin");
            CurrentUser.SetCurrentUserType("Admin");

            //Create path
            string pathToday = "C:/MASDATA/" + valueDiscIDTextBox + "/" + System.DateTime.Now.ToString("yyyy_MM_dd");
            System.IO.Directory.CreateDirectory(pathToday);

            DiscIDTextBox.Text = "NOT SET";
            valueDiscIDTextBox = DiscIDTextBox.Text;
            SegmentNumberTextbox.Text = "0911";
            valueSegmentNumberTextbox = SegmentNumberTextbox.Text;
            finePBshow = "sgbg";
            exposureSlider.Value = 100;

            CheckForIllegalCrossThreadCalls = false;

            SetSegmentControllerArrows();
            //exposureSlider.SetThumbRect();
            ShowSegmentId(valueSegmentNumberTextbox);
        }

        private void SetSegmentControllerArrows()
        {
            //CCW_Button
            var counterClockwiseArrow = new Bitmap(Resources.curvedArrow);
            counterClockwiseArrow.RotateFlip(RotateFlipType.RotateNoneFlipX);
            this.CounterClockwise_button.BackgroundImage = counterClockwiseArrow;
            this.CounterClockwise_button.BackgroundImageLayout = ImageLayout.Stretch;
            this.CounterClockwise_button.Text = "";

            //CCW_Button
            var clockwiseArrow = new Bitmap(Resources.curvedArrow);
            this.Clockwise_button.BackgroundImage = clockwiseArrow;
            this.Clockwise_button.BackgroundImageLayout = ImageLayout.Stretch;
            this.Clockwise_button.Text = "";
        }

        /// <summary>
        /// Lets the GUI know that the settings have been updated or not
        /// </summary>
        /// <param name="value">Sets the settingsUpdated variable depeding on what the application are requesting</param>    
        public void SettingsUpdated(bool value)
        {
            settingsUpdated = value;
        }

        /// <summary>
        /// Disposes the images on display
        /// </summary>
        public void DisposeImages()
        {
            if (this.CalibrateImgPB.InvokeRequired)
            {
                SetImagesCallback d = new SetImagesCallback(DisposeImages);
                this.Invoke(d, new object[] { });
            }
            else
            {
                CalibrateImgPB.Image.Dispose();
            }
        }

        /// <summary>
        /// Returns the current value of the settingsUpdated variable, if this one is true the settings needs to be updated
        /// </summary>
        /// <returns>
        /// Returns a boolean indicating if the settings have been updated
        /// </returns>
        public bool IsSettingsUpdated()
        {
            return settingsUpdated;
        }

        /// <summary>
        /// This method updates all the GUI elements that needs to be updated every cycle of the worker class
        /// </summary>
        /// <param name="i">A counter that indicates to the user that the worker and GUI classes are working with eachother.
        /// The worker count and the GUI displays the new value to the user every cycle.</param>    
        public void UpdateGUI(int i)
        {
            currentUser = CurrentUser.GetCurrentUser();
            currentUserType = CurrentUser.GetCurrentUserType();

            if (currentUser.Trim() != "")
            {
                if (String.Compare(currentUserType.Trim(), "Admin") == 0)
                {
                    if (this.DiscIDTextBox.InvokeRequired || this.SegmentNumberTextbox.InvokeRequired)
                    {
                        SetEnableCallback e = new SetEnableCallback(UpdateGUI);
                        this.Invoke(e, new object[] { i });
                    }
                    else
                    {
                        this.DiscIDTextBox.Enabled = true;
                        this.SegmentNumberTextbox.Enabled = true;
                    }
                }
                else
                {
                    if (this.DiscIDTextBox.InvokeRequired || this.SegmentNumberTextbox.InvokeRequired)
                    {
                        SetEnableCallback e = new SetEnableCallback(UpdateGUI);
                        this.Invoke(e, new object[] { i });
                    }
                    else
                    {
                        this.DiscIDTextBox.Enabled = false;
                        this.SegmentNumberTextbox.Enabled = false;
                    }
                }
            }
            else
            {
                if (this.DiscIDTextBox.InvokeRequired || this.SegmentNumberTextbox.InvokeRequired)
                {
                    SetEnableCallback e = new SetEnableCallback(UpdateGUI);
                    this.Invoke(e, new object[] { i });
                }
                else
                {

                }
            }

        }

        /// <summary>
        /// Updates the picturebox which contains the overview image for the segment
        /// </summary>
        /// <param name="image">The image to be displayed in the picturebox</param>    
        public void ShowOverviewBitmap(Bitmap image)
        {
            if (this.overviewImagePB.InvokeRequired)
            {
                SetOverviewImageCallback d = new SetOverviewImageCallback(ShowOverviewBitmap);
                this.Invoke(d, new object[] { image.Clone() });
            }
            else
            {
                overviewImagePB.Image = null;
                overviewImagePB.Image = (Bitmap)image.Clone();
            }
        }

        /// <summary>
        /// Updates the picturebox which contains the calibration image
        /// </summary>
        /// <param name="image">The image to be displayed in the picturebox</param>    
        public void ShowCalibrateBitmap(Bitmap image)
        {
            if (this.CalibrateImgPB.InvokeRequired)
            {
                SetCalibrationImageCallback d = new SetCalibrationImageCallback(ShowCalibrateBitmap);
                this.Invoke(d, new object[] { image.Clone() });
            }
            else
            {
                CalibrateImgPB.Image = null;
                CalibrateImgPB.Image = (Bitmap)image.Clone();
            }
        }

        /// <summary>
        /// Updates the picturebox which contains the calibration image
        /// </summary>
        /// <param name="image">The image to be displayed in the picturebox</param>    
        public void ShowBlackHoleBitmap(Bitmap image)
        {
            if (this.BlackHolePB.InvokeRequired)
            {
                SetBlackHoleImageCallback d = new SetBlackHoleImageCallback(ShowBlackHoleBitmap);
                this.Invoke(d, new object[] { image.Clone() });
            }
            else
            {
                BlackHolePB.Image = null;
                BlackHolePB.Image = (Bitmap)image.Clone();
            }
        }

        /// <summary>
        /// Updates the picturebox which contains the image of the combined image calculated by the algorithm during fine alignment
        /// </summary>
        /// <param name="image">The image to be displayed in the picturebox</param>    
        public void ShowCombinedBitmap(Bitmap image)
        {
            if (this.combinedImagePB.InvokeRequired)
            {
                SetCombinedImageCallback d = new SetCombinedImageCallback(ShowCombinedBitmap);
                this.Invoke(d, new object[] { image.Clone() });
            }
            else
            {
                combinedImagePB.Image = null;
                combinedImagePB.Image = (Bitmap)image.Clone(); //image;
            }
        }

        /// <summary>
        /// This method returns the value currently in the textbox for interval between black background and bitmap on the monitor
        /// </summary>
        /// <returns>
        /// Returns the number(as a string) of intervall between the monitor showing a bitmap and a black background, only works in Fine Alignment phase
        /// </returns>
        public string GetDiscID()
        {
            return valueDiscIDTextBox;
        }

        /// <summary>
        /// Gets the live mode.
        /// </summary>
        /// <returns></returns>
        public string GetLiveMode()
        {
            return valueLiveCheckState;
        }

        /// <summary>
        /// Gets the state for remove background in calibrate image.
        /// </summary>
        /// <returns></returns>
        public string GetCalBackMode()
        {
            return valueCalBackCheckState;
        }

        /// <summary>
        /// This method returns the value of the textbox containing the segment the user want to run at the moment
        /// </summary>
        /// <returns>
        /// Returns the number(as a string) of the segment that the user wants to run the algorithm on at the moment
        /// </returns>
        public string GetSegmentNumber()
        {
            return valueSegmentNumberTextbox;
        }

        /// <summary>
        /// Returns the value for fine alignment if the picture should be signal, background or signal-background
        /// </summary>
        /// <returns>
        /// Returns the setting as a string 
        /// </returns>
        public string GetFineShow()
        {
            return finePBshow;
        }

        /// <summary>
        /// This method returns the current online/offline mode
        /// </summary>
        /// <returns>
        /// The online/offline mode
        /// </returns>
        public bool GetOfflineOnlineMode()
        {
            return false;
        }

        /// <summary>
        /// This method returns the current fine/coarse alignment mode
        /// </summary>
        /// <returns>
        /// The fine/coarse alignment mode
        /// </returns>
        public string GetAlignmentMode()
        {
            string rval = Alignment;
            if (rval[0] == 'c' && rval[1] == 'h')
                Alignment = "over";
            return rval;
        }

        /// <summary>
        /// This method returns true if the application is in manual alignment mode
        /// </summary>
        /// <returns>
        /// The alignment mode
        /// </returns>
        public bool GetManualAlignment()
        {
            return manualAlignment;
        }
        
        /// <summary>
        /// Event method for when the user presses the enter button inside the textbox displaying the number of cycles between each black image on the monitor
        /// </summary>
        /// <param name="sender">The control, in this method the textbox</param> 
        /// <param name="e">The event information</param>  
        private void DiscIDTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && (valueDiscIDTextBox != DiscIDTextBox.Text))
            {
                //this.ActiveControl = BlackBGNumberLabel;

                DAL.InsertEvent(DiscIDTextBox.Text, valueDiscIDTextBox, "No user", "Intervall black background change", "BlackBGNumberTextBox");

                valueDiscIDTextBox = DiscIDTextBox.Text;

                string pathString1 = "C:/MASDATA/" + valueDiscIDTextBox + "/";
                string pathString2 = "C:/MASDATA/" + valueDiscIDTextBox + "/" + System.DateTime.Now.ToString("yyyy_MM_dd");
                System.IO.Directory.CreateDirectory(pathString1);
                System.IO.Directory.CreateDirectory(pathString2);
            }
        }

        /// <summary>
        /// Event method for when the user presses the enter button inside the textbox displaying the z distance
        /// </summary>
        /// <param name="sender">The control, in this method the textbox</param> 
        /// <param name="e">The event information</param>  
        private void zDistTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && (valueZdist != DiscIDTextBox.Text))
            {
                valueZdist = zDist.Text;

                if (Math.Abs(double.Parse(valueZdist) - 7576) > 5)
                {
                    MessageBox.Show("Warning! Z distance not within tolerance");
                }
                else
                {
                    label5.ForeColor = Color.Green;
                }
                double[] data = { double.Parse(valueZdist) };
            }
        }

        /// <summary>
        /// Event method for when the user presses the enter button inside the textbox displaying the current segment number being worked on
        /// </summary>
        /// <param name="sender">The control, in this method the textbox</param> 
        /// <param name="e">The event information</param>  
        private void SegmentNumberTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && (valueSegmentNumberTextbox != SegmentNumberTextbox.Text))
            {
                //this.ActiveControl = SegmentNumberLabel;

                DAL.InsertEvent(SegmentNumberTextbox.Text, valueSegmentNumberTextbox, CurrentUser.GetCurrentUser(), "Segment changed", "SegmentNumberTextbox");

                valueSegmentNumberTextbox = SegmentNumberTextbox.Text;
                ShowSegmentId(valueSegmentNumberTextbox);
            }
        }


        /// <summary>
        /// Event method for when the user clicks the eventlog button in the menu. Displays the eventlog to the user
        /// </summary>
        /// <param name="sender">The control, in this method the menuitem</param> 
        /// <param name="e">The event information</param>  
        private void eventLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Eventlog eventForm = new Eventlog();

            eventForm.TopMost = true;
            eventForm.Show();
        }

        private void ShutDownProper()
        {
            //cameraController.StopChecking();

            Visible = false;

            System.Threading.Thread.Sleep(1);

            DAL.UpdateFPSExp(cameraController.GetExposureTime(), cameraController.GetFramerate());

            Application.Exit();
            System.Environment.Exit(0);

        }

        /// <summary>
        /// Event method for when the user clicks the exit button in the menu. Terminates the program
        /// </summary>
        /// <param name="sender">The control, in this method the menuitem</param> 
        /// <param name="e">The event information</param> 
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShutDownProper();
        }

        /// <summary>
        /// Event method for when the user clicks the login button. Creates a new LoginForm
        /// </summary>
        /// <param name="sender">The control, in this method the button</param> 
        /// <param name="e">The event information</param> 
        private void button1_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();

            loginForm.TopMost = true;

            loginForm.Show();
        }

        /// <summary>
        /// Event method for when the user clicks the logout button. Calls the CurrentUser class and logouts the user
        /// </summary>
        /// <param name="sender">The control, in this method the button</param> 
        /// <param name="e">The event information</param> 
        private void logoutButton_Click(object sender, EventArgs e)
        {
            CurrentUser.Logout();
        }

        /// <summary>
        /// Event method for when the user clicks the administrate button in the menu. creates a new AdministrateUserForm if the current user is the a admin
        /// </summary>
        /// <param name="sender">The control, in this method the menuitem</param> 
        /// <param name="e">The event information</param> 
        private void administrateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.Compare(currentUserType.Trim(), "Admin") == 0)
            {
                AdministrateUsersForm adminForm = new AdministrateUsersForm();

                adminForm.Show();
                adminForm.TopMost = true;
            }
            else
            {
                MessageBox.Show("You are not the administrator");
            }
        }

        private void SetToFine()
        {
            Alignment = "fine";
            BIA_timer.Enabled = false;

            DAL.InsertEvent("1", "0", CurrentUser.GetCurrentUser(), "Fine alignment activated", "fineRadioButton");
        }

        private void SetToCoarse()
        {
            Alignment = "coarse";
            BIA_timer.Enabled = true;
            DAL.InsertEvent("0", "1", CurrentUser.GetCurrentUser(), "Coarse alginment deactivated", "coarceRadioButton");
        }
        
        /// <summary>
        /// Event method for when the user clicks the option button in the menu. creates a new OptionForm if the current user is the a admin
        /// </summary>
        /// <param name="sender">The control, in this method the menuitem</param> 
        /// <param name="e">The event information</param> 
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.Compare(currentUserType.Trim(), "Admin") == 0)
            {
                OptionForm optionForm = new OptionForm(cameraController);

                optionForm.ParentWindow = this;

                optionForm.Show();
                optionForm.TopMost = true;
            }
            else
            {
                MessageBox.Show("You are not the administrator");
            }
        }

        /// <summary>
        /// Event method for when the user clicks the image history button in the menu. creates a new ImageHistory form if the current user is the a admin
        /// </summary>
        /// <param name="sender">The control, in this method the menuitem</param> 
        /// <param name="e">The event information</param> 
        private void imagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.Compare(currentUserType.Trim(), "Admin") == 0)
            {
                ImageHistory imgHistory = new ImageHistory();

                imgHistory.Show();
                imgHistory.TopMost = true;
            }
            else
            {
                MessageBox.Show("You are not the administrator");
            }
        }

        /// <summary>
        /// Updates the picturebox which contains the image of the segment when the left bitmap is shown on the monitor, only during coarse alignment
        /// </summary>
        /// <param name="image">The image to be displayed in the picturebox</param>   
        public void ShowLeftRightOneBackgroundBitmap(Bitmap image)
        {
            if (this.leftRightPBOne.InvokeRequired)
            {
                SetLeftImageCallback d = new SetLeftImageCallback(ShowLeftRightOneBackgroundBitmap);
                this.Invoke(d, new object[] { image });
            }
            else
            {
                leftRightPBOne.Image = null;

                leftRightPBOne.Image = new Bitmap(image);

            }
        }

        /// <summary>
        /// Updates the picturebox which contains the image of the segment when the right bitmap is shown on the monitor, only during coarse alignment
        /// </summary>
        /// <param name="image">The image to be displayed in the picturebox</param>  
        public void ShowLeftRightTwoBackgroundBitmap(Bitmap image)
        {
            if (this.leftRightPBTwo.InvokeRequired)
            {
                SetRightImageCallback d = new SetRightImageCallback(ShowLeftRightTwoBackgroundBitmap);
                this.Invoke(d, new object[] { image });
            }
            else
            {
                leftRightPBTwo.Image = null;

                leftRightPBTwo.Image = new Bitmap(image);

            }
        }

        /// <summary>
        /// Updates the picturebox which contains the image of the segment when the up bitmap is shown on the monitor, only during coarse alignment
        /// </summary>
        /// <param name="image">The image to be displayed in the picturebox</param>  
        public void ShowUpDownOneBackgroundBitmap(Bitmap image)
        {
            if (this.upDownPBOne.InvokeRequired)
            {
                SetUpImageCallback d = new SetUpImageCallback(ShowUpDownOneBackgroundBitmap);
                this.Invoke(d, new object[] { image });
            }
            else
            {
                upDownPBOne.Image = null;

                upDownPBOne.Image = new Bitmap(image);
            }
        }

        /// <summary>
        /// Updates the picturebox which contains the image of the segment when the down bitmap is shown on the monitor, only during coarse alignment
        /// </summary>
        /// <param name="image">The image to be displayed in the picturebox</param>  
        public void ShowUpDownTwoBackgroundBitmap(Bitmap image)
        {
            if (this.upDownPBTwo.InvokeRequired)
            {
                SetDownImageCallback d = new SetDownImageCallback(ShowUpDownTwoBackgroundBitmap);
                this.Invoke(d, new object[] { image });
            }
            else
            {
                upDownPBTwo.Image = null;

                upDownPBTwo.Image = new Bitmap(image);
            }
        }

        /// <summary>
        /// Setting the update telling the worker that the overview image should be updated
        /// </summary>
        /// <param name="value">Set or not set the value that the worker reads</param>  
        public void SetManualUpdateOverviewImg(bool value)
        {
            manualUpdateOverview = value;
        }

        /// <summary>
        /// Setting the update telling the worker that the overview image should be updated
        /// </summary>
        public bool GetUpdateManualOverview()
        {
            return manualUpdateOverview;
        }

        /// <summary>
        /// Updates the CoM value in the GUI
        /// </summary>
        /// <param name="CoM">The CoM string</param>
        /// <param name="realCoMInput">beats me</param>
        public void UpdateCoMLabel(string CoM, Point realCoMInput)
        {
            if (this.CoMLabel.InvokeRequired)
            {
                SetCoMLabelActiveCallback d = new SetCoMLabelActiveCallback(UpdateCoMLabel);
                this.Invoke(d, new object[] { CoM, realCoMInput });
            }
            else
            {
                CoMLabel.Text = "CoM offset: " + CoM;
                realCoM = realCoMInput;
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            ShutDownProper();
        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabOverview"])
            {
                Alignment = "over";
            }

            if (tabControl1.SelectedTab == tabControl1.TabPages["tabCoarse"])
            {
                SetToCoarse();
            }

            if (tabControl1.SelectedTab == tabControl1.TabPages["tabFine"])
            {
                SetToFine();
            }

            if (tabControl1.SelectedTab == tabControl1.TabPages["tabCal"])
            {
                Alignment = "calibrate";
            }
        }
        

        // The LIVE button has changed state
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            valueLiveCheckState = checkBox1.CheckState.ToString();
            //MessageBox.Show(valueLiveCheckState);
        }

        class SegmentNumber
        {
            public void FromString(string str)
            {
                int pos0 = str[0] - '0';
                int pos1 = str[1] - '0';
                int pos2 = str[2] - '0';
                int pos3 = str[3] - '0';
                line = pos0 * 10 + pos1;
                seg = pos2;
                sub = pos3;
            }
            public override string ToString()
            {
                string ss =
                    line.ToString();
                if (line < 10)
                    ss = '0' + ss;
                ss += seg.ToString();
                ss += sub.ToString();
                return ss;
            }

            private int SubCnt()
            {
                switch (seg)
                {
                    case 1: return 1;
                    case 2: return 2;
                    case 3: return 3;
                }
                return 0;
            }

            private void NxtLine()
            {
                ++line;
                if (line >= 12)
                    line -= 11;
            }
            private void PrvLine()
            {
                --line;
                if (line <= 0)
                    line += 11;
            }

            public void Up()
            {
                if (sub == 1)
                {
                    sub = SubCnt();
                    PrvLine();
                }
                else
                {
                    --sub;
                }
            }

            public void Dn()
            {
                if (sub == SubCnt())
                {
                    sub = 1;
                    NxtLine();
                }
                else
                {
                    ++sub;
                }
            }
            public void Lf()
            {
                if (seg > 1) --seg;
                if (sub > SubCnt())
                    sub = SubCnt();
            }
            public void Rg()
            {
                if (seg < 3) ++seg;
            }

            private int line, seg, sub;

        }

        SegmentNumber segnum = new SegmentNumber();

        private void CounterClockwise_button_Click(object sender, EventArgs e)
        {
            segnum.FromString(valueSegmentNumberTextbox);
            segnum.Up();
            string seg = segnum.ToString();
            valueSegmentNumberTextbox = seg;
            SegmentNumberTextbox.Text = seg;
            ShowSegmentId(seg);
        }

        private void Clockwise_button_Click(object sender, EventArgs e)
        {
            segnum.FromString(valueSegmentNumberTextbox);
            segnum.Dn();
            string seg = segnum.ToString();
            valueSegmentNumberTextbox = seg;
            SegmentNumberTextbox.Text = seg;
            ShowSegmentId(seg);
        }

        private void Out_button_Click(object sender, EventArgs e)
        {
            segnum.FromString(valueSegmentNumberTextbox);
            segnum.Rg();
            string seg = segnum.ToString();
            valueSegmentNumberTextbox = seg;
            SegmentNumberTextbox.Text = seg;
            ShowSegmentId(seg);
        }

        private void In_button_Click(object sender, EventArgs e)
        {
            segnum.FromString(valueSegmentNumberTextbox);
            segnum.Lf();
            string seg = segnum.ToString();
            valueSegmentNumberTextbox = seg;
            SegmentNumberTextbox.Text = seg;
            ShowSegmentId(seg);
        }

        private void btnCheckAllFine_Click(object sender, EventArgs e)
        {
            // check all
            bool old = checkBox1.Checked;
            checkBox1.Checked = false;
            Alignment = "checkALLfine";
            caf.Start();
            caf.Title("check all fine");
            caf.ShowDialog();
            checkBox1.Checked = old;
        }

        private void btnCheckAllCoarse_Click(object sender, EventArgs e)
        {
            // check all
            bool old = checkBox1.Checked;
            checkBox1.Checked = false;
            Alignment = "checkALLcoarse";
            caf.Start();
            caf.Title("check all coarse");
            caf.ShowDialog();
            checkBox1.Checked = old;
        }

        /// <summary>
        /// show segment number
        /// </summary>
        /// <param name="segnum"></param>
        public void ShowSegmentId(string segnum)
        {
            segmentId_statusLabel.Text = "SEGMENT " + segnum;
        }

        /// <summary>
        /// show algnment
        /// </summary>
        /// <param name="align_x"></param>
        /// <param name="align_y"></param>
        public void ShowOffsetAlignment(double align_x, double align_y)
        {
            try
            {
                AlignmentOffsetXValue_statusLabel.Text = align_x.ToString("N1") + " mm";
                AlignmentOffsetXValue_statusLabel.ForeColor = (Math.Abs(align_x) > 4) ? Color.Red : Color.Green;

                AlignmentOffsetYValue_statusLabel.Text = align_y.ToString("N1") + " mm";
                AlignmentOffsetYValue_statusLabel.ForeColor = (Math.Abs(align_y) > 4) ? Color.Red : Color.Green;
            }
            catch
            {
                //????
            }
        }

        /// <summary>
        /// show rotation
        /// </summary>
        /// <param name="rot_x"></param>
        /// <param name="rot_y"></param>
        /// <param name="rot_z"></param>
        public void ShowOffsetRotation(double rot_x, double rot_y, double rot_z)
        {
            RotationOffsetXValue_statusLabel.Text = rot_x.ToString("N1") + " mrad";
            RotationOffsetXValue_statusLabel.ForeColor = (Math.Abs(rot_x) > 3) ? Color.Red : Color.Green;

            RotationOffsetYValue_statusLabel.Text = rot_y.ToString("N1") + " mrad";
            RotationOffsetYValue_statusLabel.ForeColor = (Math.Abs(rot_y) > 3) ? Color.Red : Color.Green;

            RotationOffsetZValue_statusLabel.Text = rot_z.ToString("N1") + " mrad";
            RotationOffsetZValue_statusLabel.ForeColor = (Math.Abs(rot_z) > 10) ? Color.Red : Color.Green;
        }

        /// <summary>
        /// show number of segments used as reference points
        /// </summary>
        /// <param name="NOKsegs"
        public void ShowsRefSegs(int NOKsegs)
        {
            calibrateReferencePoints_label.Text = NOKsegs.ToString();
            calibrateReferencePoints_label.ForeColor = (Math.Abs(NOKsegs) > 2) ? Color.Red : Color.Green;
        }

        /// <summary>
        /// Updates the labels.
        /// </summary>
        /// <param name="segnum">The segment number.</param>
        /// <param name="align_x">The x alignment.</param>
        /// <param name="align_y">The y alignment.</param>
        /// <param name="rot_x">The x rotation.</param>
        /// <param name="rot_y">The y rotation.</param>
        /// <param name="rot_z">The z rotation.</param>
        public void UpdateLabel(
            string segnum,
            double align_x,
            double align_y,
            double rot_x,
            double rot_y,
            double rot_z)
        {

            ShowSegmentId(segnum);
            ShowOffsetAlignment(align_x, align_y);
            ShowOffsetRotation(rot_x, rot_y, rot_z);
        }

        /// <summary>
        /// hide upper image
        /// </summary>
        public void HideUpperImg()
        {
            pb_Upper.Image = imageList1.Images[0];
            lbl_ul.Visible = false;
            lbl_ur.Visible = false;
        }

        /// <summary>
        /// set upper image l/r only
        /// </summary>
        /// <param name="lft"></param>
        /// <param name="rgt"></param>
        public void SetUpperImgNoval(int lft, int rgt)
        {
            lbl_ul.Visible = false;
            lbl_ur.Visible = false;

            bool neglft = lft < 0;
            if (neglft) lft = -lft;
            bool negrgt = rgt < 0;
            if (negrgt) rgt = -rgt;

            int num = 1 + (neglft ? 0 : 2) + (negrgt ? 0 : 1);
            pb_Upper.Image = imageList1.Images[num];
        }

        /// <summary>
        /// Sets the upper img.
        /// </summary>
        /// <param name="lft">The LFT.</param>
        /// <param name="rgt">The RGT.</param>
        public void SetUpperImg(int lft, int rgt)
        {
            if (lft == 0 && rgt == 0)
            {
                pb_Upper.Image = imageList1.Images[0];
                lbl_ul.Visible = false;
                lbl_ur.Visible = false;
                return;
            }

            lbl_ul.Visible = true;
            lbl_ur.Visible = true;

            bool neglft = lft < 0;
            if (neglft) lft = -lft;
            bool negrgt = rgt < 0;
            if (negrgt) rgt = -rgt;

            lbl_ul.Text = lft.ToString();
            lbl_ur.Text = rgt.ToString();

            int num = 1 + (neglft ? 0 : 2) + (negrgt ? 0 : 1);
            pb_Upper.Image = imageList1.Images[num];
        }
        
        /// <summary>
        /// hide lower img
        /// </summary>
        public void HideLowerImg()
        {
            pb_Lower.Image = imageList1.Images[0];
            lbl_ul.Visible = false;
            lbl_ur.Visible = false;
        }

        /// <summary>
        /// set lower image l/r only
        /// </summary>
        /// <param name="lft"></param>
        /// <param name="rgt"></param>
        public void SetLowerImgNoval(int lft, int rgt)
        {
            lbl_ll.Visible = false;
            lbl_lr.Visible = false;

            bool neglft = lft < 0;
            if (neglft) lft = -lft;
            bool negrgt = rgt < 0;
            if (negrgt) rgt = -rgt;

            int num = 1 + (neglft ? 0 : 2) + (negrgt ? 0 : 1);
            pb_Lower.Image = imageList1.Images[num];
        }
        
        /// <summary>
        /// Sets the lower img.
        /// </summary>
        /// <param name="lft">The LFT.</param>
        /// <param name="rgt">The RGT.</param>
        public void SetLowerImg(int lft, int rgt)
        {
            if (lft == 0 && rgt == 0)
            {
                pb_Lower.Image = imageList1.Images[0];
                lbl_ll.Visible = false;
                lbl_lr.Visible = false;
                return;
            }

            lbl_ll.Visible = true;
            lbl_lr.Visible = true;

            bool neglft = lft < 0;
            if (neglft) lft = -lft;
            bool negrgt = rgt < 0;
            if (negrgt) rgt = -rgt;

            lbl_ll.Text = lft.ToString();
            lbl_lr.Text = rgt.ToString();

            int num = 1 + (neglft ? 0 : 2) + (negrgt ? 0 : 1);
            pb_Lower.Image = imageList1.Images[num];
        }

        /// <summary>
        /// Sets the fine img.
        /// </summary>
        /// <param name="lft">The LFT.</param>
        /// <param name="rgt">The RGT.</param>
        public void SetFineImg(double lft, double rgt)
        {
            if (lft == 0 && rgt == 0)
            {
                pbFine.Image = imageList1.Images[0];
                lbl_fl.Visible = false;
                lbl_fr.Visible = false;
                return;
            }

            lbl_fl.Visible = true;
            lbl_fr.Visible = true;

            bool neglft = lft < 0.0;
            if (neglft) lft = -lft;
            bool negrgt = rgt < 0.0;
            if (negrgt) rgt = -rgt;

            lbl_fl.Text = lft.ToString("N1");
            lbl_fr.Text = rgt.ToString("N1");

            int num = 1 + (neglft ? 0 : 2) + (negrgt ? 0 : 1);
            pbFine.Image = imageList1.Images[num];
        }

        private bool can_accept = false;
        /// <summary>
        /// Sets the tangetial and radial ofsetts in "fine".
        /// </summary>
        /// <param name="tan">The tan.</param>
        /// <param name="rad">The rad.</param>
        public void SetTanRad(double tan, double rad)
        {
            lbl_tan_ofs.Text = tan.ToString("N2");
            lbl_rad_ofs.Text = rad.ToString("N2");
            can_accept = (Math.Abs(tan) <= 0.3) && (Math.Abs(rad) <= 0.3);
            acceptButton.BackColor = can_accept ? Color.Green : Color.Red;
            lbl_tan_ofs.ForeColor = (Math.Abs(tan) <= 0.3) ? Color.Green : Color.Red;
            lbl_rad_ofs.ForeColor = (Math.Abs(rad) <= 0.3) ? Color.Green : Color.Red;
        }

        /// <summary>
        /// Sets the available number of segments/refpoints for calibrate.
        /// </summary>
        /// <param name="tan">The tan.</param>
        /// <param name="rad">The rad.</param>
        public void SetTanRad(int numOfSegs)
        {
            calibrateReferencePoints_label.Text = numOfSegs.ToString();
            var canAcceptCal = (numOfSegs >= 2);
            RotationOffsetZValue_statusLabel.BackColor = canAcceptCal ? Color.Green : Color.Red;
        }

        private void acceptButton_Click(object sender, EventArgs e)
        {
            if (!can_accept)
            {
                var acceptSegment = MessageBox.Show("Segment is not valid! Do you want to validate it anyway?", "Invalid segment", MessageBoxButtons.YesNo) == DialogResult.Yes;
                if (!acceptSegment)
                {
                    return;
                }
            }

            string path = "c:/MASDATA/" + GetDiscID() + "/" + System.DateTime.Now.ToString("yyyy_MM_dd") + "/";
            PrintAcceptFineData(path);

            string fn = path + "image_";
            var now = DateTime.Now;
            fn += now.ToShortDateString() + "_" + now.ToShortTimeString().Replace(':', '-') + "_";
            fn += valueSegmentNumberTextbox + ".bmp";
            if (combinedImagePB.Image != null) combinedImagePB.Image.Save(fn);
            Alignment = "checkcalibrate";
            tabControl1.SelectedIndex = 0;
        }

        private void PrintAcceptFineData(string path)
        {
            path = path + "accept_data.csv";
            var newFile = !File.Exists(path);

            var segment = GetSegmentNumber();
            var tanOffset = lbl_tan_ofs.Text;
            var radOffset = lbl_rad_ofs.Text;
            var isOk = Convert.ToInt32(can_accept);
            var time = DateTime.Now.ToString("HH:mm:ss");
            using (var streamWriter = new StreamWriter(path, append: !newFile))
            {
                if (newFile)
                {
                    streamWriter.Write(string.Format("{0};{1};{2};{3};{4};\r\n", "segment", "Ok", "tan offset", "rad offset", "time"));

                }
                streamWriter.Write(string.Format("{0};{1};{2};{3};{4};\r\n", segment, isOk, tanOffset, radOffset, time));
            }
        }

        private string folder_path = ".";

        private void pathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowNewFolderButton = true;
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                folder_path = folderBrowserDialog1.SelectedPath;
            }
        }

        private void sgbgFine_CheckedChanged(object sender, EventArgs e)
        {
            finePBshow = "sgbg";
            label2.Text = finePBshow;
        }

        private void bgFine_CheckedChanged(object sender, EventArgs e)
        {
            finePBshow = "bg";
            label2.Text = finePBshow;
        }

        private void sgFine_CheckedChanged(object sender, EventArgs e)
        {
            finePBshow = "sg";
            label2.Text = finePBshow;
        }

        public void SetExposureSlider(double f)
        {
            exposureSlider.Value = (int)(100 * f);
        }
        public double GetExposureSlider()
        {
            return exposureSlider.Value / 100.0;
        }

        private void CalBackCB_CheckedChanged(object sender, EventArgs e)
        {
            valueCalBackCheckState = CalBackCB.CheckState.ToString();
        }

        private void reduceExposureLarge_button_Click(object sender, EventArgs e)
        {
            exposureSlider.Value -= 20;
        }

        private void reduceExposureSmall_button_Click(object sender, EventArgs e)
        {
            exposureSlider.Value -= 5;

        }

        private void increaseExposureSmall_button_Click(object sender, EventArgs e)
        {
            exposureSlider.Value += 5;

        }

        private void increaseExposureLarge_button_Click(object sender, EventArgs e)
        {
            exposureSlider.Value += 20;

        }

        private void LoadData_button_Click(object sender, EventArgs e)
        {
            PopulateSegments();

        }

        private void PopulateSegments()
        {
            var file = GetFile();
            var updatedSegments = GetSegmentsFromFile(file);
            UpdateStatusOfSegments(updatedSegments);
        }

        private void UpdateStatusOfSegments(List<Segment> updatedSegments)
        {
            LoadedSegments = updatedSegments;
            Alignment = "UpdateStatusOfSegments";
        }

        private List<Segment> GetSegmentsFromFile(string file)
        {
            var result = new List<Segment>();
            using (var streamReader = new StreamReader(file))
            {
                var thisLine = streamReader.ReadLine();//header
                thisLine = streamReader.ReadLine();//first row of data
                while (thisLine != null)
                {
                    var separatedLine = thisLine.Split(new char[] { ';' });
                    if (separatedLine.Length != 6)
                    {
                        throw new Exception("Unexpected line from file: " + thisLine);
                    }

                    //Only add newest instance
					if (!result.Any(seg => seg.Id == separatedLine[0]))
                    {
                        result.Add(new Segment
                        {
                            Id = separatedLine[0],
                            Status = Convert.ToBoolean(int.Parse(separatedLine[1])),
                            OffsetTan = double.Parse(separatedLine[2]),
                            OffsetRad = double.Parse(separatedLine[3]),
                            OffsetX = 0,
                            OffsetY = 0
                        });
                    }

                    thisLine = streamReader.ReadLine();
                }
            }

            return result;
        }

        private string GetFile()
        {
            string result;
            string path = "c://MASDATA//" + GetDiscID() + "//" + System.DateTime.Now.ToString("yyyy_MM_dd") + "//";
            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = path;
            var dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                result = openFileDialog.FileName;
                if (!result.Contains("accept_data"))
                {
                    MessageBox.Show("incorrect chosen file!");
                }
                else
                {
                    return result;
                }

            }
            return "";

        }
    }

    public class Segment
    {
        public string Id { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public bool Status { get; set; }
        public double OffsetTan { get; set; }
        public double OffsetRad { get; set; }
    }
}

