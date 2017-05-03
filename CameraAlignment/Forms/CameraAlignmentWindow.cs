﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using MirrorAlignmentSystem;

namespace CameraAlignment
{
	public partial class CameraAlignmentWindow : Form
	{
		private uEye.Camera _camera;
		IntPtr displayHandle = IntPtr.Zero;
		private System.Drawing.Color m_OverlayColor = System.Drawing.Color.Black;

		/// <summary>
		/// The class constructor
		/// </summary>
		public CameraAlignmentWindow()
		{
			InitializeComponent();
		}

		private void InitCamera(object sender, EventArgs e)
		{
			bool bDirect3D = false;
			bool bOpenGL = false;

			uEye.Defines.Status statusRet;
			_camera = new uEye.Camera();

			// open first available camera
			statusRet = _camera.Init(0, CameraAlignment_pictureBox.Handle.ToInt32());
			if (statusRet == uEye.Defines.Status.SUCCESS)
			{
				uEye.Defines.DisplayMode supportedMode;
				statusRet = _camera.DirectRenderer.GetSupported(out supportedMode);

				if ((supportedMode & uEye.Defines.DisplayMode.Direct3D) == uEye.Defines.DisplayMode.Direct3D)
				{
					bDirect3D = true;
				}
				else
				{
					bDirect3D = false;
				}

				if ((supportedMode & uEye.Defines.DisplayMode.OpenGL) == uEye.Defines.DisplayMode.OpenGL)
				{
					bOpenGL = true;
				}
				else
				{
					bOpenGL = false;
				}

				if (((supportedMode & uEye.Defines.DisplayMode.Direct3D) == uEye.Defines.DisplayMode.Direct3D) ||
					((supportedMode & uEye.Defines.DisplayMode.OpenGL) == uEye.Defines.DisplayMode.OpenGL))
				{

					if (bOpenGL == true)
					{
						// set display mode
						statusRet = _camera.Display.Mode.Set(uEye.Defines.DisplayMode.OpenGL);
					}

					if (bDirect3D == true)
					{
						// set display mode
						statusRet = _camera.Display.Mode.Set(uEye.Defines.DisplayMode.Direct3D);
					}

					// start live
					_camera.Acquisition.Capture();
					_camera.DirectRenderer.EnableScaling();

					// update information
					UpdateOverlayInformation();
					UpdateImageInformation();

					// set default key color
					m_OverlayColor = System.Drawing.Color.Black; //TODO: what does this do?
				}
				else
				{
					MessageBox.Show("Direct3D and OpenGL are not supported");
					Close();
				}
			}
			else
			{
				MessageBox.Show("Start Live Video failed");

			}

			// Connect Event
			_camera.EventFrame += onFrameEvent;
		}

		private void onFrameEvent(object sender, EventArgs e)
		{
			uEye.Camera Camera = sender as uEye.Camera;

			Int32 s32MemID;
			Camera.Memory.GetActive(out s32MemID);

			//TODO: displayHandle is never set. What is it used for?

			Camera.Display.Render(s32MemID, displayHandle, uEye.Defines.DisplayRenderMode.FitToWindow);

		}

		//TODO: not used?
		private void UpdateOverlayInformation()
		{
			uEye.Types.Size<UInt32> overlaySize;
			uEye.Defines.Status statusRet;

			statusRet = _camera.DirectRenderer.Overlay.GetSize(out overlaySize);
		}

		//TODO: not used?
		private void UpdateImageInformation()
		{
			/* open the camera */
			System.Drawing.Rectangle imageRect;
			uEye.Defines.Status statusRet;

			statusRet = _camera.Size.AOI.Get(out imageRect);
		}

		private void SetCrosshair_button_Click(object sender, EventArgs e)
		{
			uEye.Defines.Status statusRet;
			statusRet = _camera.DirectRenderer.Overlay.Clear();

			// get graphics
			Graphics graph;
			_camera.DirectRenderer.Overlay.GetGraphics(out graph);


			uEye.Types.Size<UInt32> overlaySize;
			statusRet = _camera.DirectRenderer.Overlay.GetSize(out overlaySize);

			var overlayCenter = new Point((int)overlaySize.Width / 2, (int)overlaySize.Height / 2);
			var xOffset = GetXOffset();
			var yOffset = GetYOffset();

			var overlayOffset = new Point(overlayCenter.X - (int)xOffset, overlayCenter.Y - (int)yOffset);

			DrawOffsetCrosshair(graph, overlayOffset);
			DrawCenterReferenceCircle(graph, overlayCenter);

			// show overlay
			_camera.DirectRenderer.Overlay.Show();
		}

		private void DrawCenterReferenceCircle(Graphics graph, Point center)
		{
			const int circleRadius = 20;
			
			var bluePen = new Pen(Color.Blue, 5);

			graph.DrawEllipse(bluePen, center.X-circleRadius/2, center.Y-circleRadius/2, circleRadius, circleRadius);
		}

		private void DrawOffsetCrosshair(Graphics graph, Point overlayOffset)
		{
			const int crosshairLineLength = 40;
			const int crosshairCenterSpace = 30;
			const int crosshairCenterSpace_half = crosshairCenterSpace / 2;

			var redPen = new Pen(Color.Red, 7);

			//Horizontal
			graph.DrawLine(redPen,
				new Point(overlayOffset.X - (crosshairLineLength + crosshairCenterSpace_half), overlayOffset.Y),
				new Point(overlayOffset.X - crosshairCenterSpace_half, overlayOffset.Y));
			graph.DrawLine(redPen,
				new Point(overlayOffset.X + (crosshairLineLength + crosshairCenterSpace_half), overlayOffset.Y),
				new Point(overlayOffset.X + crosshairCenterSpace_half, overlayOffset.Y));

			//Vertical
			graph.DrawLine(redPen,
				new Point(overlayOffset.X, overlayOffset.Y - (crosshairLineLength + crosshairCenterSpace_half)),
				new Point(overlayOffset.X, overlayOffset.Y - crosshairCenterSpace_half));
			graph.DrawLine(redPen,
				new Point(overlayOffset.X, overlayOffset.Y + (crosshairLineLength + crosshairCenterSpace_half)),
				new Point(overlayOffset.X, overlayOffset.Y + crosshairCenterSpace_half));		
		}

		private double GetXOffset()
		{
			const double pixelLength_mrad_per_pixel = 0.71;

			var xOffset_mm = double.Parse(xOffsetTB.Text);
			//var yOffset_mm = double.Parse(yOffsetTB.Text);
			var zDistance_mm = double.Parse(zOffsetTB.Text);

			return ((xOffset_mm / zDistance_mm) / pixelLength_mrad_per_pixel) * 1000;
		}

		private double GetYOffset()
		{
			const double pixelLength_mrad_per_pixel = 0.71;

			//var xOffset_mm = double.Parse(xOffsetTB.Text);
			var yOffset_mm = double.Parse(yOffsetTB.Text);
			var zDistance_mm = double.Parse(zOffsetTB.Text);

			return ((yOffset_mm / zDistance_mm) / pixelLength_mrad_per_pixel) * 1000;
		}
	}
}