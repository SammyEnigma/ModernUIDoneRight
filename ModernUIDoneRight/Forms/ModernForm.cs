﻿using NickAc.ModernUIDoneRight.Controls;
using NickAc.ModernUIDoneRight.Native;
using NickAc.ModernUIDoneRight.Objects;
using NickAc.ModernUIDoneRight.Objects.Interaction;
using NickAc.ModernUIDoneRight.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NickAc.ModernUIDoneRight.Forms
{
    public class ModernForm : Form
    {

        #region Mouse-Resize Anywhere 
        bool mouseChanged = false;
        void MouseMoveEvent(object sender, MouseEventArgs e)
        {
            //Get the control
            Control c = ((Control)sender);

            //Check if window is maximized, if it is, stop!
            if (!(WindowState != FormWindowState.Maximized)) return;
            //Try to see where the mouse was
            var result = HitTest(e.Location, c.Location);
            var cur = FormUtils.HitTestToCursor(FormUtils.ConvertToResizeResult(result));
            //Change the mouse accordingly
            if (!c.Cursor.Equals(cur) && mouseChanged) {
                c.Cursor = cur;
                mouseChanged = !cur.Equals(Cursors.Default);
            }
        }

        void HandleMouseEventHandler(object sender, MouseEventArgs e)
        {
            //Get the control
            Control c = (Control)sender;
            //Check if the window isn't and there isn't more than one click
            if ((WindowState != FormWindowState.Maximized) && e.Clicks != 1) return;
            //Check where clicked
            var result = HitTest(e.Location, c.Location);
            //Invoke method that will start firm resizing
            if (Sizable)
                FormUtils.StartFormResizeFromEdge(this, FormUtils.ConvertToResizeResult(result), c);
        }

        public void HandleMouseMoveAndChild(Control c)
        {
            //Listen to mouse events
            c.MouseDown += HandleMouseEventHandler;
            c.MouseMove += MouseMoveEvent;
            foreach (Control c2 in c.Controls) {
                //Do the same for child controls
                //(Recursive method call)
                HandleMouseMoveAndChild(c2);
            }
        }
        public void UnhandleMouseMoveAndChild(Control c)
        {
            //Remove mouse events listeners
            c.MouseDown -= HandleMouseEventHandler;
            c.MouseMove -= MouseMoveEvent;
            foreach (Control c2 in c.Controls) {
                //Do the same for child controls
                //(Recursive method call)
                UnhandleMouseMoveAndChild(c2);
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            //Detect when control is added and handle form resizing
            HandleMouseMoveAndChild(e.Control);
        }


        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            //Detect when control is removed and remove all mouse listeners
            UnhandleMouseMoveAndChild(e.Control);
        }
        #endregion

        #region Global Variables
        /// <summary>
        /// The default height of the titlebar
        /// </summary>
        public const int DEFAULT_TITLEBAR_HEIGHT = 32;
        /// <summary>
        /// The size of the border
        /// </summary>
        private const int SIZING_BORDER = 7;

        private readonly List<ModernTitlebarButton> titlebarButtons, nativeTitlebarButtons;
        WindowHitTestResult windowHit = WindowHitTestResult.None;
        ColorScheme colorScheme;
        #endregion

        #region Constructor
        public ModernForm()
        {
            ColorScheme = DefaultColorSchemes.Blue;

            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            titlebarButtons = new List<ModernTitlebarButton>();
            nativeTitlebarButtons = GenerateNativeButtons(DEFAULT_TITLEBAR_HEIGHT, this);

            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
        }
        #endregion

        #region Properties
        public ShadowType ShadowType { get; set; } = ShadowType.Default;
        public bool TitlebarVisible { get; set; } = true;
        public bool Sizable { get; set; } = true;

        public List<ModernTitlebarButton> TitlebarButtons => titlebarButtons;

        private List<ModernTitlebarButton> NativeTitlebarButtons => nativeTitlebarButtons;
        /// <summary>
        /// The actual height of the titlebar
        /// </summary>
        public int TitlebarHeight { get; set; } = DEFAULT_TITLEBAR_HEIGHT;
        /// <summary>
        /// The color scheme on this window
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ColorScheme ColorScheme {
            get { return colorScheme; }
            set {
                colorScheme = value;
                Refresh();
            }
        }
        void ResetColorScheme()
        {
            ColorScheme = DefaultColorSchemes.Blue;
        }

        /* Rectangles used to allow window resizing */
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Rectangle LeftSide => Rectangle.FromLTRB(0, TitlebarRectangle.Bottom, SIZING_BORDER, FormBounds.Bottom - SIZING_BORDER);
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Rectangle LeftBottom => Rectangle.FromLTRB(0, FormBounds.Bottom - SIZING_BORDER, SIZING_BORDER, FormBounds.Bottom);
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Rectangle BottomSide => Rectangle.FromLTRB(SIZING_BORDER, FormBounds.Bottom - SIZING_BORDER, FormBounds.Right - SIZING_BORDER, FormBounds.Bottom);
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Rectangle RightBottom => Rectangle.FromLTRB(FormBounds.Right - SIZING_BORDER, FormBounds.Bottom - SIZING_BORDER, FormBounds.Right, FormBounds.Bottom);
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Rectangle RightSide => Rectangle.FromLTRB(FormBounds.Right - SIZING_BORDER, TitlebarRectangle.Bottom, FormBounds.Right, FormBounds.Bottom - SIZING_BORDER);
        /* Rectangles used to allow window resizing */

        /// <summary>
        /// The form bounds rectangle relative to 0,0
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Rectangle FormBounds => new Rectangle(Point.Empty, Size);
        /// <summary>
        /// Rectangle that represents the complete titlebar
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Rectangle TitlebarRectangle => Rectangle.FromLTRB(1, 1, FormBounds.Right - 1, TitlebarVisible ? TitlebarHeight + 1 : 0);
        /// <summary>
        /// Rectangle that represents all caption/titlebar buttons
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Rectangle TitlebarButtonsRectangle {
            get {
                var btnWidth = GetTitleBarButtonsWidth();
                var titlebarRect = TitlebarRectangle;
                return Rectangle.FromLTRB(titlebarRect.Right - btnWidth, titlebarRect.Top, titlebarRect.Right, titlebarRect.Bottom);
            }
        }
        /// <summary>
        /// Rectangle that represents the titlebar text
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Rectangle TextBarRectangle => Rectangle.FromLTRB(TitlebarRectangle.Left, TitlebarRectangle.Top, TitlebarRectangle.Right - GetTitleBarButtonsWidth(), TitlebarRectangle.Bottom);
        /// <summary>
        /// The font used in the titlebar
        /// </summary>
        public Font TitleBarFont { get; set; } = SystemFonts.CaptionFont;
        #endregion

        #region Methods

        new void ResetBackColor()
        {
            BackColor = Color.White;
        }

        private int HandleTitlebarButtonClick(MouseEventArgs e, ref int titlebarButtonOffset, List<ModernTitlebarButton> buttons)
        {
            for (int i = 0; i < buttons.Count; i++) {
                var btn = buttons[i];
                if (!btn.Visible) continue;
                Rectangle rect = GetTitlebarButtonRectangle(titlebarButtonOffset, btn);
                titlebarButtonOffset += btn.Width;
                if (rect.Contains(e.Location)) {
                    btn.OnClick(e);
                }
            }

            return titlebarButtonOffset;
        }

        private int RenderTitlebarButtons(PaintEventArgs e, Point curLoc, SolidBrush secondary, List<ModernTitlebarButton> buttons, ref int titlebarButtonOffset)
        {
            for (int i = 0; i < buttons.Count; i++) {
                var btn = buttons[i];
                if (!btn.Visible) continue;
                Rectangle rect = GetTitlebarButtonRectangle(titlebarButtonOffset, btn);
                if (rect.Contains(curLoc))
                    e.Graphics.FillRectangle(secondary, rect);
                GraphicUtils.DrawCenteredText(e.Graphics, btn.Text, btn.Font, rect, ColorScheme.ForegroundColor);
                titlebarButtonOffset += btn.Width;
            }

            return titlebarButtonOffset;
        }

        /// <summary>
        /// Get total width of all the caption buttons
        /// </summary>
        /// <returns></returns>
        private int GetTitleBarButtonsWidth()
        {
            int titlebarButtonOffset = 0;
            for (int i = 0; i < NativeTitlebarButtons.Count; i++) {
                var btn = NativeTitlebarButtons[i];
                if (!btn.Visible) continue;
                Rectangle rect = GetTitlebarButtonRectangle(titlebarButtonOffset, btn);
                titlebarButtonOffset += btn.Width;
            }
            for (int i = 0; i < TitlebarButtons.Count; i++) {
                var btn = TitlebarButtons[i];
                if (!btn.Visible) continue;
                Rectangle rect = GetTitlebarButtonRectangle(titlebarButtonOffset, btn);
                titlebarButtonOffset += btn.Width;
            }
            return titlebarButtonOffset;
        }
        /// <summary>
        /// Get rectangle of a button
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="btn">The button</param>
        /// <returns></returns>
        private Rectangle GetTitlebarButtonRectangle(int offset, ModernTitlebarButton btn)
        {
            return Rectangle.FromLTRB(TitlebarRectangle.Right - btn.Width - offset, 0, TitlebarRectangle.Right - offset, TitlebarRectangle.Bottom);
        }
        /// <summary>
        /// This method adds the default titlebar buttons.
        /// </summary>
        /// <param name="width">Button width</param>
        /// <param name="parent">Form containing the buttons</param>
        /// <returns></returns>
        private static List<ModernTitlebarButton> GenerateNativeButtons(int width, Form parent)
        {

            //Create a temporary list
            List<ModernTitlebarButton> list = new List<ModernTitlebarButton>
            {
                new NativeTitlebarButton(parent, width, NativeTitlebarButton.TitlebarAction.Close),
                new NativeTitlebarButton(parent, width, NativeTitlebarButton.TitlebarAction.Maximize),
                new NativeTitlebarButton(parent, width, NativeTitlebarButton.TitlebarAction.Minimize)
            };

            return list;
        }


        public WindowHitTestResult HitTest(Point loc)
        {
            return HitTest(loc, Point.Empty);
        }


        public WindowHitTestResult HitTest(Point loc, Point offset)
        {
            Point negativeOffset = new Point(-offset.X, -offset.Y);
            if (TitlebarButtonsRectangle.OffsetAndReturn(negativeOffset).Contains(loc))
                return WindowHitTestResult.TitleBarButtons;

            if (TextBarRectangle.OffsetAndReturn(negativeOffset).Contains(loc))
                return WindowHitTestResult.TitleBar;

            if (!Sizable)
                return WindowHitTestResult.None;

            if (LeftBottom.OffsetAndReturn(negativeOffset).Contains(loc))
                return WindowHitTestResult.BottomLeft;

            if (LeftSide.OffsetAndReturn(negativeOffset).Contains(loc))
                return WindowHitTestResult.Left;

            if (BottomSide.OffsetAndReturn(negativeOffset).Contains(loc))
                return WindowHitTestResult.Bottom;

            if (RightBottom.OffsetAndReturn(negativeOffset).Contains(loc))
                return WindowHitTestResult.BottomRight;

            if (RightSide.OffsetAndReturn(negativeOffset).Contains(loc))
                return WindowHitTestResult.Right;

            return WindowHitTestResult.None;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsAppBarAvailable => Controls.OfType<AppBar>().Any();

        #endregion

        #region Overriden Methods
        public override string Text {
            get => base.Text; set {
                base.Text = value;
                Invalidate();
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            if (!DesignMode) {
                CenterToScreen();

                //Check if we can use the aero shadow
                if ((ShadowType.Equals(ShadowType.AeroShadow) || ShadowType.Equals(ShadowType.Default)) && DwmNative.ExtendFrameIntoClientArea(this, 0, 0, 0, 1)) {
                    //We can! Tell windows to allow the rendering to happen on our borderless form
                    DwmNative.AllowRenderInBorderless(this);
                }
                else if (ShadowType.Equals(ShadowType.Default) || ShadowType.Equals(ShadowType.FlatShadow)) {
                    //No aero for us! We must create the typical flat shadow.
                    new ShadowForm().Show(this);
                }
            }
            base.OnLoad(e);
            int v = GetTitleBarButtonsWidth();
            base.MinimumSize = new Size(v, TitlebarHeight + SIZING_BORDER);
        }

        public override Size MaximumSize {
            get {
                return Screen.GetWorkingArea(this).Size;
            }
            set {
                value = Screen.GetWorkingArea(this).Size;
                base.MaximumSize = value;
            }
        }

        public override Rectangle DisplayRectangle => Rectangle.FromLTRB(TitlebarRectangle.Left, TitlebarRectangle.Bottom, TitlebarRectangle.Right, FormBounds.Bottom - 1);

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (DesignMode) return;
            var hitResult = HitTest(e.Location);
            var resizeResult = FormUtils.ConvertToResizeResult(hitResult);
            Cursor cursor = FormUtils.HitTestToCursor(resizeResult);
            if (WindowState != FormWindowState.Maximized) {
                if (mouseChanged) {
                    base.Cursor = cursor;
                }
                mouseChanged = true /*!cursor.Equals(Cursors.Default)*/;
            }
            Invalidate(TitlebarButtonsRectangle);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (DesignMode) return;
            if (mouseChanged) {
                Cursor = Cursors.Default;
                mouseChanged = false;
            }
            Invalidate(TitlebarButtonsRectangle);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (DesignMode) return;
            if (mouseChanged) {
                Cursor = Cursors.Default;
                mouseChanged = false;
            }
            Invalidate(TitlebarButtonsRectangle);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Sizable) return;
            var hitResult = HitTest(e.Location);

            windowHit = hitResult;
            Invalidate(TitlebarButtonsRectangle);

            if (hitResult == WindowHitTestResult.TitleBar) {
                FormUtils.StartFormDragFromTitlebar(this);
            }
            else if (hitResult != WindowHitTestResult.TitleBarButtons && hitResult != WindowHitTestResult.None && Sizable) {
                FormUtils.StartFormResizeFromEdge(this, FormUtils.ConvertToResizeResult(hitResult));
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (mouseChanged) {
                Cursor = Cursors.Default;
                mouseChanged = false;
            }
            Invalidate(TitlebarButtonsRectangle);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (windowHit != WindowHitTestResult.None && windowHit != WindowHitTestResult.TitleBar && windowHit != WindowHitTestResult.TitleBarButtons && Sizable) {
                windowHit = WindowHitTestResult.None;
                return;
            }
            int titlebarButtonOffset = 0;
            switch (HitTest(e.Location)) {
                case WindowHitTestResult.TitleBarButtons:
                    titlebarButtonOffset = 0;
                    HandleTitlebarButtonClick(e, ref titlebarButtonOffset, NativeTitlebarButtons);
                    HandleTitlebarButtonClick(e, ref titlebarButtonOffset, TitlebarButtons);
                    break;
            }
        }


        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            Point curLoc = PointToClient(Cursor.Position);

            using (SolidBrush primary = new SolidBrush(ColorScheme.PrimaryColor)) {
                using (SolidBrush secondary = new SolidBrush(ColorScheme.SecondaryColor)) {
                    //Draw titlebar
                    if (TitlebarVisible)
                        e.Graphics.FillRectangle(primary, TitlebarRectangle);
                    //Draw form border
                    GraphicUtils.DrawRectangleBorder(FormBounds, e.Graphics, ColorScheme.SecondaryColor);

                    if (!TitlebarVisible)
                        return;
                    //Start rendering the titlebar buttons
                    int titlebarButtonOffset = 0;
                    titlebarButtonOffset = RenderTitlebarButtons(e, curLoc, secondary, NativeTitlebarButtons, ref titlebarButtonOffset);
                    titlebarButtonOffset = RenderTitlebarButtons(e, curLoc, secondary, TitlebarButtons, ref titlebarButtonOffset);
                    //Dectect if an app bar is available.
                    //If it is, draw the window title.
                    if (!IsAppBarAvailable) {
                        GraphicUtils.DrawCenteredText(e.Graphics, Text, TitleBarFont, Rectangle.FromLTRB(TextBarRectangle.Left + SIZING_BORDER, TextBarRectangle.Top, TextBarRectangle.Right - SIZING_BORDER, TextBarRectangle.Bottom), ColorScheme.ForegroundColor, false, true);
                    }
                }

            }
        }
        #endregion
    }
}