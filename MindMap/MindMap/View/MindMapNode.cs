﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MindMap.View
{
    public partial class MindMapNode : UserControl
    {
        public MindMapNode()
        {
            InitializeComponent();
        }

        #region 属性


        private MindMapNode _ParentNode = null;
        /// <summary> 设置或获取父节点
        /// 
        /// </summary>
        public MindMapNode ParentNode
        {
            set { _ParentNode = value; }
            get { return _ParentNode; }
        }

        private Font g_TextFont = new Font(new FontFamily("微软雅黑"), 12);
        /// <summary> 当前节点的字体
        /// 
        /// </summary>
        public Font TextFont
        {
            get
            {
                return g_TextFont;
            }
            set
            {
                g_TextFont = value;
                Content_lable.Font = g_TextFont;//设置字体
                ResetNodeSize();//重新设置节点尺寸

            }
        }

        private TreeNode _TreeNode = null;
        /// <summary> 设置当前节点的内容
        /// 
        /// </summary>
        public TreeNode TreeNode
        {
            set
            {
                if (value == null) return;
                _TreeNode = value;
                Content_lable.Text = _TreeNode.Text;
                Content_lable.Font = g_TextFont;
                Content_lable.ForeColor = Color.FromArgb(255, 255, 255);
                Content_lable.BackColor = _NodeBackColor.Normaly.Value;

                Chidren_Panel.Controls.Clear();
                foreach (TreeNode TreeNodeItem in _TreeNode.Nodes)
                {
                    MindMapNode MindMapNodeTemp = new MindMapNode();
                    MindMapNodeTemp.TextFont = g_TextFont;
                    MindMapNodeTemp.TreeNode = TreeNodeItem;
                    MindMapNodeTemp.Margin = new Padding(0, 2, 0, 2);
                    Chidren_Panel.Controls.Add(MindMapNodeTemp);
                }
                ReSetSize();
                DrawingConnectLine();
            }
        }

        private bool _Selected = false;
        /// <summary>当前节点是否被选中
        /// 
        /// </summary>
        public bool Selected
        {
            get { return _Selected; }
            set
            {
                _Selected = value;
            }
        }


        /// <summary> 设置当前节点的背景颜色
        /// 
        /// </summary>
        public MindMapNodeBackColor NodeBackColor { get { return _NodeBackColor; } set { _NodeBackColor = value; } }
        private MindMapNodeBackColor _NodeBackColor = new MindMapNodeBackColor(Color.FromArgb(48, 120, 215));


        #endregion 属性               

        #region 方法

        /// <summary> 再面板上画出当前节点的连接线
        /// 
        /// </summary>
        private void DrawingConnectLine()
        {
            int CurrentNodeHeightCenter = Content_Panel.Height / 2;
            Point StartPoint = new Point(0, CurrentNodeHeightCenter);

            Graphics LineGraphics = DrawingLine_panel.CreateGraphics();
            LineGraphics.Clear(DrawingLine_panel.BackColor);//清除之前的
            LineGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//开启抗锯齿

            Pen PenTemp = new Pen(Color.Black, 1);
            foreach (Control ControlItem in this.Chidren_Panel.Controls)
            {
                int TopTemp = ControlItem.Top + (ControlItem.Height / 2);
                Point PointTemp = new Point(this.DrawingLine_panel.Width, TopTemp);


                //LineGraphics.DrawLine(PenTemp, StartPoint, PointTemp);

                List<Point> PointArray = new List<Point>();
                PointArray.Add(StartPoint);
                PointArray.Add(new Point(StartPoint.X + DrawingLine_panel.Width / 2, StartPoint.Y));
                PointArray.Add(new Point(PointTemp.X - DrawingLine_panel.Width / 2, PointTemp.Y));

                PointArray.Add(PointTemp);
                LineGraphics.DrawCurve(PenTemp, PointArray.ToArray(), 0);
            }
        }

        /// <summary> 刷新单个节点的宽度和高度
        /// 
        /// </summary>
        private void ReSetSize()
        {
            Graphics g = this.CreateGraphics();
            g.PageUnit = GraphicsUnit.Display;
            SizeF ContentSize = g.MeasureString(Content_lable.Text, g_TextFont);
            Content_Panel.Width = Convert.ToInt32(ContentSize.Width + (ContentSize.Width * 0.15));

            int MaxChidrenWidth = 0;
            int HeightCount = 0;
            foreach (Control ControlItem in this.Chidren_Panel.Controls)
            {
                if (MaxChidrenWidth < ControlItem.Width) MaxChidrenWidth = ControlItem.Width;
                HeightCount += ControlItem.Height + 4;
            }

            MaxChidrenWidth = MaxChidrenWidth + DrawingLine_panel.Width + Content_Panel.Width;
            this.Width = MaxChidrenWidth;
            if (HeightCount < ContentSize.Height) HeightCount = Convert.ToInt32(ContentSize.Height);
            this.Height = HeightCount;

            Content_lable.Width = Content_Panel.Width + 10;
            Content_lable.Height = Convert.ToInt32(ContentSize.Height);
            Content_lable.Left = 0;
            Content_lable.Top = (Content_Panel.Height - Content_lable.Height) / 2;

        }

        /// <summary> 获取该节点下的子节点
        /// 
        /// </summary>
        /// <param name="IsAll">是否包含孙节点在内的所有子节点</param>
        /// <returns></returns>
        public List<MindMapNode> GetChidrenNode(bool IsAll = false)
        {
            List<MindMapNode> ResultList = new List<MindMapNode>();
            foreach (MindMapNode MindMapNodeItem in Chidren_Panel.Controls)
            {
                ResultList.Add(MindMapNodeItem);
                if (IsAll)
                {
                    ResultList.AddRange(MindMapNodeItem.GetChidrenNode(IsAll));
                }
            }
            return ResultList;
        }

        /// <summary> 递归设置子节点
        /// 
        /// </summary>
        /// <param name="FontSource"></param>
        public void SetTextFont(Font FontSource)
        {
            if (FontSource == null) return;
            g_TextFont = FontSource;
            Content_lable.Font = g_TextFont;
            GetChidrenNode(false).ForEach(T1 => T1.SetTextFont(FontSource));//递归将子节点也设置字体
            ReSetSize();
        }

        /// <summary> 递归向上设置父节点的尺寸
        /// 用于如果某节点修改了文本或字体，需要重新计算该节点的大小，其父节点的子节点容器也需要调节大小
        /// </summary>
        public void ResetNodeSize()
        {
            ReSetSize();
            if (this._ParentNode != null)
            {
                _ParentNode.ResetNodeSize();
            }
        }

        /// <summary> 添加一个节点
        /// 
        /// </summary>
        public void AddNode(TreeNode TreeNodeParame)
        {
            _TreeNode.Nodes.Add(TreeNodeParame);
            MindMapNode NewNode = new MindMapNode();
            NewNode.TextFont = g_TextFont;
            NewNode.TreeNode = TreeNodeParame;
            NewNode.Margin = new Padding(0, 2, 0, 2);
            Chidren_Panel.Controls.Add(NewNode);
            ResetNodeSize();
        }

        /// <summary> 移除一个节点
        /// 
        /// </summary>
        public void Remove(MindMapNode MindMapNodeParame)
        {

            MindMapNode MindMapNodeTemp = null;

            foreach (Control ControlItem in Chidren_Panel.Controls)
            {
                MindMapNodeTemp = (MindMapNode)ControlItem;
                if (MindMapNodeParame == MindMapNodeTemp)
                {
                    MindMapNodeTemp.Parent = null;
                }
            }
        }
        #endregion 方法

        #region 事件
        /// <summary> 重画时重新画出连接线
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MindMapNode_Paint(object sender, PaintEventArgs e)
        {
            DrawingConnectLine();

        }

        private void Content_lable_MouseEnter(object sender, EventArgs e)
        {
            Content_lable.BackColor = _NodeBackColor.Enter.Value;
        }

        private void Content_lable_MouseDown(object sender, MouseEventArgs e)
        {
            Content_lable.BackColor = _NodeBackColor.Down.Value;
        }

        private void Content_lable_MouseLeave(object sender, EventArgs e)
        {
            Content_lable.BackColor = _NodeBackColor.Normaly.Value;
        }

        private void Content_lable_MouseUp(object sender, MouseEventArgs e)
        {
            Content_lable.BackColor = _NodeBackColor.Normaly.Value;
        }

        #endregion 事件

        #region 配套使用的内部类
        /// <summary> 设置节点的背景色
        /// 
        /// </summary>
        public class MindMapNodeBackColor
        {
            /// <summary> 必须设置节点在正常时的背景颜色，如果其他颜色为空则，其他色在取值时会基于正常色自动给出缺省颜色
            /// 
            /// </summary>
            /// <param name="ColorParame"></param>
            public MindMapNodeBackColor(Color NormalyColor)
            {
                _Normaly = NormalyColor;
            }

            /// <summary>节点在正常时的背景颜色
            /// 
            /// </summary>
            public Color? Normaly { get { return _Normaly; } set { _Normaly = value; } }
            private Color? _Normaly = null;

            /// <summary> 节点在鼠标进入时的背景颜色 [如果为空取值时将取到比正常色稍亮一些的颜色]
            /// 
            /// </summary>
            public Color? Enter
            {
                get
                {
                    if (_Enter == null)
                    {
                        int IntRed = _Normaly.Value.R + 30;
                        int IntGreen = _Normaly.Value.G + 30;
                        int IntBlue = _Normaly.Value.B + 30;
                        IntRed = GetColorValue(IntRed);
                        IntGreen = GetColorValue(IntGreen);
                        IntBlue = GetColorValue(IntBlue);
                        _Enter = Color.FromArgb(IntRed, IntGreen, IntBlue);
                    }
                    return _Enter;
                }
                set { _Enter = value; }
            }
            private Color? _Enter = null;

            /// <summary> 节点在鼠标按下时的背景颜色 [如果为空取值时将取到比正常色稍暗一些的颜色]
            /// 
            /// </summary>
            public Color? Down
            {
                get
                {
                    if (_Down == null)
                    {
                        int IntRed = _Normaly.Value.R - 30;
                        int IntGreen = _Normaly.Value.G - 30;
                        int IntBlue = _Normaly.Value.B - 30;
                        IntRed = GetColorValue(IntRed);
                        IntGreen = GetColorValue(IntGreen);
                        IntBlue = GetColorValue(IntBlue);
                        _Down = Color.FromArgb(IntRed, IntGreen, IntBlue);
                    }
                    return _Down;


                }
                set { _Down = value; }
            }
            private Color? _Down = null;

            /// <summary> 限制int不能超过0-255的范围，超过最小值则取最小值超过最大值则取最大值
            /// 
            /// </summary>
            /// <param name="ColorValue"></param>
            /// <returns></returns>
            private int GetColorValue(int ColorValue)
            {
                int IntResult = ColorValue;
                IntResult = IntResult > 255 ? 255 : IntResult;//不能大于255，如果大于就取255
                IntResult = IntResult < 0 ? 0 : IntResult;//不能小于0，如果小于0那就取0
                return IntResult;
            }
        }


    }
    #endregion 配套使用的内部类
}

