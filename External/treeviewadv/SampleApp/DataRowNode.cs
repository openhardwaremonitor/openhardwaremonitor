using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Aga.Controls.Tree;
using System.Drawing;

namespace SampleApp
{
    public class DataRowNode
    {
        public DataRowNode(DataRow row, string text)
        {
            m_row = row;
            m_text = text;
        }

        string m_text;
        public string Text
        {
            get { return m_text; }
            set { m_text = value;}
        }

        private Image _icon;
        public Image Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        private DataRow m_row;

        public DataRow Row
        {
            get { return m_row; }
            set { m_row = value; }
        }
    }
}
