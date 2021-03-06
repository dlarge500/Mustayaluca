﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace MustayalucaEditor
{
    public partial class SubItemProperties : Form
    {

        public string baseName = null;
        public int initialIndex = -1;
        public string currentSkinID = string.Empty;

        public SubItemProperties(bool showHyperlinkParameterDialog, string skinFileID)
        {
            InitializeComponent();
            gbHyperlinkParameter.Enabled = false;
            currentSkinID = skinFileID;

            if (showHyperlinkParameterDialog)
                gbHyperlinkParameter.Enabled = true;

            cbOnlineVideosReturn.Visible = false;

            switch (skinFileID)
            {
              case formMustayalucaEditor.tvseriesSkinID:
                foreach (KeyValuePair<string, string> tvv in formMustayalucaEditor.tvseriesViews)
                {
                  cboViews.Items.Add(tvv.Value);
                }
                break;
              case formMustayalucaEditor.musicSkinID:
                foreach (KeyValuePair<string, string> mvv in formMustayalucaEditor.musicViews)
                {
                  cboViews.Items.Add(mvv.Value);
                }
                break;
              case formMustayalucaEditor.onlineVideosSkinID:
                cbOnlineVideosReturn.Visible = true;
                foreach (KeyValuePair<string, string> mvv in formMustayalucaEditor.onlineVideosViews)
                {
                  cboViews.Items.Add(mvv.Value);
                }
                break;
            }
        }

        public string BaseName
        {
            set { baseName = value; }
        }

        public int InitialIndex
        {
            set { initialIndex = value; }
        }

        public string DisplayName
        {
            get
            {
                return tbItemDisplayName.Text;
            }
            set
            {
                tbItemDisplayName.Text = value;
            }
        }

        public string HypelinkParameterName
        {
            get
            {
                return cboViews.Text;
            }
            set
            {
                cboViews.Text = value;
            }
        }

        public string onlineVideosReturnOption
        {
          get
          {
            if (cbOnlineVideosReturn.Checked)
              return "Root";
            else
              return "Locked";
          }
          set
          {
            if (string.IsNullOrEmpty(value))
              cbOnlineVideosReturn.Checked = false;
            else
            {
              if (value.ToString().ToLower() == "root")
                cbOnlineVideosReturn.Checked = true;
              else
                cbOnlineVideosReturn.Checked = false;
            }
          }
        }


        public string tvseriesHypelinkParameter
        {
            get
            {
                if (formMustayalucaEditor.tvseriesViews.Count == 0)
                    return cboViews.Text;

                foreach (KeyValuePair<string, string> tvv in formMustayalucaEditor.tvseriesViews)
                {
                    if (tvv.Value == cboViews.Text)
                        return tvv.Key;
                }
                return "false";
            }

            set
            {
                if (formMustayalucaEditor.tvseriesViews.Count == 0)
                    cboViews.Text = value;
                int i = 0;
                foreach (KeyValuePair<string, string> tvv in formMustayalucaEditor.tvseriesViews)
                {
                    if (value == tvv.Key)
                    {
                        cboViews.Text = tvv.Value;
                        initialIndex = i;
                        break;
                    }
                    i++;
                }
            }
        }

        public string musicHypelinkParameter
        {
          get
          {
            if (formMustayalucaEditor.musicViews.Count == 0)
              return cboViews.Text;

            foreach (KeyValuePair<string, string> tvv in formMustayalucaEditor.musicViews)
            {
              if (tvv.Value == cboViews.Text)
                return tvv.Key;
            }
            return "false";
          }

          set
          {
            if (formMustayalucaEditor.musicViews.Count == 0)
              cboViews.Text = value;
            int i = 0;
            foreach (KeyValuePair<string, string> tvv in formMustayalucaEditor.musicViews)
            {
              if (value == tvv.Key)
              {
                cboViews.Text = tvv.Value;
                initialIndex = i;
                break;
              }
              i++;
            }
          }
        }

        public string onlineVideosHypelinkParameter
        {
          get
          {
            if (formMustayalucaEditor.onlineVideosViews.Count == 0)
              return cboViews.Text;

            foreach (KeyValuePair<string, string> tvv in formMustayalucaEditor.onlineVideosViews)
            {
              if (tvv.Value == cboViews.Text)
                return tvv.Key;
            }
            return "false";
          }

          set
          {
            if (formMustayalucaEditor.onlineVideosViews.Count == 0)
              cboViews.Text = value;
            int i = 0;
            foreach (KeyValuePair<string, string> tvv in formMustayalucaEditor.onlineVideosViews)
            {
              if (value == tvv.Key)
              {
                cboViews.Text = tvv.Value;
                initialIndex = i;
                break;
              }
              i++;
            }
          }
        }

        private void tbItemDisplayName_TextChanged(object sender, EventArgs e)
        {
            int start = tbItemDisplayName.SelectionStart;
            if (isIlegalXML(tbItemDisplayName.Text))
            {
                tbItemDisplayName.Text = tbItemDisplayName.Text.Substring(0, tbItemDisplayName.Text.Length - 1);
                tbItemDisplayName.SelectionStart = start;
                return;
            }
            tbItemDisplayName.Text = tbItemDisplayName.Text.ToUpper();
            tbItemDisplayName.SelectionStart = start;
        }

        bool isIlegalXML(string theValue)
        {
            Match m = formMustayalucaEditor.isIleagalXML.Match(theValue);
            return m.Success;
        }

        private void tbItemDisplayName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.Hide();

            if (e.KeyCode == Keys.Escape)
            {
                tbItemDisplayName.Text = string.Empty;
                this.Hide();
            }
        }

        private void btSaveAndClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btClearParameter_Click(object sender, EventArgs e)
        {
            cboViews.Text = string.Empty;
        }


        private void cboViews_SelectedIndexChanged(object sender, EventArgs e)
        {
          if (initialIndex != -1 && (tbItemDisplayName.Text == baseName || initialIndex != cboViews.SelectedIndex))
          {
            //TVSeries
            if (currentSkinID == formMustayalucaEditor.tvseriesSkinID)
              tbItemDisplayName.Text = formMustayalucaEditor.tvseriesViews[cboViews.SelectedIndex].Value;
            // Music
            if (currentSkinID == formMustayalucaEditor.musicSkinID)
              tbItemDisplayName.Text = formMustayalucaEditor.musicViews[cboViews.SelectedIndex].Value;
            // OnlineVideos
            if (currentSkinID == formMustayalucaEditor.onlineVideosSkinID)
              tbItemDisplayName.Text = formMustayalucaEditor.onlineVideosViews[cboViews.SelectedIndex].Value;

            initialIndex = cboViews.SelectedIndex;
          }
        }
    }
}
