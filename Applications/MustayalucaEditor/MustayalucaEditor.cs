﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Drawing;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Utils;
using System.Text.RegularExpressions;
using SMPCheckSum;
using WindowPlugins.GUITVSeries;
using SQLite.NET;
using MediaPortal.GUI.Music;
using MediaPortal.GUI.View;
using OnlineVideos;

namespace MustayalucaEditor
{
  [PluginIcons("MustayalucaEditor.Resources.SMPEditor.png", "MustayalucaEditor.Resources.SMPEditorDisabled.png")]
  public partial class formMustayalucaEditor : Form, ISetupForm
  {
    #region enums

    public enum errorCode
    {
      info,
      infoQuestion,
      loadError,
      readError,
      major,
    };

    public enum chosenMenuStyle
    {
      verticalStyle,
      horizontalStandardStyle,
      horizontalContextStyle,
      graphicMenuStyle
    };

    enum chosenWeatherStyle
    {
      bottom,
      middle
    }

    enum menuType
    {
      vertical,
      horizontal,
    };

    enum sync
    {
      OnLoad,
      editing,
    };

    enum rssImageType
    {
      noImage,
      skinImage,
      infoserviceImage,
    };

    enum screenResolutionType
    {
      res1280x720,
      res1920x1080
    };

    public enum CompareResult
    {
      ciCompareOk,
      ciPixelMismatch,
      ciSizeMismatch
    };

    enum tvSeriesRecentType
    {
      summary,
      full
    }

    enum movPicsRecentType
    {
      summary,
      full
    }

    public enum mostRecentTVSeriesSummaryStyle
    {
      plot,
      fanart
    }

    public enum mostRecentMovPicsSummaryStyle
    {
      plot,
      fanart
    }

    public enum musicMostRecentStyle
    {
      style1,
      style2
    }

    public enum picturesMostRecentStyle
    {
      style1,
      style2
    }

    public enum recordedTVMostRecentStyle
    {
      style1,
      style2
    }

    public enum displayMostRecent
    {
      off,
      tvSeries,
      movies,
      music,
      recordedTV,
      freeDriveSpace,
      powerControl,
      sleepControl,
      stocks,
      htpcInfo,
      updateControl,
      pictures
    }

    enum isOverlayType
    {
      TVSeries,
      MovPics,
      Music,
      RecordedTV,
      freeDriveSpace,
      powerControl,
      sleepControl,
      stocks,
      htpcInfo,
      updateControl,
      Pictures
    }

    public enum fanartSource
    {
      Scraper,
      UserDef
    }

    #endregion

    #region Variables

    Label fhChoice = new Label();
    RadioButton fhRBUserDef = new RadioButton();
    RadioButton fhRBScraper = new RadioButton();

    List<string> rawXMLFileNames = new List<string>();
    List<string> prettyFileNames = new List<string>();


    public static List<prettyItem> prettyItems = new List<prettyItem>();
    public static List<menuItem> menuItems = new List<menuItem>();
    public static List<string> driveFreeSpaceDrives = new List<string>();


    public static List<string> theTVSeriesViews = new List<string>();
    public static List<string> theMusicViews = new List<string>();
    public static List<string> theOnlineVideosViews = new List<string>();

    List<backgroundItem> bgItems = new List<backgroundItem>();
    List<string> ids = new List<string>();
    List<string> idsTemp = new List<string>();
    List<string> skinFontsFocused = new List<string>();
    List<string> skinFontsUnFocused = new List<string>();
    List<string> foldersToMove = new List<string>();

    Form user3dConfirm = new Form();
    Button bt3dOk = new Button();
    TextBox tb3dInfo = new TextBox();
    CheckBox cb3dShowAgain = new CheckBox();

    public const string tvseriesSkinID = "9811";
    public const string movingPicturesSkinID = "96742";
    public const string musicSkinID = "501";
    public const string tvSkinID = "1";
    public const string onlineVideosSkinID = "4755";
    public const string picturesSkinID = "2";
    public const bool hyperlinkParameterEnabled = true;
    public const bool hyperlinkParameterDisabled = false;
    const string quote = "\"";

    public static bool basicHomeLoadError = false;
    public static bool changeOutstanding = false;

    bool useInfoServiceSeparator = false;
    bool fanartHandlerUsed = false;
    bool exitCondition = false;
    bool mostRecentTVSeriesCycleFanart = true;
    bool mostRecentMovPicsCycleFanart = true;
    bool subMenuL1Exists = false;
    //bool subMenuL2Exists = false;
    bool fanartHandlerRelease2 = false;
    public static bool hlWarningDone = false;
    public static bool isAlpha = false;
    public static bool isBeta = false;

    string xml;
    string xmlTemplate;
    string dropShadowColor = "1d1f1b";
    string infoServiceDayProperty = "forecast";
    string splashScreenImage = null;
    string defFocus = "FFFFFF";
    string defUnFocus = "C0C0C0";
    string level1LateralBladeVisible;
    string level2LateralBladeVisible;
    public string bgFolderName = "animations";
    public string fhUserDef = string.Empty;
    public string fhSuffix = ".any";
    public static string driveFreeSpaceList = string.Empty;

    //int textXOffset = -25;
    //int maxXPosition = 520;
    int menuOffset = 0;
    int deskHeight;
    int deskWidth;
    int tvSeriesMenuID;
    int MovingPicturesMenuID;
    int MusicMenuID;
    int TVMenuID;

    bool xmlFilesDisplayed = false;

    Version baseISVer = new Version("0.9.9.3");
    Version baseISVerTwitter = new Version("1.2.0.0");
    Version isSeparatorVer = new Version("1.1.0.0");
    Version mpReleaseVersion = new Version("1.0.2.22554");
    Version isWeatherVersion = new Version("1.6.0.0");
    Version fanarthandlerVersionRequired = new Version("2.2.0.0");
    Version fhRelease2 = new Version("2.2.2.0");

    Version mpAlphaRelease = new Version("1.1.5.0");
    Version mpBetaRelease = new Version("1.1.5.26731");

    public Version fhOverlayVersion;

    public static Regex isIleagalXML = new Regex("[&<>]");

    // Default Style to Mustayaluca standard
    public static chosenMenuStyle menuStyle = chosenMenuStyle.verticalStyle;
    chosenWeatherStyle weatherStyle = chosenWeatherStyle.bottom;
    rssImageType rssImage = rssImageType.skinImage;

    // Default to Full Details
    tvSeriesRecentType tvSeriesRecentStyle = tvSeriesRecentType.summary;
    movPicsRecentType movPicsRecentStyle = movPicsRecentType.summary;

    //Default Summary Style
    public static mostRecentTVSeriesSummaryStyle mrTVSeriesSummStyle = mostRecentTVSeriesSummaryStyle.plot;
    public static mostRecentMovPicsSummaryStyle mrMovPicsSummStyle = mostRecentMovPicsSummaryStyle.plot;
    public static musicMostRecentStyle mrMusicStyle = musicMostRecentStyle.style1;
    public static picturesMostRecentStyle mrPicturesStyle = picturesMostRecentStyle.style1;
    public static recordedTVMostRecentStyle mrRecordedTVStyle = recordedTVMostRecentStyle.style1;

    // Defaut to HD res - this is any resoloution other than 1920x1080 (FullHD)
    screenResolutionType screenres = screenResolutionType.res1920x1080;

    CheckSum checkSum = new CheckSum();
    SkinInfo skInfo = new SkinInfo();
    Helper helper = new Helper();
    Form userConfirm = new Form();
    Button btOk = new Button();
    TextBox tbInfo = new TextBox();
    CheckBox cbShowAgain = new CheckBox();
    Panel selectPanel = new Panel();
    Button nextBatch = new Button();
    Button prevBatch = new Button();
    Button imgCancel = new Button();
    ProgressBar pBar = new ProgressBar();
    Label pLabel = new Label();
    Form downloadForm = new Form();
    Button downloadStop = new Button();
    TVSereisFormatOptions tvSeriesOptions = new TVSereisFormatOptions();
    MovPicsSummaryOptions movPicsOptions = new MovPicsSummaryOptions();
    editorValues basicHomeValues = new editorValues();
    defaultImages defImgs = new defaultImages();
    randomFanartSetting randomFanart = new randomFanartSetting();
    mostRecentDisplaySelection mrDisplaySelection = new mostRecentDisplaySelection();
    getButtonTexture buttonTexture = new getButtonTexture();
    //Menu Theme 
    Form menuThemeForm = new Form();
    Button btThemeOk = new Button();
    Button btThemeCancel = new Button();
    Button btThemeNext = new Button();
    Button btThemePrev = new Button();
    Button btReset = new Button();
    TextBox tbThemeInfo = new TextBox();
    Label lbMenuItem = new Label();
    Label lbTheme = new Label();
    ComboBox cboThemeSelection = new ComboBox();
    PictureBox pbThemePreview = new PictureBox();
    List<string> menuThemeFiles = new List<string>();
    int themeImageIndex = 0;
    //bool menuThemeActive = false;
    //QuickList Info
    Form formQlPopup = new Form();
    Label lbQlPopup = new Label();
    //Most Recent
    public static List<KeyValuePair<string, string>> tvseriesViews = new List<KeyValuePair<string, string>>();
    public static List<KeyValuePair<string, string>> musicViews = new List<KeyValuePair<string, string>>();
    public static List<KeyValuePair<string, string>> onlineVideosViews = new List<KeyValuePair<string, string>>();

    string MustayalucaMediaPath = Path.Combine(SkinInfo.mpPaths.Mustayalucapath, "media");
    string menuThemeName = "3DBackgrounds";
    string profileVersion = "2.0";

    #endregion

    #region Public methods

    public formMustayalucaEditor()
    {
      InitializeComponent();
      randomFanart.fanartGames = false;
      randomFanart.fanartMovies = false;
      randomFanart.fanartMovingPictures = false;
      randomFanart.fanartMusic = false;
      randomFanart.fanartPictures = false;
      randomFanart.fanartPlugins = false;
      randomFanart.fanartTv = false;
      randomFanart.fanartTVSeries = false;
      randomFanart.fanartScoreCenter = false;
      randomFanart.fanartMoviesScraperFanart = false;
      randomFanart.fanartMusicScraperFanart = false;

      disableBGSharing.Location = new Point(10, 104);

      //Check the display res
      deskHeight = Screen.PrimaryScreen.Bounds.Height;
      deskWidth = Screen.PrimaryScreen.Bounds.Width;

      Version mpVersion = new Version(MediaPortalVersion);
      if (mpVersion.CompareTo(mpAlphaRelease) >= 0)
        isAlpha = true;

      if (mpVersion.CompareTo(mpBetaRelease) >= 0 || File.Exists("C:\\usebeta.txt"))
        isBeta = true;

      buildDownloadForm();
      inialiseImgControls();

      releaseVersion.Text = String.Format("Version: {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
      DateTime buildDate = getLinkerTimeStamp(System.Reflection.Assembly.GetEntryAssembly().Location.ToString());
      compileTime.Text += " " + buildDate.ToString() + " GMT";

      lastUsedTab.Checked = Properties.Settings.Default.rememberLastUsedTab;
      if (Properties.Settings.Default.rememberLastUsedTab)
      {
        MustayalucaMenu.SelectedIndex = Properties.Settings.Default.lastUsedTab;
      }
      backupVersionsToKeep.Text = Properties.Settings.Default.keepVersions.ToString();
      if (Properties.Settings.Default.autoPurge)
      {
        autoPurgeBackups.Checked = true;
      }

      if (validForMPVersion("1.1.6.0"))
      {
        musicViews = GetMusicViews();
        cboParameterViews.Visible = false;
        lbParameterView.Visible = false;
        cboParameterViews.Items.Clear();
        theMusicViews.Clear();
        foreach (KeyValuePair<string, string> mvv in musicViews)
        {
          theMusicViews.Add(mvv.Value);
        }
        // For 1.2 Weather Icons are now part of skin
        useSkinWeatherIcons.Visible = false;
      }

      string filename = Path.Combine(Path.Combine(SkinInfo.mpPaths.pluginPath, "windows"), "MP-TVSeries.dll");
      if (Helper.IsAssemblyAvailable("MP-TVSeries", new Version(3, 0, 0, 1673), filename))
      {
        tvseriesViews = GetTVSeriesViews();
        cboParameterViews.Visible = false;
        lbParameterView.Visible = false;
        cboParameterViews.Items.Clear();
        theTVSeriesViews.Clear();
        foreach (KeyValuePair<string, string> tvv in tvseriesViews)
        {
          theTVSeriesViews.Add(tvv.Value);
        }
      }

      filename = Path.Combine(Path.Combine(SkinInfo.mpPaths.pluginPath, "windows\\OnlineVideos"), "OnlineVideos.dll");
      if (Helper.IsAssemblyAvailable("OnlineVideos", new Version(1, 0, 0, 0), filename))
      {
        onlineVideosViews = GetOnlineVideosViews();
        cboParameterViews.Visible = false;
        lbParameterView.Visible = false;
        cboParameterViews.Items.Clear();
        theOnlineVideosViews.Clear();
        foreach (KeyValuePair<string, string> ovv in onlineVideosViews)
        {
          theOnlineVideosViews.Add(ovv.Value);
        }
      }
			
      Version fhVersion = new Version(helper.fileVersion(Path.Combine(Path.Combine(SkinInfo.mpPaths.pluginPath, "process"), "FanartHandler.dll")));

      if (fhVersion.CompareTo(fhRelease2) >= 0)
      {
        fhUserDef = ".userdef";
        fanartHandlerRelease2 = true;
      }

      buildFHchoiceControls();
      disableBGSharing.Location = new Point(10, 124);

      checkAndEnableOverlays();
    }

    //
    // Key/value read methods TVSeries
    //
    // TVSeries Key
    //
    public string getTVSeriesViewKey(string value)
    {
      foreach (KeyValuePair<string, string> tvv in tvseriesViews)
      {
        if (tvv.Value.ToLower() == value.ToLower())
          return tvv.Key;
      }
      return "false";
    }
    // TVSeries Value
    //
    public string getTVSeriesViewValue(string key)
    {
      foreach (KeyValuePair<string, string> tvv in tvseriesViews)
      {
        if (tvv.Key.ToLower() == key.ToLower())
          return tvv.Value;
      }
      return "false";
    }
    //
    // Key/value read methods Music
    //
    // Music key
    //
    public string getMusicViewKey(string value)
    {
      foreach (KeyValuePair<string, string> mvv in musicViews)
      {
        if (mvv.Value.ToLower() == value.ToLower())
          return mvv.Key;
      }
      return "false";
    }
    //Music Value
    //
    public string getMusicViewValue(string key)
    {
      foreach (KeyValuePair<string, string> mvv in musicViews)
      {
        if (mvv.Key.ToLower() == key.ToLower())
          return mvv.Value;
      }
      return "false";
    }
    //
    // Key/value read methods OnlineVideos
    //
    //OnlineVideos Key
    //
    public string getOnlineVideosViewKey(string value)
    {
      foreach (KeyValuePair<string, string> mvv in onlineVideosViews)
      {
        if (mvv.Value.ToLower() == value.ToLower())
          return mvv.Key;
      }
      return "false";
    }
    //OnlineVideos Value
    //
    public string getOnlineVideosViewValue(string key)
    {
      foreach (KeyValuePair<string, string> mvv in onlineVideosViews)
      {
        if (mvv.Key.ToLower() == key.ToLower())
          return mvv.Value;
      }
      return "false";
    }
    //
    // Does the plugin support paramerters
    //
    public static bool pluginTakesParameter(string hyperLink)
    {
      Helper helper = new Helper();
      if (!isAlpha)
        return false;

      Dictionary<string, bool> parametersValid = new Dictionary<string, bool>();

      // List of plugin skinIDs that take parameters - all a bit manual and should add a file to store these at some point
      parametersValid.Add(tvseriesSkinID, true);
      parametersValid.Add(onlineVideosSkinID, true);

      if (isBeta)
      {
        parametersValid.Add(musicSkinID, true);
      }

      if (parametersValid.ContainsKey(hyperLink))
        return parametersValid[hyperLink];
      else
        return false;
    }

    #endregion

    #region Base overrides

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      if (helper.readMPConfiguration("skin", "name").ToLowerInvariant() != "mustayaluca")
      {
        DialogResult result = helper.showError("MediaPortal is not configured to use the Mustayaluca skin\n\nThe Menu Editor requires the selected Skin to be Mustayaluca.\n\nDo you want set Mustayaluca as the selected Skin", errorCode.infoQuestion);

        if (result == DialogResult.No)
        {
          this.Hide();
          exitCondition = true; ;
        }
        else
        {
          helper.writeMPConfiguration("skin", "name", "mustayaluca");
          MediaPortal.Profile.Settings.SaveCache();
        }
      }
      if (!exitCondition)
      {
        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MustayalucaEditor.rtfFiles.introduction.rtf");
        menuDescription.LoadFile(stream, RichTextBoxStreamType.RichText);
        GetMediaPortalSkinPath();
        readFonts();
        getBackupFileTotals();
        string startBasicHome = "no";
        if (isBeta)
          startBasicHome = helper.readMPConfiguration("gui", "startbasichome");
        else
          startBasicHome = helper.readMPConfiguration("general", "startbasichome");

        if (startBasicHome.ToLower() != "yes")
        {
          DialogResult result = helper.showError("MediaPortal is not configured to start with the BasicHome menu\n\nThe Menu Generated by this Editor requires this option to be enabled.\n\nDo you want the Basichome Menu enabled?", errorCode.infoQuestion);

          if (result == DialogResult.No)
          {
            this.Close();
          }
          else
          {
            helper.writeMPConfiguration("general", "startbasichome", "yes");
            if (isBeta)
            {
              helper.writeMPConfiguration("gui", "startbasichome", "yes");
              helper.writeMPConfiguration("gui", "useonlyonehome", "yes");
            }
          }
        }

        if (!System.IO.File.Exists(SkinInfo.mpPaths.sMPbaseDir + "\\Weather\\128x128.zip"))
          useSkinWeatherIcons.Text = "Replace Standard Weather Icons with Skin Supplied Versions";
        else
          useSkinWeatherIcons.Text = "Restore Standard Weather Icons";

        if (loadIDs() == true)
        {
          bgBox.Enabled = true;
          tbItemName.Enabled = true;
          addButton.Enabled = true;
          editButton.Enabled = false;
          cancelButton.Enabled = false;
          saveButton.Enabled = false;
          removeButton.Enabled = true;
          itemsOnMenubar.Enabled = true;
          disableBGSharing.Enabled = true;

          rbRssInfoServiceImage.Checked = false;
          rbRssNoImage.Checked = false;
          rbRssSkinImage.Checked = true;

          menuitemName.Text = null;
          menuItemLabel.Text = null;
          menuitemBGFolder.Text = null;
          menuitemWindow.Text = null;

          xmlFiles.SelectedItem = null;
          cboContextLabel.Text = null;
          tbItemName.Text = null;
          bgBox.Text = null;
          isWeather.Checked = false;
          selectedWindow.Text = null;
          selectedWindowID.Text = null;


          selectedWindow.Text = null;
          selectedWindowID.Text = null;

          if (fanarthandlerVersionRequired.CompareTo(fhOverlayVersion) > 0)
          {
            cbEnableRecentMusic.Enabled = false;
            cbEnableRecentRecordedTV.Enabled = false;
            mrDisplaySelection.disableMusicRB = false;
            mrDisplaySelection.disableRecordedTVRB = false;
          }
        }
        MustayalucaMenu.TabPages.Remove(menuStyleTab);
        MustayalucaMenu.TabPages.Remove(weatherStyleTab);
        MustayalucaMenu.TabPages.Remove(splashscreenSelector);

        loadMenuSettings();
        //buildThemeScreen();
        buildQuickListInfo();
        checkSplashScreens();
        toolStripStatusLabel2.Visible = false;
        itemsOnMenubar.SelectedIndex = 0;
        screenReset();
        setScreenProperties(itemsOnMenubar.SelectedIndex);
        disableItemControls();
        loadBackgroundDirs();
        cancelCreateButton.Visible = false;
        btGenerateMenu.Enabled = true;
        editButton.Enabled = true;
        btMenuIcon.Visible = false;

        if (validForMPVersion("1.0.2.22554"))
          wrapString.Enabled = true;
        else
          wrapString.Enabled = false;


        if (menuStyle == chosenMenuStyle.graphicMenuStyle)
        {
          btMenuIcon.Visible = true;
          pbMenuIconInfo.Visible = true;
        }
        //buttonTexture.initButtonTexture();
      }
      else
        this.Close();
    }



    #endregion

    #region Private methods

    bool loadIDs()
    {
      xmlFiles.Enabled = true;
      helper.getSkinFileList(ref xmlFiles, ref ids);

      //Load the file names into our own list
      rawXMLFileNames = xmlFiles.Items.OfType<string>().ToList();

      fhOverlayVersion = new Version(helper.fileVersion(Path.Combine(Path.Combine(SkinInfo.mpPaths.pluginPath, "Process"), "Fanarthandler.dll")));
      //return true;
      if (xmlFiles.Items.Count > 0)
      {
        helper.loadPrettyItems(ids);

        // Load Pretty filenames list
        foreach (prettyItem p in prettyItems)
        {
          prettyFileNames.Add(p.name);
        }

        // Default to displaying pretty names
        xmlFiles.Items.Clear();
        xmlFiles.DataSource = prettyFileNames;

        disableItemControls();
        cancelCreateButton.Visible = false;
        btGenerateMenu.Enabled = true;
        return true;
      }
      else
      {
        helper.showError("Error reading Skin diectory - no files found", errorCode.major);
        return false;
      }
    }


    displayMostRecent getMostRecentDisplayOption()
    {
      if (mrDisplaySelection.mrToDisplay == displayMostRecent.tvSeries)
        return displayMostRecent.tvSeries;

      if (mrDisplaySelection.mrToDisplay == displayMostRecent.movies)
        return displayMostRecent.movies;

      return displayMostRecent.off;
    }

    void setMostRecentDisplayOption(displayMostRecent dmr)
    {
      switch (dmr)
      {
        case displayMostRecent.off:
          {
            mrDisplaySelection.mrToDisplay = displayMostRecent.off;
            break;
          }
        case displayMostRecent.tvSeries:
          {
            mrDisplaySelection.mrToDisplay = displayMostRecent.tvSeries;
            break;
          }
        case displayMostRecent.movies:
          {
            mrDisplaySelection.mrToDisplay = displayMostRecent.movies;
            break;
          }
      }
    }


    void addButton_Click(object sender, EventArgs e)
    {
      int index = ids.IndexOf(selectedWindowID.Text);
      try
      {
        while (rawXMLFileNames[index].ToString().ToLower() != selectedWindow.Text.ToLower())
        {
          index = ids.IndexOf(selectedWindowID.Text, index + 1);
        }
      }
      catch (Exception exp)
      {
        helper.showError("QuickSelect Skin XML Missing or Invalid" + exp.Message, errorCode.info);
        return;
      }

      if (xmlFiles.SelectedItem != null && (bgBox.Text != "" || cboFanartProperty.Text != "") && tbItemName.Text != "")
      {
        toolStripStatusLabel1.Text = rawXMLFileNames[index].ToString() + " added to menu";
        menuItem item = new menuItem();
        item.xmlFileName = rawXMLFileNames[index].ToString();
        item.name = tbItemName.Text;
        item.contextLabel = cboContextLabel.Text;
        item.hyperlink = ids[index];
        item.bgFolder = bgBox.Text;
        item.fanartProperty = cboFanartProperty.Text;

        if (fhRBScraper.Checked)
          item.fhBGSource = fanartSource.Scraper;
        else
          item.fhBGSource = fanartSource.UserDef;

        item.fanartHandlerEnabled = cbItemFanartHandlerEnable.Checked;
        item.EnableMusicNowPlayingFanart = cbEnableMusicNowPlayingFanart.Checked;
        item.isWeather = isWeather.Checked;
        item.disableBGSharing = disableBGSharing.Checked;

        // If using 3D backgrounds disable BG sharing for item.
        if (!item.fanartHandlerEnabled && (bgBox.Text.ToLower() == "3dbackgrounds"))
          item.disableBGSharing = true;

        item.showMostRecent = getMostRecentDisplayOption();
        if (pluginTakesParameter(item.hyperlink) && cboParameterViews.SelectedIndex != -1)
        {
          switch (item.hyperlink)
          {
            case tvseriesSkinID:
              item.hyperlinkParameter = getTVSeriesViewKey(cboParameterViews.SelectedItem.ToString());
              break;
            case musicSkinID:
              item.hyperlinkParameter = getMusicViewKey(cboParameterViews.SelectedItem.ToString());
              break;
            case onlineVideosSkinID:
              item.hyperlinkParameter = getOnlineVideosViewKey(cboParameterViews.SelectedItem.ToString());
              break;
            default:
              break;
          }

        }
        else
          item.hyperlinkParameter = "false";

        if (item.fanartHandlerEnabled)
          checkAndSetRandomFanart(item.fanartProperty);
        else
          checkAndSetDefultImage(item);

        if (string.IsNullOrEmpty(buttonTexture.SelectedIcon))
          item.buttonTexture = setDefaultIcons(int.Parse(item.hyperlink), "Black");
        else
          item.buttonTexture = buttonTexture.SelectedIcon;

        //buttonTexture.MenuItem = item.name;
        setDefaultIcons(int.Parse(item.hyperlink), "Black");

        if (cbOnlineVideosReturn.Checked)
          item.hyperlinkParameterOption = "Root";
        else
          item.hyperlinkParameterOption = "Locked";

        menuItems.Add(item);
        itemsOnMenubar.Items.Add(item.name);
        reloadBackgroundItems();
        // clear up
        tbItemName.Text = string.Empty;
        bgBox.Text = string.Empty;
        cboContextLabel.Text = string.Empty;

        if (itemsOnMenubar.Items.Count > 2)
          btGenerateMenu.Enabled = true;
        xmlFiles.SelectedIndex = -1;
        changeOutstanding = true;
      }
      else
      {
        helper.showError("All fields must be complete before a Menu Item can be added", errorCode.info);
      }

    }

    void editButton_Click(object sender, EventArgs e)
    {
      if (itemsOnMenubar.SelectedIndex == -1)
      {
        helper.showError("No menu item selected\n\nPlease select item above to edit", errorCode.info);
        return;
      }

      int index = itemsOnMenubar.SelectedIndex;
      menuItem mnuItem = menuItems[index];

      btGenerateMenu.Enabled = false;
      if (xmlFilesDisplayed)
        xmlFiles.SelectedIndex = ids.IndexOf(mnuItem.hyperlink);
      else
      {
        selectedWindowID.Text = mnuItem.hyperlink;
        selectedWindow.Text = mnuItem.xmlFileName;
      }

      tbItemName.Text = mnuItem.name;
      cboContextLabel.Text = mnuItem.contextLabel;
      bgBox.Text = mnuItem.bgFolder;
      cboFanartProperty.Text = mnuItem.fanartProperty;
      buttonTexture.SelectedIcon = mnuItem.buttonTexture;
      buttonTexture.MenuItem = mnuItem.name;
      buttonTexture.menIndex = index;
      cbOnlineVideosReturn.Checked = false;

      if (mnuItem.hyperlinkParameterOption == "Root")
        cbOnlineVideosReturn.Checked = true;

      if (mnuItem.fhBGSource == fanartSource.Scraper)
      {
        fhRBScraper.Checked = true;
        fhRBUserDef.Checked = false;
      }
      else
      {
        fhRBScraper.Checked = false;
        fhRBUserDef.Checked = true;
      }

      switch (mnuItem.hyperlink)
      {
        case tvseriesSkinID:
          cboParameterViews.DataSource = theTVSeriesViews;
          if (mnuItem.hyperlinkParameter != "false")
            cboParameterViews.Text = getTVSeriesViewValue(mnuItem.hyperlinkParameter);
          else
            cboParameterViews.Text = string.Empty;
          break;
        case musicSkinID:
          cboParameterViews.DataSource = theMusicViews;
          if (mnuItem.hyperlinkParameter != "false")
            cboParameterViews.Text = getMusicViewValue(mnuItem.hyperlinkParameter);
          else
            cboParameterViews.Text = string.Empty;
          break;
        case onlineVideosSkinID:
          cboParameterViews.DataSource = theOnlineVideosViews;
          if (mnuItem.hyperlinkParameter != "false")
            cboParameterViews.Text = getOnlineVideosViewValue(mnuItem.hyperlinkParameter);
          else
            cboParameterViews.Text = string.Empty;
          break;
        default:
          break;
      }



      if (cboFanartProperty.Text.ToLower() == "false")
        cboFanartProperty.Text = "";

      cbItemFanartHandlerEnable.Checked = mnuItem.fanartHandlerEnabled;
      cbEnableMusicNowPlayingFanart.Checked = mnuItem.EnableMusicNowPlayingFanart;
      disableBGSharing.Checked = mnuItem.disableBGSharing;
      isWeather.Checked = mnuItem.isWeather;
      selectedWindow.Text = xmlFiles.Text;
      selectedWindowID.Text = mnuItem.hyperlink;
      setMostRecentDisplayOption(mnuItem.showMostRecent);
      if (mnuItem.fanartHandlerEnabled)
        checkAndSetRandomFanart(mnuItem.fanartProperty);

      saveButton.Enabled = true;
      cancelButton.Enabled = true;
      editButton.Enabled = false;
      enableItemControls();
      addButton.Enabled = false;
      cancelCreateButton.Visible = false;
    }

    void saveButton_Click(object sender, EventArgs e)
    {
      if (itemsOnMenubar.SelectedItem != null)
      {
        int index = itemsOnMenubar.SelectedIndex;
        menuItem item = new menuItem();
        item = menuItems[index];
        item.name = tbItemName.Text;
        item.contextLabel = cboContextLabel.Text;
        item.bgFolder = bgBox.Text;
        item.fanartProperty = cboFanartProperty.Text;

        if (fhRBScraper.Checked)
          item.fhBGSource = fanartSource.Scraper;
        else
          item.fhBGSource = fanartSource.UserDef;

        item.fanartHandlerEnabled = cbItemFanartHandlerEnable.Checked;
        item.EnableMusicNowPlayingFanart = cbEnableMusicNowPlayingFanart.Checked;

        item.xmlFileName = selectedWindow.Text;
        item.hyperlink = selectedWindowID.Text;

        if (cboParameterViews.SelectedIndex != -1)
        {
          if (item.hyperlink == tvseriesSkinID)
            item.hyperlinkParameter = getTVSeriesViewKey(cboParameterViews.SelectedItem.ToString());

          if (item.hyperlink == musicSkinID)
            item.hyperlinkParameter = getMusicViewKey(cboParameterViews.SelectedItem.ToString());

          if (item.hyperlink == onlineVideosSkinID)
            item.hyperlinkParameter = getOnlineVideosViewKey(cboParameterViews.SelectedItem.ToString());
        }
        else
        {
          item.hyperlinkParameter = "false";
          cboParameterViews.Text = string.Empty;
        }
        item.disableBGSharing = disableBGSharing.Checked;
        // If using 3D backgrounds disable BG sharing for item.
        if (!item.fanartHandlerEnabled && (bgBox.Text.ToLower() == "3dbackgrounds"))
          item.disableBGSharing = true;

        item.isWeather = isWeather.Checked;
        item.showMostRecent = getMostRecentDisplayOption();

        if (item.isWeather && weatherBGlink.Checked && item.fanartHandlerEnabled)
        {
          DialogResult result = helper.showError("You have selected to use Fanart Random images for the Weather item\nbut the option 'Link Background to Current Weather' is enabled\nand will override this setting\n\nDisable the 'Link Background to Current Weather' Option? ", errorCode.infoQuestion);
          if (result == DialogResult.Yes)
          {
            weatherBGlink.Checked = false;
          }
        }

        if (item.fanartHandlerEnabled)
          checkAndSetRandomFanart(item.fanartProperty);
        else
        {
          checkAndSetDefultImage(item);
        }

        item.buttonTexture = buttonTexture.SelectedIcon;

        if (cbOnlineVideosReturn.Checked)
          item.hyperlinkParameterOption = "Root";
        else
          item.hyperlinkParameterOption = "Locked";

        menuItems[index] = item;
        itemsOnMenubar.Items.RemoveAt(index);
        itemsOnMenubar.Items.Insert(index, item.name);
        reloadBackgroundItems();
        screenReset();
        disableItemControls();
        cancelCreateButton.Visible = false;
        btGenerateMenu.Enabled = true;
        changeOutstanding = true;
        cboParameterViews.Visible = false;
        if (menuStyle == chosenMenuStyle.graphicMenuStyle)
          displayMenuIcon(item.buttonTexture);
      }
    }

    void screenReset()
    {
      if (saveButton.Enabled)
      {
        tbItemName.Text = string.Empty;
        cboContextLabel.Text = string.Empty;
        isWeather.Checked = false;
        bgBox.SelectedIndex = -1;
        cboFanartProperty.SelectedIndex = -1;
        saveButton.Enabled = false;
        cancelButton.Enabled = false;
        editButton.Enabled = true;
      }
      selectedWindow.Text = string.Empty;
      selectedWindowID.Text = string.Empty;
      cboParameterViews.Text = string.Empty;
      lbParameterView.Visible = false;
      cbOnlineVideosReturn.Visible = false;

    }


    void itemsOnMenubar_SelectedIndexChanged(object sender, EventArgs e)
    {
      screenReset();
      setScreenProperties(itemsOnMenubar.SelectedIndex);
      editButton.Enabled = true;
      disableItemControls();
    }

    void cancelButton_Click(object sender, EventArgs e)
    {
      screenReset();
      setScreenProperties(itemsOnMenubar.SelectedIndex);
      disableItemControls();
      btGenerateMenu.Enabled = true;
    }

    void setScreenProperties(int index)
    {
      if (index < 0) return;

      menuItem mnuItem = menuItems[index];

      if (xmlFilesDisplayed)
        xmlFiles.SelectedItem = mnuItem.xmlFileName;
      else
      {
        int pindex = 0;
        foreach (prettyItem p in prettyItems)
        {
          if (p.id == mnuItem.hyperlink)
            break;
          pindex++;
        }
        if (pindex < prettyItems.Count)
          xmlFiles.SelectedIndex = pindex;
      }

      cboContextLabel.Text = mnuItem.contextLabel;
      tbItemName.Text = mnuItem.name;
      cboFanartProperty.Text = mnuItem.fanartProperty;
      cbItemFanartHandlerEnable.Checked = mnuItem.fanartHandlerEnabled;
      cbEnableMusicNowPlayingFanart.Checked = mnuItem.EnableMusicNowPlayingFanart;
      disableBGSharing.Checked = mnuItem.disableBGSharing;
      cbOnlineVideosReturn.Visible = false;

      if (pluginTakesParameter(mnuItem.hyperlink))
      {
        switch (mnuItem.hyperlink)
        {
          case tvseriesSkinID:
            cboParameterViews.DataSource = theTVSeriesViews;
            if (mnuItem.hyperlinkParameter != "false")
              cboParameterViews.Text = getTVSeriesViewKey(mnuItem.hyperlinkParameter);
            else
            {
              cboParameterViews.Text = string.Empty;
              cboParameterViews.SelectedIndex = -1;
            }
            break;
          case musicSkinID:
            cboParameterViews.DataSource = theMusicViews;
            if (mnuItem.hyperlinkParameter != "false")
              cboParameterViews.Text = getMusicViewValue(mnuItem.hyperlinkParameter);
            else
            {
              cboParameterViews.Text = string.Empty;
              cboParameterViews.SelectedIndex = -1;
            }
            break;
          case onlineVideosSkinID:
            cboParameterViews.DataSource = theOnlineVideosViews;
            if (mnuItem.hyperlinkParameter != "false")
            {
              cboParameterViews.Text = getOnlineVideosViewValue(mnuItem.hyperlinkParameter);
              if (mnuItem.hyperlinkParameterOption == "Root")
                cbOnlineVideosReturn.Checked = true;
              else
                cbOnlineVideosReturn.Checked = false;
            }
            else
            {
              cboParameterViews.Text = string.Empty;
              cboParameterViews.SelectedIndex = -1;
            }
            cbOnlineVideosReturn.Visible = true;
            break;
          default:
            break;
        }
        cboParameterViews.Visible = true;
        lbParameterView.Visible = true;

      }
      else
      {
        cboParameterViews.Visible = false;
        lbParameterView.Visible = false;
      }

      bgBox.Text = mnuItem.bgFolder;

      menuitemName.Text = mnuItem.name;
      menuItemLabel.Text = mnuItem.contextLabel;
      menuitemBGFolder.Text = mnuItem.bgFolder;
      menuitemWindow.Text = mnuItem.xmlFileName;

      selectedWindowID.Text = mnuItem.hyperlink;
      selectedWindow.Text = mnuItem.xmlFileName;

      setMostRecentDisplayOption(mnuItem.showMostRecent);

      if (menuStyle == chosenMenuStyle.graphicMenuStyle)
        displayMenuIcon(mnuItem.buttonTexture);

      UpdateImageControlVisibility(mnuItem.fanartHandlerEnabled);
    }

    void displayMenuIcon(string icon)
    {
      string MustayalucaMediaPath = Path.Combine(SkinInfo.mpPaths.Mustayalucapath, "media");
      if (string.IsNullOrEmpty(icon) || !File.Exists(Path.Combine(MustayalucaMediaPath, icon)))
        icon = "homeButtons\\_noimage.png";

      pbMenuIconInfo.Image = Image.FromFile(Path.Combine(MustayalucaMediaPath, icon)).GetThumbnailImage(40, 40, null, new IntPtr());
    }

    void browseButton_Click(object sender, EventArgs e)
    {
      folderBrowserDialog.ShowNewFolderButton = false;
      folderBrowserDialog.Description = "Select the folder containing the images for this menu item";
      folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
      if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
      {
        if (getFileListing(folderBrowserDialog.SelectedPath, "*.*", true).Length == 0)
          helper.showError("The selected directory contains no files\n\nPleas ensure the folder contains at least one image file", errorCode.info);
        else
          bgBox.Text = folderBrowserDialog.SelectedPath;
      }
    }



    void xmlFiles_Click(object sender, EventArgs e)
    {
      selectedWindow.Text = xmlFiles.Text;
      selectedWindowID.Text = ids[xmlFiles.SelectedIndex];
      enableItemControls();
      editButton.Enabled = false;
      cancelCreateButton.Visible = true;
      btGenerateMenu.Enabled = false;
      if (pluginTakesParameter(selectedWindowID.Text))
      {
        cboParameterViews.Visible = true;
        lbParameterView.Visible = true;
        if (selectedWindowID.Text == onlineVideosSkinID)
          cbOnlineVideosReturn.Visible = true;
      }
      else
      {
        cboParameterViews.Visible = false;
        lbParameterView.Visible = false;
        cbOnlineVideosReturn.Visible = false;
      }
    }


    // Move selected menu item up one position in list. 
    void btMoveUp_Click(object sender, EventArgs e)
    {
      // Do nothing if no item is selected or if already at top
      if (itemsOnMenubar.SelectedItem != null && itemsOnMenubar.SelectedIndex > 0)
      {
        int index = itemsOnMenubar.SelectedIndex;


        Object listItem = itemsOnMenubar.SelectedItem;
        menuItem mnuItem = menuItems[index];

        itemsOnMenubar.Items.RemoveAt(index);
        menuItems.RemoveAt(index);

        itemsOnMenubar.Items.Insert(index - 1, listItem);
        menuItems.Insert(index - 1, mnuItem);

        itemsOnMenubar.SelectedIndex = index - 1;

      }
      foreach (menuItem item in menuItems)
      {
        Console.WriteLine(item.name);
      }
      Console.WriteLine("");

    }

    // Move selected menu item down one position in list. 
    void btMoveDown_Click(object sender, EventArgs e)
    {
      // Do nothing if no item is selected or if already at bottom
      if (itemsOnMenubar.SelectedItem != null && itemsOnMenubar.SelectedIndex < itemsOnMenubar.Items.Count - 1)
      {
        int index = itemsOnMenubar.SelectedIndex;


        Object listItem = itemsOnMenubar.SelectedItem;
        menuItem mnuItem = menuItems[index];

        itemsOnMenubar.Items.RemoveAt(index);
        menuItems.RemoveAt(index);
        if (index < itemsOnMenubar.Items.Count)
        {
          itemsOnMenubar.Items.Insert(index + 1, listItem);
          menuItems.Insert(index + 1, mnuItem);
        }
        else
        {
          itemsOnMenubar.Items.Add(listItem);
          menuItems.Add(mnuItem);
        }
        itemsOnMenubar.SelectedIndex = index + 1;

      }

      foreach (menuItem item in menuItems)
      {
        Console.WriteLine(item.name);
      }
      Console.WriteLine("");
    }

    bool isIlegalXML(string theValue)
    {
      Match m = isIleagalXML.Match(theValue);
      return m.Success;
    }

    void itemName_TextChanged(object sender, EventArgs e)
    {
      int start = tbItemName.SelectionStart;
      if (isIlegalXML(tbItemName.Text))
      {
        tbItemName.Text = tbItemName.Text.Substring(0, tbItemName.Text.Length - 1);
        tbItemName.SelectionStart = start;
        return;
      }
      tbItemName.Text = tbItemName.Text.ToUpper();
      tbItemName.SelectionStart = start;
    }


    void cboContextLabels_TextChanged(object sender, EventArgs e)
    {
      int start = cboContextLabel.SelectionStart;
      if (isIlegalXML(tbItemName.Text))
      {
        tbItemName.Text = tbItemName.Text.Substring(0, tbItemName.Text.Length - 1);
        tbItemName.SelectionStart = start;
        return;
      }
      cboContextLabel.Text = cboContextLabel.Text.ToUpper();
      cboContextLabel.SelectionStart = start;
    }


    void btGenerateMenu_Click(object sender, EventArgs e)
    {
      if (itemsOnMenubar.CheckedItems.Count > 1 || itemsOnMenubar.CheckedItems.Count == 0)
      {
        if (itemsOnMenubar.CheckedItems.Count == 0) helper.showError("\t      No Default Item\n\nYou must set one item as the default menu item.", errorCode.info);
        if (itemsOnMenubar.CheckedItems.Count > 1) helper.showError("          More than one default item set\n\nOnly one item can be set as the default menu.", errorCode.info);
      }
      else if (itemsOnMenubar.Items.Count < 3)
      {
        helper.showError("          Too few Menu Items - The minium is 3\n\nPlease add additional Menu items(s) before trying to Generate.", errorCode.info);
      }
      else
        genMenu(false);

      changeOutstanding = false;

    }

    void genMenu(bool onFormClosing)
    {
      validateMenuOffset();
      foreach (menuItem item in menuItems)
      {
        item.id = 1000 + menuItems.IndexOf(item);
        if (item.name == itemsOnMenubar.CheckedItems[0].ToString())
        {
          item.isDefault = true;
          basicHomeValues.defaultId = menuItems.IndexOf(item);
        }
        else
        {
          item.isDefault = false;
        }
        if (!infoserviceOptions.Enabled || !weatherBGlink.Checked)
          if (item.bgFolder == null && item.fanartProperty == null)
          {
            helper.showError("Menu Item " + item.name + " Has no Background Image folder\n\n\tPlease set a Folder", errorCode.info);
            return;
          }
      }
      setBasicHomeValues();
      if (menuStyle == chosenMenuStyle.verticalStyle)
      {
        writeMenu(menuType.vertical, onFormClosing);
      }
      else
      {
        writeMenu(menuType.horizontal, onFormClosing);
      }
      if (cboClearCache.Checked)
        clearCacheDir();
    }


    // This function call will regenerate the menu from the currenly saved mustayalucamenuprofile.xml
    // it assums that this file is correct.....
    public void reGenterateMenu()
    {
      this.WindowState = FormWindowState.Minimized;
      GetMediaPortalSkinPath();
      loadIDs();
      readFonts();
      getBackupFileTotals();
      loadMenuSettings();
      setBasicHomeValues();
      foreach (menuItem item in menuItems)
      {
        item.id = 1000 + menuItems.IndexOf(item);
        if (item.name == itemsOnMenubar.CheckedItems[0].ToString())
        {
          item.isDefault = true;
          basicHomeValues.defaultId = menuItems.IndexOf(item);
        }
        else
        {
          item.isDefault = false;
        }
        if (!infoserviceOptions.Enabled || !weatherBGlink.Checked)
          if (item.bgFolder == null && item.fanartProperty == null)
          {
            helper.showError("Menu Item " + item.name + " Has no Background Image folder\n\n\tPlease set a Folder", errorCode.info);
            return;
          }
      }
      if (menuStyle == chosenMenuStyle.verticalStyle)
      {
        writeMenu(menuType.vertical, true);
      }
      else
      {
        writeMenu(menuType.horizontal, true);
      }
      if (cboClearCache.Checked)
        clearCacheDir();
      Application.Exit();
    }

    void writeMenu(menuType direction, bool onFormClosing)
    {

      randomFanart.fanartGames = false;
      randomFanart.fanartMovies = false;
      randomFanart.fanartMoviesScraperFanart = false;
      randomFanart.fanartMovingPictures = false;
      randomFanart.fanartMusic = false;
      randomFanart.fanartMusicScraperFanart = false;
      randomFanart.fanartPictures = false;
      randomFanart.fanartPlugins = false;
      randomFanart.fanartTv = false;
      randomFanart.fanartTVSeries = false;
      randomFanart.fanartScoreCenter = false;

      switchOnOffFHControls();
      assoicateDefaultItems();
      generateXML(direction);
      generateBg(direction);

      if (fanartHandlerUsed)
      {
        generateFanartControls();
        generateBackgroundLoading();
      }

      if (!cbDisableClock.Checked)
        generateClock();

      if (!cbHideFanartScraper.Checked)
        generatefanartScraper();

      if (enableFiveDayWeather.Checked)
        GenerateFiveDayWeather();

      if (summaryWeatherCheckBox.Checked && infoserviceOptions.Enabled)
      {
        bool itemHasWeather = false;
        foreach (backgroundItem item in bgItems)
        {
          // Check what item to hide weather summary on
          if (item.isWeather)
          {
            itemHasWeather = true;
            basicHomeValues.weatherControl = (int.Parse(item.ids[0]) + 200);
            generateWeathersummary(basicHomeValues.weatherControl);
          }
        }
        // always show weather summary
        if (!itemHasWeather) generateWeathersummary(null);
      }

      if (direction == menuType.horizontal)
      {
        if (!cbDisableExitMenu.Checked)
          generateTopBarH();

        generateMenuGraphicsH();
        generateCrowdingFixH();
      }

      if (enableRssfeed.Checked && infoserviceOptions.Enabled)
      {
        if (direction == menuType.horizontal)
        {
          generateRSSTicker();
          if (enableTwitter.Checked && infoserviceOptions.Enabled) generateTwitter();
        }
      }

      generateMostRecentFilesAndImports("AddImports");

      toolStripStatusLabel1.Text = "Done!";

      if (System.IO.File.Exists(SkinInfo.mpPaths.Mustayalucapath + "BasicHome.xml"))
        System.IO.File.Delete(SkinInfo.mpPaths.Mustayalucapath + "BasicHome.xml");

      xml = xml.Replace("<!-- BEGIN GENERATED ID CODE-->", "<id>35</id>");
      CheckSum checkSum = new CheckSum();
      writeXMLFile("BasicHome.xml");

      generateMostRecentFilesAndImports("GenImports");

      if (menuStyle == chosenMenuStyle.graphicMenuStyle)
        //generateOverlay(int.Parse(txtMenuPos.Text), 0, basicHomeValues.weatherControl);
        generateOverlay(0, 380, basicHomeValues.weatherControl);
      else
        //generateOverlay(int.Parse(txtMenuPos.Text), 765, basicHomeValues.weatherControl);
        generateOverlay(0, 0, basicHomeValues.weatherControl);

      changeOutstanding = false;
      getBackupFileTotals();
      if (!onFormClosing)
      {
        DialogResult result = helper.showError("BasicHome.xml Saved Sucessfully \n\n  Backup file has been created \n\nDo you want to Contine Editing", errorCode.infoQuestion);
        if (result == DialogResult.No)
        {
          //reset everything
          //xmlFiles.Items.Clear();
          //cboQuickSelect.Items.Clear();
          itemsOnMenubar.Items.Clear();
          prettyItems.Clear();
          ids.Clear();
          bgItems.Clear();
          menuItems.Clear();
          this.Close();
        }

        // reset item id's as it is possible to generate again.
        foreach (menuItem item in menuItems)
        {
          item.id = menuItems.IndexOf(item);
        }
      }
    }
    //
    // Switch on or off the FH controls
    //
    void switchOnOffFHControls()
    {
      foreach (menuItem menItem in menuItems)
      {
        if (menItem.fanartHandlerEnabled)
        {
          // Check and set random fanart
          if (menItem.fanartProperty.ToLower().Contains("games"))
            randomFanart.fanartGames = true;

          if (menItem.fanartProperty.ToLower().Contains("plugins"))
            randomFanart.fanartPlugins = true;

          if (menItem.fanartProperty.ToLower().Contains("picture"))
            randomFanart.fanartPictures = true;

          if (menItem.fanartProperty.ToLower().Contains("tv"))
            randomFanart.fanartTv = true;

          if (menItem.fanartProperty.ToLower().Contains("music"))
          {
            if (fanartHandlerRelease2)
            {
              if (menItem.fhBGSource == fanartSource.Scraper)
              {
                randomFanart.fanartMusic = false;
                randomFanart.fanartMusicScraperFanart = true;
              }
              else
              {
                randomFanart.fanartMusic = true;
                randomFanart.fanartMusicScraperFanart = false;
              }
            }
            else
              randomFanart.fanartMusic = true;
          }

          if (menItem.fanartProperty.ToLower().Contains("tvseries"))
            randomFanart.fanartTVSeries = true;

          if (menItem.fanartProperty.ToLower().Contains("movingpicture"))
            randomFanart.fanartMovingPictures = true;

          if (menItem.fanartProperty.ToLower().Contains("movie"))
          {
            if (fanartHandlerRelease2)
            {
              if (menItem.fhBGSource == fanartSource.Scraper)
              {
                randomFanart.fanartMovies = false;
                randomFanart.fanartMoviesScraperFanart = true;
              }
              else
              {
                randomFanart.fanartMovies = true;
                randomFanart.fanartMoviesScraperFanart = false;
              }
            }
            else
              randomFanart.fanartMovies = true;
          }

          if (menItem.fanartProperty.ToLower().Contains("scorecenter"))
            randomFanart.fanartScoreCenter = true;
        }
      }
    }
    //
    // Generate the Most recent importfile and add to basic home
    //
    void generateMostRecentFilesAndImports(string recentAction)
    {
      //
      // Generate the Infoservice Most Recent Import files
      //
      if (recentAction == "GenImports")
      {
        if (cbMostRecentTvSeries.Checked)
          // Params: Overlay Type, Recent added summary x,y, Recent watched summary x,y
          if (mrTVSeriesSummStyle == mostRecentTVSeriesSummaryStyle.fanart)
            generateMostRecentOverlay(menuStyle, isOverlayType.TVSeries, 579, (int.Parse(txtMenuPos.Text) - 244), 976, 370);
          else if (mrTVSeriesSummStyle == mostRecentTVSeriesSummaryStyle.plot)
            generateMostRecentOverlay(menuStyle, isOverlayType.TVSeries, 620, (int.Parse(txtMenuPos.Text) - 370), 976, 370);

        if (cbMostRecentMovPics.Checked)
          // Params: Overlay Type, Recent added summary x,y, Recent watched summary x,y
          if (mrMovPicsSummStyle == mostRecentMovPicsSummaryStyle.fanart)
            generateMostRecentOverlay(menuStyle, isOverlayType.MovPics, 672, (int.Parse(txtMenuPos.Text) - 431), 976, 370);
          else if (mrMovPicsSummStyle == mostRecentMovPicsSummaryStyle.plot)
            generateMostRecentOverlay(menuStyle, isOverlayType.MovPics, 695, (int.Parse(txtMenuPos.Text) - 462), 976, 370);
        //
        // Only generate music and RecordedTV if the correct Fanart Handler version is installed and enabled
        //
        if (helper.pluginEnabled(Helper.Plugins.FanartHandler) && (fanarthandlerVersionRequired.CompareTo(fhOverlayVersion) <= 0))
        {
          // Params: Overlay Type, Recent added summary x,y, Recent watched summary x,y
          if (cbEnableRecentMusic.Checked)
          {
            if (mrMusicStyle == musicMostRecentStyle.style1)
              generateMostRecentOverlay(menuStyle, isOverlayType.Music, 695, (int.Parse(txtMenuPos.Text) - 338), 0, 0);
            else if (mrMusicStyle == musicMostRecentStyle.style2)
              generateMostRecentOverlay(menuStyle, isOverlayType.Music, 672, (int.Parse(txtMenuPos.Text) - 271), 0, 0);
          }

          // Params: Overlay Type, Recent added summary x,y, Recent watched summary x,y
          if (cbEnableRecentRecordedTV.Checked)
            generateMostRecentOverlay(menuStyle, isOverlayType.RecordedTV, 620, (int.Parse(txtMenuPos.Text) - 340), 0, 0);

          // Params: Overlay Type, Recent added summary x,y, Recent watched summary x,y
          if (cbEnableRecentPictures.Checked)
            generateMostRecentOverlay(menuStyle, isOverlayType.Pictures, 620, (int.Parse(txtMenuPos.Text) - 340), 0, 0);

        }

        if (cbFreeDriveSpaceOverlay.Checked)
          generateMostRecentOverlay(menuStyle, isOverlayType.freeDriveSpace, 976, 50, 0, 0);

        if (cbSleepControlOverlay.Checked)
          generateMostRecentOverlay(menuStyle, isOverlayType.sleepControl, 976, 50, 0, 0);

        if (cbSocksOverlay.Checked)
          generateMostRecentOverlay(menuStyle, isOverlayType.stocks, 976, 50, 0, 0);

        if (cbPowerControlOverlay.Checked)
          generateMostRecentOverlay(menuStyle, isOverlayType.powerControl, 976, 50, 0, 0);

        if (cbHtpcInfoOverlay.Checked)
          generateMostRecentOverlay(menuStyle, isOverlayType.htpcInfo, 976, 50, 0, 0);

        if (cbUpdateControlOverlay.Checked)
          generateMostRecentOverlay(menuStyle, isOverlayType.updateControl, 976, 50, 0, 0);
      }
      //
      // Add the imports to basichome
      //
      if (recentAction == "AddImports")
      {
        if (cbMostRecentTvSeries.Checked && mostRecentVisibleControls(isOverlayType.TVSeries) != null)
          generateMostRecentInclude(isOverlayType.TVSeries);

        if (cbMostRecentMovPics.Checked && mostRecentVisibleControls(isOverlayType.MovPics) != null)
          generateMostRecentInclude(isOverlayType.MovPics);

        //
        // Only add imports to basichome if the correct Fanart Handler version is installed and enabled
        //
        if (helper.pluginEnabled(Helper.Plugins.FanartHandler) && (fanarthandlerVersionRequired.CompareTo(fhOverlayVersion) <= 0))
        {
          if (cbEnableRecentMusic.Checked && mostRecentVisibleControls(isOverlayType.Music) != null)
            generateMostRecentInclude(isOverlayType.Music);

          if (cbEnableRecentRecordedTV.Checked && mostRecentVisibleControls(isOverlayType.RecordedTV) != null)
            generateMostRecentInclude(isOverlayType.RecordedTV);

          if (cbEnableRecentPictures.Checked && mostRecentVisibleControls(isOverlayType.Pictures) != null)
            generateMostRecentInclude(isOverlayType.Pictures);

        }

        if (cbFreeDriveSpaceOverlay.Checked)
          generateMostRecentInclude(isOverlayType.freeDriveSpace);

        if (cbSleepControlOverlay.Checked)
          generateMostRecentInclude(isOverlayType.sleepControl);

        if (cbSocksOverlay.Checked)
          generateMostRecentInclude(isOverlayType.stocks);

        if (cbPowerControlOverlay.Checked)
          generateMostRecentInclude(isOverlayType.powerControl);

        if (cbHtpcInfoOverlay.Checked)
          generateMostRecentInclude(isOverlayType.htpcInfo);

        if (cbUpdateControlOverlay.Checked)
          generateMostRecentInclude(isOverlayType.updateControl);
      }
    }
    //
    // Check and set default menu item for Recent Music or RecordedTV
    //
    void assoicateDefaultItems()
    {
      bool norecent = true;

      // Check if most recent music is enabled, does it have an assiocated menu item, if not default to music
      if (mostRecentVisibleControls(isOverlayType.Music) == "No" && cbEnableRecentMusic.Checked)
      {
        foreach (menuItem menItem in menuItems)
        {
          if (menItem.showMostRecent == displayMostRecent.music)
            norecent = false;
        }
        if (norecent)
        {
          for (int i = 0; i < menuItems.Count; i++)
          {
            if (menuItems[i].hyperlink.ToString() == musicSkinID)
              menuItems[i].showMostRecent = displayMostRecent.music;
          }
        }
      }
      // Check if most recent recorded TV is enabled, is there an assiocated menu item, if not default to RecordedTV
      norecent = true;
      if (mostRecentVisibleControls(isOverlayType.RecordedTV) == "No" && cbEnableRecentRecordedTV.Checked)
      {
        foreach (menuItem menItem in menuItems)
        {
          if (menItem.showMostRecent == displayMostRecent.recordedTV)
            norecent = false;
        }
        if (norecent)
        {
          for (int i = 0; i < menuItems.Count; i++)
          {
            if (menuItems[i].hyperlink.ToString() == tvSkinID)
              menuItems[i].showMostRecent = displayMostRecent.recordedTV;
          }
        }
      }
    }

    void horizontalContextLabels_CheckedChanged(object sender, EventArgs e)
    {
      setBasicHomeValues();
    }

    private void addSubmenus_Click(object sender, EventArgs e)
    {
      if (itemsOnMenubar.SelectedIndex != -1)
      {
        formSubMenuDesigner subMenuForm = new formSubMenuDesigner();
        subMenuForm.createSubmenu(itemsOnMenubar.SelectedIndex);

        // If we have added a submenu then disable Background sharing - get background issues outherwise
        if (menuItems[itemsOnMenubar.SelectedIndex].subMenuLevel1.Count > 0)
          menuItems[itemsOnMenubar.SelectedIndex].disableBGSharing = true;
      }
      else
        helper.showError("Please Highlight Menu Item to add SubMenus to", errorCode.info);
    }

    private void btSelectOverlays_Click(object sender, EventArgs e)
    {
      setOverlayStates();
      if (itemsOnMenubar.SelectedIndex != -1)
      {
        mrDisplaySelection.mrToDisplay = menuItems[itemsOnMenubar.SelectedIndex].showMostRecent;
        mrDisplaySelection.ShowDialog();
        menuItems[itemsOnMenubar.SelectedIndex].showMostRecent = mrDisplaySelection.mrToDisplay;
      }
      else
        helper.showError("Please Highlight Menu Item to edit Overlays to", errorCode.info);
    }

    void setOverlayStates()
    {
      mrDisplaySelection.setEnableState(displayMostRecent.freeDriveSpace, cbFreeDriveSpaceOverlay.Checked);
      mrDisplaySelection.setEnableState(displayMostRecent.htpcInfo, cbHtpcInfoOverlay.Checked);
      mrDisplaySelection.setEnableState(displayMostRecent.music, cbEnableRecentMusic.Checked);
      mrDisplaySelection.setEnableState(displayMostRecent.pictures, cbEnableRecentPictures.Checked);
      mrDisplaySelection.setEnableState(displayMostRecent.powerControl, cbPowerControlOverlay.Checked);
      mrDisplaySelection.setEnableState(displayMostRecent.recordedTV, cbEnableRecentRecordedTV.Checked);
      mrDisplaySelection.setEnableState(displayMostRecent.sleepControl, cbSleepControlOverlay.Checked);
      mrDisplaySelection.setEnableState(displayMostRecent.stocks, cbSocksOverlay.Checked);
      mrDisplaySelection.setEnableState(displayMostRecent.updateControl, cbUpdateControlOverlay.Checked);
      mrDisplaySelection.setEnableState(displayMostRecent.tvSeries, cbMostRecentTvSeries.Checked);
      mrDisplaySelection.setEnableState(displayMostRecent.movies, cbMostRecentMovPics.Checked);
    }

    private void cboParameterViews_SelectedIndexChanged(object sender, EventArgs e)
    {
      //cboContextLabel.Text = tvseriesViews[cboTvSeriesView.SelectedIndex].Value.ToUpper();
    }

    private void btConfigureFreeDriveSpace_Click(object sender, EventArgs e)
    {
      SelectHardDrives selectDrives = new SelectHardDrives();
      selectDrives.ShowDialog();
    }

    private void cboFanartProperty_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!fanartHandlerRelease2)
        return;

      if (cboFanartProperty.Text.ToLower() == "music" || cboFanartProperty.Text.ToLower() == "movie")
      {
        fhChoice.Visible = true;
        fhRBScraper.Visible = true;
        fhRBUserDef.Visible = true;
      }
      else
      {
        fhChoice.Visible = false;
        fhRBScraper.Visible = false;
        fhRBUserDef.Visible = false;
      }
    }

    private void cbFreeDriveSpaceOverlay_CheckedChanged(object sender, EventArgs e)
    {
      btConfigureFreeDriveSpace.Enabled = cbFreeDriveSpaceOverlay.Checked ? true : false;
      mrDisplaySelection.setEnableState(displayMostRecent.freeDriveSpace, cbFreeDriveSpaceOverlay.Checked);
    }

    private void cbPowerControlOverlay_CheckedChanged(object sender, EventArgs e)
    {
      mrDisplaySelection.setEnableState(displayMostRecent.powerControl, cbPowerControlOverlay.Checked);
    }



    private void btMenuIcon_Click(object sender, EventArgs e)
    {
      buttonTexture.setButtonTexture();
      displayMenuIcon(buttonTexture.SelectedIcon);
    }

    private void bgBox_SelectionChangeCommitted(object sender, EventArgs e)
    {
      if (bgBox.SelectedItem.ToString().ToLower() == "3dbackgrounds")
      {
        disableBGSharing.Checked = true;

        if (!Properties.Settings.Default.hide3dConfirm)
        {
          user3dConfirm.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          user3dConfirm.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          user3dConfirm.Name = "frmUser3dConfirm";
          user3dConfirm.Text = "3D Background Information";
          user3dConfirm.ControlBox = true;
          user3dConfirm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
          user3dConfirm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
          user3dConfirm.StartPosition = FormStartPosition.CenterScreen;
          user3dConfirm.Width = 400;
          user3dConfirm.Height = 160;
          user3dConfirm.MaximizeBox = false;
          user3dConfirm.MinimizeBox = false;
          user3dConfirm.TopMost = true;

          bt3dOk.Width = 80;
          bt3dOk.Text = "OK";
          bt3dOk.Location = new System.Drawing.Point(310, 105);
          bt3dOk.Click += new System.EventHandler(bt3dOk_Click);
          user3dConfirm.Controls.Add(bt3dOk);

          cb3dShowAgain.Text = "Do not show this message again";
          cb3dShowAgain.AutoSize = true;
          cb3dShowAgain.Location = new System.Drawing.Point(10, 110);
          user3dConfirm.Controls.Add(cb3dShowAgain);

          tb3dInfo.BorderStyle = BorderStyle.None;
          tb3dInfo.Multiline = true;
          tb3dInfo.Text = "When using 3D backgrounds background sharing will be automaticlly";
          tb3dInfo.AppendText(Environment.NewLine + "disabled for the Menu Item.");
          tb3dInfo.AppendText(Environment.NewLine);
          tb3dInfo.AppendText(Environment.NewLine + "Please remember you will need to set the the 3D background");
          tb3dInfo.AppendText(Environment.NewLine + "for this Menu Item in the 'Defaul Background Images' tab.");
          tb3dInfo.Location = new System.Drawing.Point(20, 20);
          tb3dInfo.WordWrap = true;
          tb3dInfo.Width = 350;
          tb3dInfo.Height = 100;
          tb3dInfo.ReadOnly = true;
          user3dConfirm.Controls.Add(tb3dInfo);

          user3dConfirm.Show();
        }
      }
    }

    private void bt3dOk_Click(object sender, EventArgs e)
    {
      user3dConfirm.Hide();
      if (cb3dShowAgain.Checked)
        Properties.Settings.Default.hide3dConfirm = true;
    }




    private void getThemeImages(string themeName)
    {
      menuThemeFiles.Clear();
      SkinInfo skInfo = new SkinInfo();
      string MustayalucaMediaPath = Path.Combine(SkinInfo.mpPaths.Mustayalucapath, "media\\" + bgFolderName);
      DirectoryInfo dInfo = new DirectoryInfo(Path.Combine(MustayalucaMediaPath, themeName));
      foreach (FileInfo fInfo in dInfo.GetFiles("*.jpg"))
      {
        menuThemeFiles.Add(fInfo.FullName);
      }
    }

    private void btBackgroundThemes_Click(object sender, EventArgs e)
    {
      readThemes();
      menuThemeForm.Show();
    }

    void readThemes()
    {
      XmlDocument doc = new XmlDocument();
      //
      // Open the intstalled themes file - If there is no file the theme will default to the 3DBackgrounds defaut theme.
      //
      if (File.Exists(SkinInfo.mpPaths.Mustayalucapath + "SMPThemes.xml"))
      {
        try
        {
          doc.Load(SkinInfo.mpPaths.Mustayalucapath + "SMPThemes.xml");
        }
        catch (Exception e1)
        {
          helper.showError("Exception while loading SMPThemes.xml\n\nUnable to Continue - please restore from backup" + e1.Message, errorCode.major);
        }
        string themeTag = "Installed Themes";
        // Now read the file
        XmlNodeList nodelist = doc.DocumentElement.SelectNodes("/themes/skin");
        cboThemeSelection.Items.Clear();
        for (int i = 0; i < int.Parse(readEntryValue(themeTag, "count", nodelist)); i++)
        {
          cboThemeSelection.Items.Add(readEntryValue(themeTag, "theme" + i.ToString(), nodelist));
        }
      }
    }

    void cboThemeSelection_SelectedIndexChanged(object sender, EventArgs e)
    {
      getThemeImages(cboThemeSelection.Text);
      pbThemePreview.Image = (Bitmap)Image.FromFile(menuThemeFiles[0]).Clone();
    }

    private void btThemeNext_Click(object sender, EventArgs e)
    {
      if ((themeImageIndex + 1) < menuThemeFiles.Count)
        themeImageIndex++;
      else
        themeImageIndex = 0;

      matchBackground(Path.GetFileNameWithoutExtension(menuThemeFiles[themeImageIndex]));
      workingImage = Image.FromFile(menuThemeFiles[themeImageIndex]);
      pbThemePreview.Image = (Bitmap)workingImage.Clone();
      workingImage.Dispose();
    }

    private void btThemePrev_Click(object sender, EventArgs e)
    {
      if ((themeImageIndex - 1) >= 0)
        themeImageIndex--;
      else
        themeImageIndex = menuThemeFiles.Count - 1;

      matchBackground(Path.GetFileNameWithoutExtension(menuThemeFiles[themeImageIndex]));
      workingImage = Image.FromFile(menuThemeFiles[themeImageIndex]);
      pbThemePreview.Image = (Bitmap)workingImage.Clone();
      workingImage.Dispose();
    }

    private void themeEnable_Click(object sender, EventArgs e)
    {
      applybackgrounds();
      menuThemeForm.Hide();
      MessageBox.Show("The 3DBackgrounds Theme has been applied\n\nIntial images have been configured for each menu item\nthese can be futher customised in'Default Background Images' tab.",
              "Background Theme 3DBackgrounds Applied",
              MessageBoxButtons.OK,
              MessageBoxIcon.Information);
      reloadBackgroundItems();
    }

    private void themeCancel_Click(object sender, EventArgs e)
    {
      menuThemeForm.Hide();
    }

    private void themeReset_Click(object sender, EventArgs e)
    {
      resetTheme();
      menuThemeForm.Hide();
      MessageBox.Show("The background theme has been reset to skin defaults\n\nPlease check and adjust backgrounds on the 'Default Background Images' tab.",
                      "Background Theme Reset",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Information);
      reloadBackgroundItems();
    }

    private void matchBackground(string background)
    {
      lbMenuItem.Text = background;
    }

    private void applybackgrounds()
    {
      for (int i = 0; i < formMustayalucaEditor.menuItems.Count; i++)
      {
        menuItems[i].fanartHandlerEnabled = false;
        menuItems[i].disableBGSharing = true;
        menuItems[i].bgFolder = "3DBackgrounds";
        menuItems[i].defaultImage = bestThemeMatch(menuItems[i].hyperlink);
      }
    }

    private void resetTheme()
    {
      for (int i = 0; i < formMustayalucaEditor.menuItems.Count; i++)
      {
        menuItems[i].fanartHandlerEnabled = false;
        menuItems[i].disableBGSharing = false;
        menuItems[i].bgFolder = bgFolder(menuItems[i].hyperlink);
        menuItems[i].defaultImage = bgFolderName + "\\" + menuItems[i].bgFolder + "\\default.jpg";
      }
    }

    private string bgFolder(string hyperlink)
    {
      switch (int.Parse(hyperlink))
      {
        case 1:
          return "tv";
        case 2:
          return "pictures";
        case 4:
          return "settings";
        case 6:
          return "movies";
        case 30:
          return "music";
        case 34:
          return "pluginsbg";
        case 501:
        case 504:
          return "music";
        case 2600:
          return "weatherbg";
        case 4755:
          return "movies";
        case 5900:
          return "movies";
        case 7890:
          return "music";
        case 9811:
          return "tv";
        case 16001:
          return "news";
        case 96742:
          return "movies";
        case 25650:
          return "music";
        case 47286:
          return "music";
        case 711992:
          return "movies";
        default:
          return "tv";
      }
    }


    private string bestThemeMatch(string hyperlink)
    {
      switch (int.Parse(hyperlink))
      {
        case 1:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_TVGuide.jpg");
        case 2:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_Pictures.jpg");
        case 4:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_Settings.jpg");
        case 6:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_Videos.jpg");
        case 30:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_Radio.jpg");
        case 34:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_Plugins.jpg");
        case 501:
        case 504:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_Music.jpg");
        case 2600:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_Weather.jpg");
        case 4755:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_OnlineVideos.jpg");
        case 5900:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_Trailers.jpg");
        case 7890:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_Music.jpg");
        case 9811:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_TVSeries.jpg");
        case 16001:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_News.jpg");
        case 96742:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_MovingPictures.jpg");
        case 25650:
        case 25653:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_RadioTime.jpg");
        case 47286:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_Rockstar.jpg");
        case 711992:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\My_OnlineVideos.jpg");
        default:
          return bgFolderName + setBGFolder(cboThemeSelection.Text + "\\default.jpg");
      }
    }

    private string setBGFolder(string folder)
    {
      if (File.Exists(Path.Combine(SkinInfo.mpPaths.Mustayalucapath + "\\media\\" + bgFolderName, folder)))
        return "\\" + folder;
      else
        return "\\" + cboThemeSelection.Text + "\\default.jpg";
    }

    private void buildThemeScreen()
    {
      getThemeImages(menuThemeName);
      //Main Theme Form
      menuThemeForm.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      menuThemeForm.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      menuThemeForm.Name = "frmMenuThemes";
      menuThemeForm.Text = "Menu Background Themes";
      menuThemeForm.ControlBox = true;
      menuThemeForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      menuThemeForm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      menuThemeForm.StartPosition = FormStartPosition.CenterScreen;
      menuThemeForm.Width = 750;
      menuThemeForm.Height = 300;
      menuThemeForm.MaximizeBox = false;
      menuThemeForm.MinimizeBox = false;
      menuThemeForm.TopMost = true;
      menuThemeForm.ControlBox = false;
      //Apply Theme Button
      btThemeOk.Width = 200;
      btThemeOk.Text = "Apply Selected Theme";
      btThemeOk.Location = new System.Drawing.Point(400, 217);
      btThemeOk.Click += new System.EventHandler(themeEnable_Click);
      menuThemeForm.Controls.Add(btThemeOk);
      //Cancel Button
      btThemeCancel.Width = 120;
      btThemeCancel.Text = "Cancel";
      btThemeCancel.Location = new System.Drawing.Point(600, 217);
      btThemeCancel.Click += new System.EventHandler(themeCancel_Click);
      menuThemeForm.Controls.Add(btThemeCancel);
      // Reset to skin default theme
      btReset.Width = 200;
      btReset.Text = "Reset to Default";
      btReset.Location = new System.Drawing.Point(400, 243);
      btReset.Click += new System.EventHandler(themeReset_Click);
      menuThemeForm.Controls.Add(btReset);
      // Previw Image PictureBox
      pbThemePreview.Width = 350;
      pbThemePreview.Height = 197;
      pbThemePreview.BorderStyle = BorderStyle.FixedSingle;
      pbThemePreview.Location = new Point(10, 40);
      pbThemePreview.SizeMode = PictureBoxSizeMode.StretchImage;
      pbThemePreview.Image = (Bitmap)Image.FromFile(menuThemeFiles[themeImageIndex]).Clone();
      menuThemeForm.Controls.Add(pbThemePreview);
      //Previous Image Button
      btThemePrev.Width = 50;
      btThemePrev.Text = "Prev";
      btThemePrev.Location = new System.Drawing.Point(10, 240);
      btThemePrev.Click += new System.EventHandler(btThemePrev_Click);
      menuThemeForm.Controls.Add(btThemePrev);
      //Next Image Button
      btThemeNext.Width = 50;
      btThemeNext.Text = "Next";
      btThemeNext.Location = new System.Drawing.Point(310, 240);
      btThemeNext.Click += new System.EventHandler(btThemeNext_Click);
      menuThemeForm.Controls.Add(btThemeNext);
      // Theme Lable
      lbTheme.Location = new Point(10, 12);
      lbTheme.Width = 43;
      lbTheme.Text = "Theme:";
      menuThemeForm.Controls.Add(lbTheme);
      //Theme selection combobox
      cboThemeSelection.Location = new Point(56, 10);
      cboThemeSelection.Width = 200;
      cboThemeSelection.Items.Add("3DBackgrounds");
      cboThemeSelection.SelectedIndex = 0;
      cboThemeSelection.SelectedIndexChanged += new System.EventHandler(cboThemeSelection_SelectedIndexChanged);
      menuThemeForm.Controls.Add(cboThemeSelection);
      // Menu Item Lable
      lbMenuItem.Location = new Point(60, 240);
      lbMenuItem.Width = 250;
      lbMenuItem.TextAlign = ContentAlignment.MiddleCenter;
      lbMenuItem.Text = "Default Background";
      menuThemeForm.Controls.Add(lbMenuItem);
      // Textbox
      tbThemeInfo.Text = "Menu themes are a set a themed backgrounds that are applied to all Menu Items currently defined. One theme is supplied though others maybe added later either by us or the community.";
      tbThemeInfo.AppendText(Environment.NewLine);
      tbThemeInfo.AppendText(Environment.NewLine + "Applying this theme will set all Menu Items to fixed background and disable background sharing. Items added after will need to be  manually configured for the select theme.");
      tbThemeInfo.AppendText(Environment.NewLine);
      tbThemeInfo.AppendText(Environment.NewLine + "Most major plugins are supported and a best match will be made  background images can be changed via the 'Default Background Images' tab.");
      tbThemeInfo.Location = new System.Drawing.Point(20, 20);
      tbThemeInfo.WordWrap = true;
      tbThemeInfo.Width = 325;
      tbThemeInfo.Height = 200;
      tbThemeInfo.Location = new Point(400, 10);
      tbThemeInfo.BorderStyle = BorderStyle.Fixed3D;
      tbThemeInfo.Multiline = true;
      tbThemeInfo.ReadOnly = true;
      menuThemeForm.Controls.Add(tbThemeInfo);
    }

    void loadBackgroundDirs()
    {
      bgBox.Items.Clear();
      string[] directories = Directory.GetDirectories(SkinInfo.mpPaths.Mustayalucapath + "media\\" + bgFolderName);
      foreach (string folder in directories)
      {
        bgBox.Items.Add(Path.GetFileName(folder));
      }
    }


    void buildQuickListInfo()
    {
      formQlPopup.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      formQlPopup.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      formQlPopup.Name = "frmQLPopup";
      formQlPopup.Text = "Quicklist Item Details";
      formQlPopup.ControlBox = true;
      formQlPopup.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      formQlPopup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      formQlPopup.StartPosition = FormStartPosition.CenterScreen;
      formQlPopup.Width = 300;
      formQlPopup.Height = 300;
      formQlPopup.MaximizeBox = false;
      formQlPopup.MinimizeBox = false;
      formQlPopup.TopMost = true;
      formQlPopup.ControlBox = false;
      // Quicklist Name


      lbQlPopup.Location = new Point(50, 50);
      lbQlPopup.Width = 250;
      lbQlPopup.Text = "Quicklist Item name.....";
      formQlPopup.Controls.Add(lbQlPopup);
    }

    private void rbMovPicsFanart_CheckedChanged(object sender, EventArgs e)
    {
      if (rbMovPicsFanart.Checked)
        mrMovPicsSummStyle = mostRecentMovPicsSummaryStyle.fanart;
      else
        mrMovPicsSummStyle = mostRecentMovPicsSummaryStyle.plot;

      displayMovPicsPreview(mrMovPicsSummStyle);
    }

    private void rbMovPicsPlot_CheckedChanged(object sender, EventArgs e)
    {
      if (rbMovPicsFanart.Checked)
        mrMovPicsSummStyle = mostRecentMovPicsSummaryStyle.fanart;
      else
        mrMovPicsSummStyle = mostRecentMovPicsSummaryStyle.plot;

      displayMovPicsPreview(mrMovPicsSummStyle);
    }

    private void rbTVSeriesFanrt_CheckedChanged(object sender, EventArgs e)
    {
      if (rbTVSeriesFanrt.Checked)
        mrTVSeriesSummStyle = mostRecentTVSeriesSummaryStyle.fanart;
      else
        mrTVSeriesSummStyle = mostRecentTVSeriesSummaryStyle.plot;

      displayTVSeriesPreview(mrTVSeriesSummStyle);
    }

    private void rbTVSeriesPlot_CheckedChanged(object sender, EventArgs e)
    {
      if (rbTVSeriesFanrt.Checked)
        mrTVSeriesSummStyle = mostRecentTVSeriesSummaryStyle.fanart;
      else
        mrTVSeriesSummStyle = mostRecentTVSeriesSummaryStyle.plot;

      displayTVSeriesPreview(mrTVSeriesSummStyle);
    }

    private void rbMusicStyle1_CheckedChanged(object sender, EventArgs e)
    {
      if (rbMusicStyle1.Checked)
        mrMusicStyle = musicMostRecentStyle.style1;
      else
        mrMusicStyle = musicMostRecentStyle.style2;

      displayMusicPreview(mrMusicStyle);
    }

    private void rbMusicStyle2_CheckedChanged(object sender, EventArgs e)
    {
      if (rbMusicStyle1.Checked)
        mrMusicStyle = musicMostRecentStyle.style1;
      else
        mrMusicStyle = musicMostRecentStyle.style2;

      displayMusicPreview(mrMusicStyle);
    }

    void displayMovPicsPreview(mostRecentMovPicsSummaryStyle mrMovPicsSummStyle)
    {
      if (mrMovPicsSummStyle == mostRecentMovPicsSummaryStyle.fanart)
      {
        pbOverlayPreview.Image = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("MustayalucaEditor.previews.movingpictures_fanart.jpg"));
      }
      else
      {
        pbOverlayPreview.Image = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("MustayalucaEditor.previews.movingpictures_plot.jpg"));
      }
    }

    void displayTVSeriesPreview(mostRecentTVSeriesSummaryStyle mrTVSeriesSummStyle)
    {
      if (mrTVSeriesSummStyle == mostRecentTVSeriesSummaryStyle.fanart)
      {
        pbOverlayPreview.Image = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("MustayalucaEditor.previews.tvseries_fanart.jpg"));
      }
      else
      {
        pbOverlayPreview.Image = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("MustayalucaEditor.previews.tvseries_plot.jpg"));
      }
    }

    void displayMusicPreview(musicMostRecentStyle musicStyle)
    {
      if (musicStyle == musicMostRecentStyle.style1)
      {
        pbOverlayPreview.Image = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("MustayalucaEditor.previews.music_style1.jpg"));
      }
      else
      {
        pbOverlayPreview.Image = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("MustayalucaEditor.previews.music_style2.jpg"));
      }
    }

    private void gbMostRecentMovPics_MouseHover(object sender, EventArgs e)
    {
      displayMovPicsPreview(mrMovPicsSummStyle);
    }

    private void gbMostRecentTVSeries_MouseHover(object sender, EventArgs e)
    {
      displayTVSeriesPreview(mrTVSeriesSummStyle);
    }

    private void gpMostRecentMusic_MouseHover(object sender, EventArgs e)
    {
      displayMusicPreview(mrMusicStyle);
    }

    #endregion

    #region Parmeter Handling

    /// <summary>
    /// Get list of views in TVseries database
    /// Key: should be used as hyperlinkParameter
    /// Val: can be used as a default display name
    /// </summary>    
    private List<KeyValuePair<string, string>> GetTVSeriesViews()
    {
      // check if we have already got them
      if (tvseriesViews.Count == 0)
        tvseriesViews = DBView.GetSkinViews();

      return tvseriesViews;
    }

    private List<KeyValuePair<string, string>> GetOnlineVideosViews()
    {
      // check if we have already got them
      if (onlineVideosViews.Count == 0)
      {
        // set path of config file, so we load user settings
        OnlineVideoSettings.Instance.ConfigDir = SkinInfo.mpPaths.configBasePath;

        // load list of sites
        OnlineVideoSettings onlineVideos = OnlineVideos.OnlineVideoSettings.Instance;
        onlineVideos.LoadSites();

        foreach (SiteSettings site in onlineVideos.SiteSettingsList)
        {
          // just get a list of enabled sites
          if (site.IsEnabled)
          {
            KeyValuePair<string, string> view = new KeyValuePair<string, string>(site.Name, site.Name);
            onlineVideosViews.Add(view);
          }
        }

      }

      return onlineVideosViews;
    }

    /// <summary>
    /// Get list of views in Music Views database
    /// Key: should be used as hyperlinkParameter
    /// Val: can be used as a default display name
    /// </summary>    
    private List<KeyValuePair<string, string>> GetMusicViews()
    {
      if (musicViews.Count == 0)
      {
        MusicViewHandler MusicViews = new MusicViewHandler();
        foreach (ViewDefinition MusicView in MusicViews.Views)
        {
          string viewKey = MusicView.Name;
          string viewValue = MusicView.LocalizedName;
          KeyValuePair<string, string> skinview = new KeyValuePair<string, string>(viewKey, viewValue);
          musicViews.Add(skinview);
        }
      }

      return musicViews;
    }

    #endregion

    #region ISetupForm Members

    /// <summary>
    /// Returns the name of the plugin which is shown in the plugin menu
    /// </summary>
    /// <returns>the name of the plugin which is shown in the plugin menu</returns>
    public string PluginName()
    {
      return "Mustayaluca BasicHome Editor";
    }

    /// <summary>
    /// Returns the description of the plugin which is shown in the plugin menu
    /// </summary>
    /// <returns>the description of the plugin which is shown in the plugin menu</returns>
    public string Description()
    {
      return "Mustayaluca BasicHome Editor";
    }

    /// <summary>
    /// Returns the author of the plugin which is shown in the plugin menu
    /// </summary>
    /// <returns>the author of the plugin which is shown in the plugin menu</returns>
    public string Author()
    {
      return "The Mustayaluca Team";
    }

    /// <summary>
    /// Indicates whether plugin can be enabled/disabled
    /// </summary>
    public void ShowPlugin()
    {
      formMustayalucaEditor startEditor = new formMustayalucaEditor();
      startEditor.ShowDialog();
    }

    /// <summary>
    /// Indicates whether plugin can be enabled/disabled
    /// </summary>
    /// <returns>true if the plugin can be enabled/disabled</returns>
    public bool CanEnable()
    {
      return false;
    }

    /// <summary>
    /// Get Windows-ID
    /// </summary>
    /// <returns>unique id for this plugin</returns>
    public int GetWindowId()
    {
      // WindowID of windowplugin belonging to this setup
      // enter your own unique code
      return -1;
    }

    /// <summary>
    /// Indicates if plugin is enabled by default
    /// </summary>
    /// <returns>true if this plugin is enabled by default</returns>
    public bool DefaultEnabled()
    {
      return true;
    }

    /// <summary>
    /// indicates if a plugin has its own setup screen
    /// </summary>
    /// <returns>true if the plugin has its own setup screen</returns>
    public bool HasSetup()
    {
      return true;
    }

    /// <summary>
    /// no Home for this plugin, return false
    /// </summary>
    /// <param name="strButtonText"></param>
    /// <param name="strButtonImage"></param>
    /// <param name="strButtonImageFocus"></param>
    /// <param name="strPictureImage"></param>
    /// <returns></returns>
    public bool GetHome(out string strButtonText, out string strButtonImage,
                        out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = String.Empty;
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;
      return false;
    }

    #endregion

  }
}



