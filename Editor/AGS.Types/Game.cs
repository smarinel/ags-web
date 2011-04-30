using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Xml;

namespace AGS.Types
{
    public class Game : IGame
    {
		private readonly string[] RESERVED_SCRIPT_NAMES = { "inventory", "character", 
			"views", "player", "object", "mouse", "system", "game", "palette",
			"hotspot", "region", "dialog", "gui", "GUI"};
        private const int PALETTE_SIZE = 256;
        private const int NUMBER_OF_GLOBAL_MESSAGES = 500;
        private const int GLOBAL_MESSAGE_ID_START = 500;

        public const int MAX_DIALOGS = 500;
        public const int MAX_INV_ITEMS = 300;
        public const int MAX_SPRITES = 30000;

        public delegate void ViewListUpdatedHandler();
        /// <summary>
        /// Fired when an external client adds/removes views
        /// </summary>
        public event ViewListUpdatedHandler ViewListUpdated;

        private List<GUI> _guis;
        private List<InventoryItem> _inventoryItems;
        private List<Character> _characters;
        private List<MouseCursor> _cursors;
        private List<Font> _fonts;
        private List<Dialog> _dialogs;
        private List<Plugin> _plugins;
        private List<Translation> _translations;
        private RoomList _rooms;
        private List<OldInteractionVariable> _oldInteractionVariables;
        private string[] _globalMessages;
        private Character _playerCharacter;
        private Settings _settings;
        private PaletteEntry[] _palette;
        private SpriteFolder _sprites;
        private ViewFolder _views;
        private AudioClipFolder _audioClips;
        private List<AudioClipType> _audioClipTypes;
        private Scripts _scripts;
        private Scripts _scriptsToCompile;
        private TextParser _textParser;
        private LipSync _lipSync;
        private CustomPropertySchema _propertySchema;
        private GlobalVariables _globalVariables;
        private IList<AudioClip> _cachedAudioClipListForCompile;
        private Dictionary<int, int> _cachedAudioClipIndexMapping;
		private string _directoryPath;
		private bool _roomsAddedOrRemoved = false;
		private Dictionary<int, object> _deletedViewIDs;
		private string _savedXmlVersion = null;
        private int? _savedXmlVersionIndex = null;
        private string _savedXmlEditorVersion = null;

        public Game()
        {
            _guis = new List<GUI>();
            _inventoryItems = new List<InventoryItem>();
            _cursors = new List<MouseCursor>();
            _dialogs = new List<Dialog>();
            _fonts = new List<Font>();
            _characters = new List<Character>();
            _plugins = new List<Plugin>();
            _translations = new List<Translation>();
            _rooms = new RoomList();
            _oldInteractionVariables = new List<OldInteractionVariable>();
            _settings = new Settings();
            _palette = new PaletteEntry[PALETTE_SIZE];
            _sprites = new SpriteFolder("Main");
            _views = new ViewFolder("Main");
            _audioClips = new AudioClipFolder("Main");
            _audioClipTypes = new List<AudioClipType>();
            _textParser = new TextParser();
            _lipSync = new LipSync();
            _propertySchema = new CustomPropertySchema();
            _globalVariables = new GlobalVariables();
            _globalMessages = new string[NUMBER_OF_GLOBAL_MESSAGES];
			_deletedViewIDs = new Dictionary<int, object>();
            _scripts = new Scripts();
            _scriptsToCompile = new Scripts();
            _scripts.Add(new Script(Script.GLOBAL_HEADER_FILE_NAME, "// script header\r\n", true));
            _scripts.Add(new Script(Script.GLOBAL_SCRIPT_FILE_NAME, "// global script\r\n", false));
            _playerCharacter = null;

            for (int i = 0; i < _globalMessages.Length; i++)
            {
                _globalMessages[i] = string.Empty;
            }

            InitializeDefaultPalette();
        }

        public string[] GlobalMessages
        {
            get { return _globalMessages; }
        }

        public IList<GUI> GUIs
        {
            get { return _guis; }
        }

        public IList<InventoryItem> InventoryItems
        {
            get { return _inventoryItems; }
        }

        public IList<Character> Characters
        {
            get { return _characters; }
        }

        public IList<Dialog> Dialogs
        {
            get { return _dialogs; }
        }

        public IList<MouseCursor> Cursors
        {
            get { return _cursors; }
        }

        public IList<Font> Fonts
        {
            get { return _fonts; }
        }

        public IList<Translation> Translations
        {
            get { return _translations; }
        }

        public List<Plugin> Plugins
        {
            get { return _plugins; }
        }

        public IList<IRoom> Rooms
        {
            get { return _rooms; }
        }

        public List<OldInteractionVariable> OldInteractionVariables
        {
            get { return _oldInteractionVariables; }
        }

        public CustomPropertySchema PropertySchema
        {
            get { return _propertySchema; }
        }

        public Character PlayerCharacter
        {
            get { return _playerCharacter; }
            set { _playerCharacter = value; }
        }

        public Settings Settings
        {
            get { return _settings; }
        }

        public PaletteEntry[] Palette
        {
            get { return _palette; }
        }

        public TextParser TextParser
        {
            get { return _textParser; }
        }

        public LipSync LipSync
        {
            get { return _lipSync; }
        }

        public SpriteFolder RootSpriteFolder
        {
            get { return _sprites; }
            set { _sprites = value; }
        }

        public ViewFolder RootViewFolder
        {
            get { return _views; }
            set { _views = value; }
        }

        public AudioClipFolder RootAudioClipFolder
        {
            get { return _audioClips; }
        }

        public IList<AudioClipType> AudioClipTypes
        {
            get { return _audioClipTypes; }
        }

        public Scripts Scripts
        {
            get { return _scripts; }
        }

        // Used by the AGF->DTA compiler to bring in any extra modules
        public Scripts ScriptsToCompile
        {
            get { return _scriptsToCompile; }
            set { _scriptsToCompile = value; }
        }

        public IList<AudioClip> CachedAudioClipListForCompile
        {
            get { return _cachedAudioClipListForCompile; }
        }

        public GlobalVariables GlobalVariables
        {
            get { return _globalVariables; }
        }

		IViewFolder IGame.Views
		{
			get { return _views; }
		}

        ISpriteFolder IGame.Sprites
        {
            get { return _sprites; }
        }

		/// <summary>
		/// The version of the Game.agf file that was loaded from disk.
		/// This is null if the game has not yet been saved.
		/// </summary>
		public string SavedXmlVersion
		{
			get { return _savedXmlVersion; }
			set { _savedXmlVersion = value; }
		}

        /// <summary>
        /// The editor version read from the Game.agf file that was loaded from disk.
        /// This is null if the game has not yet been saved or is an older version.
        /// </summary>
        public string SavedXmlEditorVersion
        {
            get { return _savedXmlEditorVersion; }
            set { _savedXmlEditorVersion = value; }
        }

        /// <summary>
        /// The version-index of the Game.agf file that was loaded from disk.
        /// This is null if the game has not yet been saved.
        /// </summary>
        public int? SavedXmlVersionIndex
        {
            get { return _savedXmlVersionIndex; }
            set { _savedXmlVersionIndex = value; }
        }

		/// <summary>
		/// Full path to the directory where the game is located
		/// </summary>
		public string DirectoryPath
		{
			get { return _directoryPath; }
			set { _directoryPath = value; }
		}

		/// <summary>
		/// If this is set, then the editor is more forceful about making
		/// the user save the game on exit.
		/// </summary>
		public bool FilesAddedOrRemoved
		{
			get { return _roomsAddedOrRemoved; }
			set { _roomsAddedOrRemoved = value; }
		}

        /// <summary>
        /// Causes the ViewListUpdated event to be fired. You should call this
        /// if you add/remove views and need the views component to update
        /// to reflect the changes.
        /// </summary>
        public void NotifyClientsViewsUpdated()
        {
            if (ViewListUpdated != null)
            {
                ViewListUpdated();
            }
        }

		/// <summary>
		/// Returns the minimum height of the room background
		/// for the current game resolution.
		/// </summary>
        public int MinRoomHeight
        {
            get
            {
                return Utilities.GetGameResolutionHeight(_settings.Resolution);
            }
        }

		/// <summary>
		/// Returns the minimum width of the room background
		/// for the current game resolution.
		/// </summary>
        public int MinRoomWidth
        {
            get
            {
                return Utilities.GetGameResolutionWidth(_settings.Resolution);
            }
        }

		/// <summary>
		/// Returns the highest numbered View in the game
		/// </summary>
        public int ViewCount
        {
            get
            {
                return FindHighestViewNumber(RootViewFolder);
            }
        }

		/// <summary>
		/// Marks the view as deleted and available for re-creation
		/// </summary>
		public void ViewDeleted(int viewNumber)
		{
			_deletedViewIDs.Add(viewNumber, null);
		}

		public View CreateNewView(IViewFolder createInFolder)
		{
			if (createInFolder == null)
			{
				createInFolder = _views;
			}
			View newView = new View();
			newView.ID = FindAndAllocateAvailableViewID();
			newView.Name = "View" + newView.ID;
			createInFolder.Views.Add(newView);
			NotifyClientsViewsUpdated();
			return newView;
		}

		/// <summary>
		/// Returns an unused View ID and allocates it as in use
		/// </summary>
		public int FindAndAllocateAvailableViewID()
        {
			if (_deletedViewIDs.Count > 0)
			{
				foreach (int availableID in _deletedViewIDs.Keys)
				{
					_deletedViewIDs.Remove(availableID);
					return availableID;
				}
			}
            return FindHighestViewNumber(_views) + 1;
        }

        private int FindHighestViewNumber(ViewFolder folder)
        {
            int highest = 0;
            foreach (View view in folder.Views)
            {
                highest = Math.Max(view.ID, highest);
            }

            foreach (ViewFolder subFolder in folder.SubFolders)
            {
                int highestInSubFolder = FindHighestViewNumber(subFolder);
                highest = Math.Max(highest, highestInSubFolder);
            }

            return highest;
        }

		/// <summary>
		/// Returns the View object associated with the supplied ID
		/// </summary>
		public View FindViewByID(int viewNumber)
        {
            return this.RootViewFolder.FindViewByID(viewNumber, true);
        }

		public Character FindCharacterByID(int charID)
		{
			foreach (Character character in _characters)
			{
				if (character.ID == charID)
				{
					return character;
				}
			}
			return null;
		}

		public UnloadedRoom FindRoomByID(int roomNumber)
        {
            foreach (UnloadedRoom room in _rooms)
            {
                if (room.Number == roomNumber)
                {
                    return room;
                }
            }
            return null;
        }

		public bool DoesRoomNumberAlreadyExist(int roomNumber)
		{
			return (FindRoomByID(roomNumber) != null);
		}

		public int FindFirstAvailableRoomNumber(int startingFromNumber)
		{
			do
			{
				startingFromNumber++;
			}
			while (DoesRoomNumberAlreadyExist(startingFromNumber));
			return startingFromNumber;
		}

        public AudioClip FindAudioClipForOldSoundNumber(IList<AudioClip> allAudio, int soundNumber)
        {
            if (allAudio == null)
            {
                allAudio = _audioClips.GetAllAudioClipsFromAllSubFolders();
            }
            string searchForName = string.Format("aSound{0}", soundNumber);
            foreach (AudioClip clip in allAudio)
            {
                if (clip.ScriptName == searchForName)
                {
                    return clip;
                }
            }
            return null;
        }

        public AudioClip FindAudioClipForOldMusicNumber(IList<AudioClip> allAudio, int musicNumber)
        {
            if (allAudio == null)
            {
                allAudio = _audioClips.GetAllAudioClipsFromAllSubFolders();
            }
            string searchForName = string.Format("aMusic{0}", musicNumber);
            foreach (AudioClip clip in allAudio)
            {
                if (clip.ScriptName == searchForName)
                {
                    return clip;
                }
            }
            return null;
        }

        public int GUIScaleFactor
        {
            get
            {
                if ((_settings.Resolution == GameResolutions.R320x200) ||
                    (_settings.Resolution == GameResolutions.R320x240))
                {
                    return 2;
                }

                return 1;
            }
        }

        /// <summary>
        /// Returns whether the game is "high resolution" (ie. 640x400 or more)
        /// </summary>
        public bool IsHighResolution
        {
            get { return (this.GUIScaleFactor == 1); }
        }

        public int GetNextAudioIndex()
        {
            return ++_settings.AudioIndexer;
        }

        public PaletteEntry[] ReadPaletteFromXML(XmlNode parentOfPaletteNode)
        {
            PaletteEntry[] palette = new PaletteEntry[PALETTE_SIZE];
            for (int i = 0; i < palette.Length; i++)
            {
                palette[i] = new PaletteEntry(i, _palette[i].Colour);
                palette[i].ColourType = _palette[i].ColourType;
            }
            foreach (XmlNode palNode in SerializeUtils.GetChildNodes(parentOfPaletteNode, "Palette"))
            {
                PaletteEntry paletteEntry = palette[Convert.ToInt32(palNode.Attributes["Index"].InnerText)];
                paletteEntry.Colour = Color.FromArgb(
                    Convert.ToInt32(palNode.Attributes["Red"].InnerText),
                    Convert.ToInt32(palNode.Attributes["Green"].InnerText),
                    Convert.ToInt32(palNode.Attributes["Blue"].InnerText));
                paletteEntry.ColourType = (PaletteColourType)Enum.Parse(typeof(PaletteColourType), palNode.Attributes["Type"].InnerText);
            }
            return palette;
        }

        public void WritePaletteToXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("Palette");

            int i = 0;
            foreach (PaletteEntry entry in _palette)
            {
                writer.WriteStartElement("PaletteEntry");
                writer.WriteAttributeString("Index", i.ToString());
                writer.WriteAttributeString("Type", entry.ColourType.ToString());
                writer.WriteAttributeString("Red", entry.Colour.R.ToString());
                writer.WriteAttributeString("Green", entry.Colour.G.ToString());
                writer.WriteAttributeString("Blue", entry.Colour.B.ToString());
                writer.WriteEndElement();
                i++;
            }

            writer.WriteEndElement();
        }

        public void ToXml(XmlTextWriter writer)
        {
            writer.WriteStartElement("Game");

            _settings.ToXml(writer);

            _lipSync.ToXml(writer);

            _propertySchema.ToXml(writer);

            writer.WriteStartElement("GlobalMessages");
            int messageIndex = GLOBAL_MESSAGE_ID_START;
            foreach (string message in _globalMessages)
            {
                writer.WriteStartElement("Message");
                writer.WriteAttributeString("ID", messageIndex.ToString());
                writer.WriteValue(message);
                writer.WriteEndElement();
                messageIndex++;
            }
            writer.WriteEndElement();

			// We need to serialize the interaction variables in case
			// they don't upgrade a room until later, and it might
			// use the global interaction variables
			writer.WriteStartElement("OldInteractionVariables");
			foreach (OldInteractionVariable var in _oldInteractionVariables)
			{
				writer.WriteStartElement("Variable");
				writer.WriteAttributeString("Name", var.Name);
				writer.WriteAttributeString("Value", var.Value.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("Plugins");
            foreach (Plugin plugin in _plugins)
            {
                plugin.ToXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Rooms");
            foreach (UnloadedRoom room in _rooms)
            {
                room.ToXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("GUIs");
            foreach (GUI gui in _guis)
            {
                gui.ToXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("InventoryItems");
            foreach (InventoryItem item in _inventoryItems)
            {
                item.ToXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("TextParser");
            _textParser.ToXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("Characters");
            foreach (Character character in _characters)
            {
                character.ToXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteElementString("PlayerCharacter", (_playerCharacter == null) ? string.Empty : _playerCharacter.ID.ToString());

            writer.WriteStartElement("Dialogs");
            foreach (Dialog dialog in _dialogs)
            {
                dialog.ToXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Cursors");
            foreach (MouseCursor cursor in _cursors)
            {
                cursor.ToXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Fonts");
            foreach (Font font in _fonts)
            {
                font.ToXml(writer);
            }
            writer.WriteEndElement();

            WritePaletteToXML(writer);

            writer.WriteStartElement("GlobalVariables");
            _globalVariables.ToXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("Sprites");
            _sprites.ToXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("Views");
            _views.ToXml(writer);
            writer.WriteEndElement();

			writer.WriteStartElement("DeletedViews");
			foreach (int viewID in _deletedViewIDs.Keys)
			{
				writer.WriteElementString("ViewID", viewID.ToString());
			}
			writer.WriteEndElement();

            _scripts.ToXml(writer);

            writer.WriteStartElement("AudioClips");
            _audioClips.ToXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("AudioClipTypes");
            foreach (AudioClipType audioClipType in _audioClipTypes)
            {
                audioClipType.ToXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Translations");
            foreach (Translation translation in _translations)
            {
                translation.ToXml(writer);
            }            
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        public void FromXml(XmlNode node)
        {
            node = node.SelectSingleNode("Game");

            _settings.FromXml(node);
            _lipSync.FromXml(node);
            _propertySchema.FromXml(node);

            if (node.SelectSingleNode("InventoryHotspotMarker") != null)
            {
                // Pre-3.0.3
                InventoryHotspotMarker marker = new InventoryHotspotMarker();
                marker.FromXml(node);
                _settings.InventoryHotspotMarker = marker;
            }

            foreach (XmlNode msgNode in SerializeUtils.GetChildNodes(node, "GlobalMessages"))
            {
                _globalMessages[Convert.ToInt32(msgNode.Attributes["ID"].InnerText) - GLOBAL_MESSAGE_ID_START] = msgNode.InnerText;
            }

            _plugins.Clear();
            foreach (XmlNode pluginNode in SerializeUtils.GetChildNodes(node, "Plugins"))
            {
                _plugins.Add(new Plugin(pluginNode));
            }

            _rooms.Clear();
            foreach (XmlNode roomNode in SerializeUtils.GetChildNodes(node, "Rooms"))
            {
                _rooms.Add(new UnloadedRoom(roomNode));
            }

            _guis.Clear();
            foreach (XmlNode guiNode in SerializeUtils.GetChildNodes(node, "GUIs"))
            {
                if (guiNode.FirstChild.Name == NormalGUI.XML_ELEMENT_NAME)
                {
                    _guis.Add(new NormalGUI(guiNode));
                }
                else
                {
                    _guis.Add(new TextWindowGUI(guiNode));
                }
            }

            _inventoryItems.Clear();
            foreach (XmlNode invNode in SerializeUtils.GetChildNodes(node, "InventoryItems"))
            {
                _inventoryItems.Add(new InventoryItem(invNode));
            }

            _textParser = new TextParser(node.SelectSingleNode("TextParser"));

            _characters.Clear();
            foreach (XmlNode invNode in SerializeUtils.GetChildNodes(node, "Characters"))
            {
                _characters.Add(new Character(invNode));
            }

            _playerCharacter = null;
            string playerCharText = SerializeUtils.GetElementString(node, "PlayerCharacter");
            if (playerCharText.Length > 0)
            {
                int playerCharID = Convert.ToInt32(playerCharText);
                foreach (Character character in _characters)
                {
                    if (character.ID == playerCharID)
                    {
                        _playerCharacter = character;
                        break;
                    }
                }
            }

            _dialogs.Clear();
            foreach (XmlNode dialogNode in SerializeUtils.GetChildNodes(node, "Dialogs"))
            {
                _dialogs.Add(new Dialog(dialogNode));
            }

            _cursors.Clear();
            foreach (XmlNode cursNode in SerializeUtils.GetChildNodes(node, "Cursors"))
            {
                _cursors.Add(new MouseCursor(cursNode));
            }

            _fonts.Clear();
            foreach (XmlNode fontNode in SerializeUtils.GetChildNodes(node, "Fonts"))
            {
                _fonts.Add(new Font(fontNode));
            }

            _palette = ReadPaletteFromXML(node);

            _sprites = new SpriteFolder(node.SelectSingleNode("Sprites").FirstChild);

            _views = new ViewFolder(node.SelectSingleNode("Views").FirstChild);

            _deletedViewIDs.Clear();
			if (node.SelectSingleNode("DeletedViews") != null)
			{
				foreach (XmlNode transNode in SerializeUtils.GetChildNodes(node, "DeletedViews"))
				{
					_deletedViewIDs.Add(Convert.ToInt32(transNode.InnerText), null);
				}
			}

            _scripts = new Scripts(node);

            if (node.SelectSingleNode("AudioClips") != null)
            {
                _audioClips = new AudioClipFolder(node.SelectSingleNode("AudioClips").FirstChild);
            }
            else
            {
                _audioClips = new AudioClipFolder("Main");
                _audioClips.DefaultPriority = AudioClipPriority.Normal;
                _audioClips.DefaultRepeat = InheritableBool.False;
                _audioClips.DefaultVolume = 100;
            }

            _audioClipTypes.Clear();
            if (node.SelectSingleNode("AudioClipTypes") != null)
            {
                foreach (XmlNode clipTypeNode in SerializeUtils.GetChildNodes(node, "AudioClipTypes"))
                {
                    _audioClipTypes.Add(new AudioClipType(clipTypeNode));
                }
            }

            _translations.Clear();
            if (node.SelectSingleNode("Translations") != null)
            {
                foreach (XmlNode transNode in SerializeUtils.GetChildNodes(node, "Translations"))
                {
                    _translations.Add(new Translation(transNode));
                }
            }

            if (node.SelectSingleNode("GlobalVariables") != null)
            {
                _globalVariables = new GlobalVariables(node.SelectSingleNode("GlobalVariables"));
            }
            else
            {
                _globalVariables = new GlobalVariables();
            }

			_oldInteractionVariables.Clear();
			if (node.SelectSingleNode("OldInteractionVariables") != null)
			{
				foreach (XmlNode varNode in SerializeUtils.GetChildNodes(node, "OldInteractionVariables"))
				{
					_oldInteractionVariables.Add(new OldInteractionVariable(SerializeUtils.GetAttributeString(varNode, "Name"), SerializeUtils.GetAttributeInt(varNode, "Value")));
				}
			}

            if (_savedXmlVersionIndex == null)
            {
                // Pre-3.0.3, upgrade co-ordinates
                ConvertCoordinatesToNativeResolution();
            }
        }

        public bool IsScriptNameAlreadyUsed(string tryName, object ignoreObject)
        {
            if (tryName == string.Empty)
            {
                return false;
            }

			foreach (string name in RESERVED_SCRIPT_NAMES)
			{
				if (tryName == name)
				{
					return true;
				}
			}

            foreach (GUI gui in this.GUIs)
            {
                if (gui != ignoreObject)
                {
                    if (gui.Name == tryName)
                    {
                        return true;
                    }

                    if (gui.Name.StartsWith("g") &&
                        (gui.Name.Length > 1) &&
                        (gui.Name.Substring(1).ToUpper() == tryName))
                    {
                        return true;
                    }
                }

                foreach (GUIControl control in gui.Controls)
                {
                    if ((control.Name == tryName) && (control != ignoreObject))
                    {
                        return true;
                    }
                }
            }

            foreach (InventoryItem item in this.InventoryItems)
            {
                if ((item.Name == tryName) && (item != ignoreObject))
                {
                    return true;
                }
            }

            foreach (Character character in this.Characters)
            {
                if (character != ignoreObject)
                {
                    if (character.ScriptName == tryName)
                    {
                        return true;
                    }

                    if (character.ScriptName.StartsWith("c") &&
                        (character.ScriptName.Length > 1) &&
                        (character.ScriptName.Substring(1).ToUpper() == tryName))
                    {
                        return true;
                    }
                }
            }

            foreach (Dialog dialog in this.Dialogs)
            {
                if ((dialog.Name == tryName) && (dialog != ignoreObject))
                {
                    return true;
                }
            }

            if (IsNameUsedByAudioClip(tryName, this.RootAudioClipFolder, ignoreObject))
            {
                return true;
            }

            if (IsNameUsedByView(tryName, this.RootViewFolder, ignoreObject))
            {
                return true;
            }

            if ((_globalVariables[tryName] != null) &&
                (_globalVariables[tryName] != ignoreObject))
            {
                return true;
            }

            return false;
        }

        private bool IsNameUsedByAudioClip(string name, AudioClipFolder folderToCheck, object ignoreObject)
        {
            foreach (AudioClip clip in folderToCheck.Items)
            {
                if ((clip.ScriptName == name) && (clip != ignoreObject))
                {
                    return true;
                }
            }

            foreach (AudioClipFolder subFolder in folderToCheck.SubFolders)
            {
                if (IsNameUsedByAudioClip(name, subFolder, ignoreObject))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNameUsedByView(string name, ViewFolder folderToCheck, object ignoreObject)
        {
            foreach (View view in folderToCheck.Views)
            {
                if ((view.Name.ToUpper() == name) && (view != ignoreObject))
                {
                    return true;
                }
            }

            foreach (ViewFolder subFolder in folderToCheck.SubFolders)
            {
                if (IsNameUsedByView(name, subFolder, ignoreObject))
                {
                    return true;
                }
            }

            return false;
        }

        public List<Script> GetAllGameAndLoadedRoomScripts()
        {
            List<Script> scripts = new List<Script>();
            foreach (Script script in this.Scripts)
            {
                scripts.Add(script);
            }
            foreach (UnloadedRoom room in this.Rooms)
            {
                if (room.Script != null)
                {
                    scripts.Add(room.Script);
                }
            }
            return scripts;
        }

        public void UpdateCachedAudioClipList()
        {
            _cachedAudioClipListForCompile = _audioClips.GetAllAudioClipsFromAllSubFolders();
            _cachedAudioClipIndexMapping = new Dictionary<int, int>();
            for (int i = 0; i < _cachedAudioClipListForCompile.Count; i++)
            {
                _cachedAudioClipIndexMapping.Add(_cachedAudioClipListForCompile[i].Index, i);
            }
        }

        public int GetAudioArrayIndexFromAudioClipIndex(int audioClipIndex)
        {
            if (audioClipIndex > 0)
            {
                if (_cachedAudioClipIndexMapping.ContainsKey(audioClipIndex))
                {
                    return _cachedAudioClipIndexMapping[audioClipIndex];
                }
            }
            return -1;
        }

        public byte[] GetPaletteAsRawPAL()
        {
            byte[] rawPalette = new byte[768];
            for (int i = 0; i < _palette.Length; i++)
            {
                rawPalette[i * 3] = (byte)(_palette[i].Colour.R / 4);
                rawPalette[i * 3 + 1] = (byte)(_palette[i].Colour.G / 4);
                rawPalette[i * 3 + 2] = (byte)(_palette[i].Colour.B / 4);
            }
            return rawPalette;
        }

        public void SetPaletteFromRawPAL(byte[] rawPalette, bool resetColourTypes)
        {
            for (int i = 0; i < _palette.Length; i++)
            {
                _palette[i] = new PaletteEntry(i, Color.FromArgb(rawPalette[i * 3] * 4, rawPalette[i * 3 + 1] * 4, rawPalette[i * 3 + 2] * 4));
                if (resetColourTypes)
                {
                    if (i <= 41)
                    {
                        _palette[i].ColourType = PaletteColourType.Gamewide;
                    }
                    else
                    {
                        _palette[i].ColourType = PaletteColourType.Background;
                    }
                }
            }
        }

        private void InitializeDefaultPalette()
        {
            Stream palInput = GetType().Assembly.GetManifestResourceStream("AGS.Types.Resources.roomdef.pal");
            byte[] rawPalette = new byte[768];
            palInput.Read(rawPalette, 0, 768);
            palInput.Close();

            SetPaletteFromRawPAL(rawPalette, true);
        }

        /// <summary>
        /// WARNING: Only call this if an old game has just been loaded
        /// in, otherwise all sizes will get doubled!!!
        /// </summary>
        public void ConvertCoordinatesToNativeResolution()
        {
            if ((_settings.Resolution == GameResolutions.R320x200) ||
                (_settings.Resolution == GameResolutions.R320x240))
            {
                // No conversion necessary -- already at native res
                return;
            }

            const int MULTIPLY_FACTOR = 2;

            foreach (GUI gui in _guis)
            {
                if (gui is NormalGUI)
                {
                    NormalGUI normalGui = (NormalGUI)gui;
                    normalGui.Left *= MULTIPLY_FACTOR;
                    normalGui.Top *= MULTIPLY_FACTOR;
                    normalGui.Width *= MULTIPLY_FACTOR;
                    normalGui.Height *= MULTIPLY_FACTOR;
                }

                foreach (GUIControl control in gui.Controls)
                {
                    control.Left *= MULTIPLY_FACTOR;
                    control.Top *= MULTIPLY_FACTOR;
                    control.Width *= MULTIPLY_FACTOR;
                    control.Height *= MULTIPLY_FACTOR;

                    if (control is GUIInventory)
                    {
                        ((GUIInventory)control).ItemWidth *= MULTIPLY_FACTOR;
                        ((GUIInventory)control).ItemHeight *= MULTIPLY_FACTOR;
                    }
                }
            }

            foreach (MouseCursor cursor in _cursors)
            {
                if (cursor.HotspotX >= 0)
                {
                    cursor.HotspotX *= MULTIPLY_FACTOR;
                    cursor.HotspotY *= MULTIPLY_FACTOR;
                }
            }

            foreach (InventoryItem item in _inventoryItems)
            {
                if (item.HotspotX >= 0)
                {
                    item.HotspotX *= MULTIPLY_FACTOR;
                    item.HotspotY *= MULTIPLY_FACTOR;
                }
            }

            foreach (Character character in _characters)
            {
                character.StartX *= MULTIPLY_FACTOR;
                character.StartY *= MULTIPLY_FACTOR;
            }
        }
    }
}
