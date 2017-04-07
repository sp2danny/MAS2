﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace MirrorAlignmentSystem
{
	/// <summary>
	///  This class is the main worker of the application
	/// </summary>
	public partial class Worker
	{
		/// <summary>
		/// The main GUI, the worker rules over the GUI
		/// </summary>
		MainWindow mainWindow;

		/// <summary>
		/// The class that controls the camera of this application
		/// </summary>
		CameraController cameraController;

		// a whole bush of liberated variables
		bool manualMeasureMode;
		bool blackBkgAquired;
		bool blackBkgAquiredUpDown;       
		bool segNotExist;
		bool manualOverviewUpdate;
		double[,] doublePoints;
		bool bitmapBkgAquired;
		bool leftBkgAquired;
		bool rightBkgAquired;
		bool upBkgAquired;
		bool downBkgAquired;
		int lifeTics;
		int programCycle;
		int blackBGCounter;
		int blackBGCounterLeftRight;
		int blackBGCounterUpDown;
		int dotWidth;
		int sensorHeight;
		bool offlineOnlineLastCycle;
		string alignmentLastCycle;
		bool leftRightUpDownLastCycle;
		bool leftRightCoarseAlignment;
		bool upDownCoarseAlignment;
		int coarseCounter;
		string segmentLastCycle;
		string segment;
		int[] AOIData;
		bool calibrateFirstCycle;
		float FPS;
		string[] cameraSettings;
		double exposureRate;
		double framerate;
		double threshold;
		int waitOnMonitor;
		int waitOnCycle;
		int xOffset;
		int yOffset;
		Bitmap cameraFineAlignBlack;
		Bitmap cameraFineAlignPattern;
		Bitmap combinedBitmap;
        Bitmap cameraOverview;
		Bitmap cameraCoarseAlignLeft;
		Bitmap cameraCoarseAlignRight;
		Bitmap cameraCoarseAlignUp;
		Bitmap cameraCoarseAlignDown;
		Bitmap cameraCoarseAlignBlackOne;
		Bitmap cameraCoarseAlignBlackTwo;
		Bitmap returnImg1;
		Bitmap returnImg2;
		Bitmap returnImg3;
		Bitmap returnImg4;
		Point massCenter = new Point(0,0);
        double[] offsetXY, offsetRT;
		MonitorHandler monitor;
		Stopwatch stopWatchProgram;
		Stopwatch stopWatchStartUp;
		Stopwatch stopWatchCamera;
		Stopwatch stopWatchAlgorithm;
		Stopwatch stopWatchBackgroundImage;
		Stopwatch stopWatchOverviewImage;
		Stopwatch stopWatchBlackBkgImage;
		Stopwatch stopWatchBitmapBkgImage;
		Stopwatch stopWatchCombinedImage;
		Stopwatch stopWatchImageStore;
		Stopwatch stopWatchUpdateGUI;
		Stopwatch stopWatchFirstIF;
		Stopwatch stopWatchFPS;
		bool cameraInitialized;

		bool offline;
		string alignmentMode;
		bool manualAlignment;



		/// <summary>
		///  This class constructor
		/// </summary>
		public Worker(MainWindow mainWindowInput, CameraController inputCamera) 
		{
			mainWindow = mainWindowInput;
			cameraController = inputCamera;
		}

		partial void ExecuteOverview();
		partial void ExecuteFine();
		partial void ExecuteCoarse();

		/// <summary>
		///  Entry point for the thread
		/// </summary>
		public void Run() 
		{

			// Set variables
			manualMeasureMode = true;
			// The application has aquired a image of the segment with a black image on the TV
			blackBkgAquired = false;
			blackBkgAquiredUpDown = false;
            
			segNotExist = true;

			manualOverviewUpdate = false;


			// The application has aquired a image of the segment with a bitmap pattern on the TV
			bitmapBkgAquired = false;

			// The application has aquired a image of the segment with a left bitmap pattern on the TV
			leftBkgAquired = false;

			// The application has aquired a image of the segment with a right bitmap pattern on the TV
			rightBkgAquired = false;

			// The application has aquired a image of the segment with a up bitmap pattern on the TV
			upBkgAquired = false;

			// The application has aquired a image of the segment with a down bitmap pattern on the TV
			downBkgAquired = false;

			// The life counter between the GUI-thread and the worker-thread
			lifeTics = 0;

			// Keeps track of the number of time the worker-thread have run it's cycle. This field is only used for debug reason as of 2016-04-29(Rev 63)
			programCycle = 1;

			// Keeps track of the number of cycles since a black image was displayed the on the TV
			blackBGCounter = 0;
			blackBGCounterLeftRight = 0;
			blackBGCounterUpDown = 0;

			dotWidth = 1;
			sensorHeight = 0;// 1200;

			// Keeps track of the offline/online mode to help the application notice when the user change mode.
			offlineOnlineLastCycle = false;

			// Keeps track of the fine/coarse alignment mode to help the application notice when the user change mode.
			alignmentLastCycle = "over";

			// Keeps track of the left-right/up-down mode during coarse alignment to help the application switch between the two.
			leftRightUpDownLastCycle = false;

			// Keeps track of the application is running left-right coarse alignment mode this cycle
			leftRightCoarseAlignment = true;

			// Keeps track of the application is running up-down coarse alignment mode this cycle
			upDownCoarseAlignment = false;

			// Keeps track of the application is running left-right coarse alignment mode this cycle
			coarseCounter = 0;

			// Keeps track of the segment the last cycle. This helps the application to determine if the user have switched segment. Switching is done manually
			// when this comment is written(Rev63 2016-04-29)
			segmentLastCycle = "";
			segment = mainWindow.GetSegmentNumber().Trim();
			AOIData = DAL.GetAOIData(segment);

			// Keeps track of the first cycle when the user wants to calibrate the alignment system. This is done to change the AOI of the camera and only done
			// the first cycle
			calibrateFirstCycle = true;

			// The camera settings the applications is using, taken from the Database
			cameraSettings = DAL.GetSettings("User");

			// The exposure rate, taken from the database
			exposureRate = 4.5 * double.Parse( cameraSettings[0] );

			framerate = double.Parse(cameraSettings[5]);

			//Threshold to be used in the algorithm
			threshold = double.Parse(cameraSettings[2]);

			// The wait time in ms between the image is sent to the monitor and when the camera starts to capture the image
			waitOnMonitor = int.Parse(cameraSettings[3]);

			// The wait time between the end of one cycle and the start of the next
			waitOnCycle = int.Parse(cameraSettings[4]);

			xOffset = int.Parse(cameraSettings[6]);
			yOffset = int.Parse(cameraSettings[7]);

			cameraFineAlignBlack = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			cameraFineAlignPattern = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			combinedBitmap = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            cameraOverview = new Bitmap(1960, 1216, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			cameraCoarseAlignLeft = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			cameraCoarseAlignRight = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			cameraCoarseAlignUp = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			cameraCoarseAlignDown = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			cameraCoarseAlignBlackOne = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			cameraCoarseAlignBlackTwo = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			returnImg1 = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			returnImg2 = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			returnImg3 = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			returnImg4 = new Bitmap(304, 164, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

			massCenter = new Point(0,0);

			//Creates a class that control the TV. This is the class that uses DirectX
			monitor = new MonitorHandler();

			//Different stopwatches to time different parts of the application
			stopWatchProgram = new Stopwatch();
			stopWatchStartUp = new Stopwatch();
			stopWatchCamera = new Stopwatch();
			stopWatchAlgorithm = new Stopwatch();
			stopWatchBackgroundImage = new Stopwatch();
			stopWatchOverviewImage = new Stopwatch();
			stopWatchBlackBkgImage = new Stopwatch();
			stopWatchBitmapBkgImage = new Stopwatch();
			stopWatchCombinedImage = new Stopwatch();
			stopWatchImageStore = new Stopwatch();
			stopWatchUpdateGUI = new Stopwatch();
			stopWatchFirstIF = new Stopwatch();
			stopWatchFPS = new Stopwatch();

			stopWatchFPS.Start();

			//  Camera initialized or not
			cameraInitialized = cameraController.Init();

			cameraController.SetExposureTime(exposureRate);
			cameraController.SetFramerate(framerate);

			//Creates a thread for the TV so that it updates as fast as possible
			Thread oThread = new Thread(new ThreadStart(monitor.Run));

			//Starts the thread for the TV

			oThread.SetApartmentState(ApartmentState.STA);
			oThread.Start();

			if (manualMeasureMode)
			{
				while (true)
				{
					stopWatchProgram.Reset();
					stopWatchProgram.Start();

					//Here is the wait time between each cycle. This one can be set in the GUI through the Option window
					Thread.Sleep(waitOnCycle);

					System.Diagnostics.Debug.WriteLine("Program cycle: " + programCycle);
					System.Diagnostics.Debug.WriteLine("---------------------------------");

					//Checks if the user has changed any settings since the last cycle. If this is the case then the settings are redownloaded from the server
					//and updated accordingly
					if (mainWindow.IsSettingsUpdated())
					{
						cameraSettings = DAL.GetSettings("User");
						threshold = double.Parse(cameraSettings[2]);
						waitOnMonitor = int.Parse(cameraSettings[3]);
						waitOnCycle = int.Parse(cameraSettings[4]);
						xOffset = int.Parse(cameraSettings[6]);
						yOffset = int.Parse(cameraSettings[7]);

						mainWindow.SettingsUpdated(false);
					}

					if (mainWindow.GetUpdateManualOverview())
					{
						manualOverviewUpdate = true;

						mainWindow.SetManualUpdateOverviewImg(false);
					}
					else
					{
						manualOverviewUpdate = false;
					}

					stopWatchStartUp.Reset();
					stopWatchStartUp.Start();

					//Checks if the user has changed any of the different modes during the last cycle
					offline = mainWindow.GetOfflineOnlineMode();
					alignmentMode = mainWindow.GetAlignmentMode();
					manualAlignment = mainWindow.GetManualAlignment();

					//User changed from one alignment mode to another, here the application resets everything it need to restart the algorithm
					if (alignmentMode != alignmentLastCycle)
					{
						System.Diagnostics.Debug.WriteLine("Fine/Coarse/Calibrate Alignment change!");
						bitmapBkgAquired = false;
						blackBkgAquired = false;
						blackBkgAquiredUpDown = false;
						leftBkgAquired = false;
						rightBkgAquired = false;
						upBkgAquired = false;
						downBkgAquired = false;

						blackBGCounter = 0;
						blackBGCounterLeftRight = 0;
						blackBGCounterUpDown = 0;
						coarseCounter = 0;
					}

					//Segment has been changed since the last cycle
					if (String.Compare(segment, segmentLastCycle) != 0)
					{
						bitmapBkgAquired = false;
						blackBkgAquired = false;
						blackBkgAquiredUpDown = false;
						leftBkgAquired = false;
						rightBkgAquired = false;
						upBkgAquired = false;
						downBkgAquired = false;

						leftRightCoarseAlignment = true;
						blackBGCounter = 0;
						blackBGCounterLeftRight = 0;
						blackBGCounterUpDown = 0;
						coarseCounter = 0;
					}

					//Here the program checks if the user from offline to online or the otherway around, here the application resets everything it need to restart the algorithm
					if (offline != offlineOnlineLastCycle)
					{
						System.Diagnostics.Debug.WriteLine("Offline/Online change!");
						bitmapBkgAquired = false;
						blackBkgAquired = false;
						blackBkgAquiredUpDown = false;
						leftBkgAquired = false;
						rightBkgAquired = false;
						upBkgAquired = false;
						downBkgAquired = false;

						blackBGCounter = 0;
						blackBGCounterLeftRight = 0;
						blackBGCounterUpDown = 0;
						coarseCounter = 0;
					}

					lifeTics++;

					stopWatchStartUp.Stop();

					//Makes sure that an overview image is taken by the camera every second.
					stopWatchFirstIF.Reset();
					stopWatchFirstIF.Start();

					//Calibrate mode and live video is being displayed in the GUI
					if (alignmentMode == "calibrate")
					{
						monitor.SetCalibrateOrNot(true);

						if (calibrateFirstCycle)
						{
                            cameraController.SetAOI(1936, 1216, 0, 0);
							cameraController.SetExposureTime(exposureRate);

							calibrateFirstCycle = false;
						}

						doublePoints = new double[4, 2];

						//Covers the whole screen
						doublePoints[0, 0] = -1;
						doublePoints[0, 1] = -1;

						doublePoints[1, 0] = 1;
						doublePoints[1, 1] = -1;

						doublePoints[2, 0] = -1;
						doublePoints[2, 1] = 1;

						doublePoints[3, 0] = 1;
						doublePoints[3, 1] = 1;

						monitor.UpdatePatternVertices(doublePoints, true);

						//Makes sure the camera is initialized before trying to aquire a image from the CameraController class
						if (cameraInitialized)
						{

                            cameraOverview = new Bitmap(cameraController.AquisitionVideo(1936));
                            //Find segment centerpoint and blackhole, draw it and show the value in the gui
                            double[] segmentCenterPoint = new double[2];
                            double[,] segmentCenterPoints = new double[4, 2];
                            double[] segmentCenterOffsetPoint = new double[2];
                            double[,] segmentCenterOffsetPoints = new double[4, 2];
                            
                            // Find reference points on segments
                            int ticker = 0;
                            string[] nokSegs = {"ok", "ok", "ok", "ok"};
                            foreach (string segmentsForCalibration in Calibrate.refSegments)
                            {
                                Calibrate.mirrorCoM(cameraOverview, segmentsForCalibration, out segmentCenterPoint, out segmentCenterOffsetPoint);
                                segmentCenterOffsetPoints[ticker, 0] = segmentCenterOffsetPoint[0];
                                segmentCenterOffsetPoints[ticker, 1] = segmentCenterOffsetPoint[1];
                                segmentCenterPoints[ticker, 0] = segmentCenterPoint[0];
                                segmentCenterPoints[ticker, 1] = segmentCenterPoint[1];
                                if (segmentCenterPoints[ticker, 0] == 1)
                                {
                                    nokSegs[ticker] = segmentsForCalibration;
                                }
                                ticker++;
                            }
                                                            
                            
                            double[] blackholePoint;
                            double[] blackholeOffsetPoint = new double[2];
                            Calibrate.findBlackHole(cameraOverview, out blackholePoint, out blackholeOffsetPoint);

                            double rotZ;
                            Calibrate.rotationZ(segmentCenterOffsetPoints, out rotZ);

                            //mainWindow.UpdateSegmentLabel(segmentCenterOffsetPoint);
                            //mainWindow.UpdateblackholeLabel(blackholeOffsetPoint);

                            // Draw the crosses
                            Bitmap tempBitmapCrosses = new Bitmap(cameraOverview);
                            Calibrate.drawCrosses(out tempBitmapCrosses, cameraOverview, nokSegs, segmentCenterPoints, blackholePoint);

                            // Draw segments in red
                            Bitmap tempBitmapSegments = Calibrate.drawSegments(tempBitmapCrosses);

                            if (cameraOverview != null)
                            {
                                cameraOverview.Dispose();
                            }
                            cameraOverview = new Bitmap(tempBitmapSegments);
                            tempBitmapSegments.Dispose();
                            tempBitmapCrosses.Dispose();
                        }

						//Displays the live video in the overview picturebox in the GUI
                        mainWindow.ShowCalibrateBitmap(cameraOverview);
                        //mainWindow.ShowOverviewHistogram(cameraOverview);

					}
					if(alignmentMode == "over")
					{
						ExecuteOverview();
                    }
                    stopWatchFirstIF.Stop();
                    System.Diagnostics.Debug.WriteLine("First IF time elapsed: " + stopWatchFirstIF.ElapsedMilliseconds + "ms");
                    System.Diagnostics.Debug.WriteLine(alignmentMode);
					if (!segNotExist)
					{
						//Fine alignment mode is active 
						if (alignmentMode == "fine")
						{
							ExecuteFine();
						} 
                        else if (alignmentMode == "coarse")
						{
							ExecuteCoarse();
						}
                        else if(alignmentMode == "checkALLfine")
                        {
                            double[,] fineData;
                            Bitmap overviewFine;
							var caf = mainWindow.getCAF();
                            Algorithm.checkAllSegmentsFine(cameraController, cameraSettings, monitor, caf, out fineData, out overviewFine);
							alignmentMode = "over";
                        }
                        else if(alignmentMode == "checkALLcoarse")
                        {
                            double[,] coarseData;
                            Bitmap coarseY, coarseX;
                            var caf = mainWindow.getCAF();


                        }
					}

                    blackBkgAquired = false;
					stopWatchUpdateGUI.Reset();
					stopWatchUpdateGUI.Start();

					mainWindow.UpdateGUI(lifeTics);

					stopWatchUpdateGUI.Stop();
					System.Diagnostics.Debug.WriteLine("UpdateGUI time elapsed: " + stopWatchUpdateGUI.ElapsedMilliseconds + "ms");

					offlineOnlineLastCycle = offline;
					alignmentLastCycle = alignmentMode;
					segmentLastCycle = mainWindow.GetSegmentNumber().Trim();
					System.Diagnostics.Debug.WriteLine("SegmentLastCycle: " + segmentLastCycle);

					stopWatchProgram.Stop();

					FPS = (float)Math.Round((decimal)(1000.0f / stopWatchProgram.ElapsedMilliseconds), 1);

					//Makes surethe FPS is not updated to often
					if (stopWatchFPS.ElapsedMilliseconds > 1000)
					{
						//mainWindow.UpdateFPS(FPS);

						stopWatchFPS.Restart();
					}

					System.Diagnostics.Debug.WriteLine("Programcycle total time: " + stopWatchProgram.ElapsedMilliseconds + "ms");
					System.Diagnostics.Debug.WriteLine("");

					programCycle++;
				}
			}
			else
			{
				MessageBox.Show("This will align the system");
				Thread.Sleep(waitOnCycle);

				// Reset variables
				manualMeasureMode = true;
				bitmapBkgAquired = false;
				blackBkgAquired = false;
				blackBkgAquiredUpDown = false;
				leftBkgAquired = false;
				rightBkgAquired = false;
				upBkgAquired = false;
				downBkgAquired = false;

				blackBGCounter = 0;
				blackBGCounterLeftRight = 0;
				blackBGCounterUpDown = 0;
				coarseCounter = 0;

				segment = mainWindow.GetSegmentNumber().Trim();
				//Gets the Area Of Interest for the segment(currently choosen in the GUI) from the database
				AOIData = DAL.GetAOIData(segment);
				AOIData[1] = sensorHeight - AOIData[1];
				AOIData[5] = sensorHeight - AOIData[5];
				AOIData[7] = sensorHeight - AOIData[7];
				AOIData[9] = sensorHeight - AOIData[9];
				AOIData[11] = sensorHeight - AOIData[11];


				//Start fine alignment
				//radial
				//Tangentiell
			}
		}

	}
}