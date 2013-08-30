using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

enum SmartTarget
{
    SMART_TARGET_NONE                           = 0,    // NONE, defaulting to invoket
    SMART_TARGET_SELF                           = 1,    // Self cast
    SMART_TARGET_VICTIM                         = 2,    // Our current target (ie: highest aggro)
    SMART_TARGET_HOSTILE_SECOND_AGGRO           = 3,    // Second highest aggro
    SMART_TARGET_HOSTILE_LAST_AGGRO             = 4,    // Dead last on aggro
    SMART_TARGET_HOSTILE_RANDOM                 = 5,    // Just any random target on our threat list
    SMART_TARGET_HOSTILE_RANDOM_NOT_TOP         = 6,    // Any random target except top threat
    SMART_TARGET_ACTION_INVOKER                 = 7,    // Unit who caused this Event to occur
    SMART_TARGET_POSITION                       = 8,    // x, y, z, o
    SMART_TARGET_CREATURE_RANGE                 = 9,    // CreatureEntry(0any), minDist, maxDist
    SMART_TARGET_CREATURE_GUID                  = 10,   // guid, entry
    SMART_TARGET_CREATURE_DISTANCE              = 11,   // CreatureEntry(0any), maxDist
    SMART_TARGET_STORED                         = 12,   // id, uses pre-stored target(list)
    SMART_TARGET_GAMEOBJECT_RANGE               = 13,   // entry(0any), min, max
    SMART_TARGET_GAMEOBJECT_GUID                = 14,   // guid, entry
    SMART_TARGET_GAMEOBJECT_DISTANCE            = 15,   // entry(0any), maxDist
    SMART_TARGET_INVOKER_PARTY                  = 16,   // invoker's party members
    SMART_TARGET_PLAYER_RANGE                   = 17,   // min, max
    SMART_TARGET_PLAYER_DISTANCE                = 18,   // maxDist
    SMART_TARGET_CLOSEST_CREATURE               = 19,   // CreatureEntry(0any), maxDist, dead?
    SMART_TARGET_CLOSEST_GAMEOBJECT             = 20,   // entry(0any), maxDist
    SMART_TARGET_CLOSEST_PLAYER                 = 21,   // maxDist
    SMART_TARGET_ACTION_INVOKER_VEHICLE         = 22,   // Unit's vehicle who caused this Event to occur
    SMART_TARGET_OWNER_OR_SUMMONER              = 23,   // Unit's owner or summoner
    SMART_TARGET_THREAT_LIST                    = 24,   // All units on creature's threat list
    SMART_TARGET_CLOSEST_ENEMY                  = 25,   // maxDist
    SMART_TARGET_CLOSEST_FRIENDLY               = 26   // maxDist
}

enum SmartEventFlags
{
    SMART_EVENT_FLAG_NOT_REPEATABLE        = 0x001,                     //Event can not repeat
    SMART_EVENT_FLAG_DIFFICULTY_0          = 0x002,                     //Event only occurs in instance difficulty 0
    SMART_EVENT_FLAG_DIFFICULTY_1          = 0x004,                     //Event only occurs in instance difficulty 1
    SMART_EVENT_FLAG_DIFFICULTY_2          = 0x008,                     //Event only occurs in instance difficulty 2
    SMART_EVENT_FLAG_DIFFICULTY_3          = 0x010,                     //Event only occurs in instance difficulty 3
    SMART_EVENT_FLAG_RESERVED_5            = 0x020,
    SMART_EVENT_FLAG_RESERVED_6            = 0x040,
    SMART_EVENT_FLAG_DEBUG_ONLY            = 0x080,                     //Event only occurs in debug build
    SMART_EVENT_FLAG_DONT_RESET            = 0x100,                     //Event will not reset in SmartScript::OnReset()

    SMART_EVENT_FLAG_DIFFICULTY_ALL        = (SMART_EVENT_FLAG_DIFFICULTY_0|SMART_EVENT_FLAG_DIFFICULTY_1|SMART_EVENT_FLAG_DIFFICULTY_2|SMART_EVENT_FLAG_DIFFICULTY_3),
    SMART_EVENT_FLAGS_ALL                  = (SMART_EVENT_FLAG_NOT_REPEATABLE|SMART_EVENT_FLAG_DIFFICULTY_ALL|SMART_EVENT_FLAG_RESERVED_5|SMART_EVENT_FLAG_RESERVED_6|SMART_EVENT_FLAG_DEBUG_ONLY|SMART_EVENT_FLAG_DONT_RESET)
}

enum SmartEventPhase
{
    SMART_EVENT_PHASE_ALWAYS  = 0,
    SMART_EVENT_PHASE_1       = 1,
    SMART_EVENT_PHASE_2       = 2,
    SMART_EVENT_PHASE_3       = 3,
    SMART_EVENT_PHASE_4       = 4,
    SMART_EVENT_PHASE_5       = 5,
    SMART_EVENT_PHASE_6       = 6,
    SMART_EVENT_PHASE_MAX     = 7
}

enum SmartEvent
{
    SMART_EVENT_UPDATE_IC                = 0,       // InitialMin, InitialMax, RepeatMin, RepeatMax
    SMART_EVENT_UPDATE_OOC               = 1,       // InitialMin, InitialMax, RepeatMin, RepeatMax
    SMART_EVENT_HEALT_PCT                = 2,       // HPMin%, HPMax%,  RepeatMin, RepeatMax
    SMART_EVENT_MANA_PCT                 = 3,       // ManaMin%, ManaMax%, RepeatMin, RepeatMax
    SMART_EVENT_AGGRO                    = 4,       // NONE
    SMART_EVENT_KILL                     = 5,       // CooldownMin0, CooldownMax1, playerOnly2, else creature entry3
    SMART_EVENT_DEATH                    = 6,       // NONE
    SMART_EVENT_EVADE                    = 7,       // NONE
    SMART_EVENT_SPELLHIT                 = 8,       // SpellID, School, CooldownMin, CooldownMax
    SMART_EVENT_RANGE                    = 9,       // MinDist, MaxDist, RepeatMin, RepeatMax
    SMART_EVENT_OOC_LOS                  = 10,      // NoHostile, MaxRnage, CooldownMin, CooldownMax
    SMART_EVENT_RESPAWN                  = 11,      // type, MapId, ZoneId
    SMART_EVENT_TARGET_HEALTH_PCT        = 12,      // HPMin%, HPMax%, RepeatMin, RepeatMax
    SMART_EVENT_TARGET_CASTING           = 13,      // RepeatMin, RepeatMax, spellid
    SMART_EVENT_FRIENDLY_HEALTH          = 14,      // HPDeficit, Radius, RepeatMin, RepeatMax
    SMART_EVENT_FRIENDLY_IS_CC           = 15,      // Radius, RepeatMin, RepeatMax
    SMART_EVENT_FRIENDLY_MISSING_BUFF    = 16,      // SpellId, Radius, RepeatMin, RepeatMax
    SMART_EVENT_SUMMONED_UNIT            = 17,      // CreatureId(0 all), CooldownMin, CooldownMax
    SMART_EVENT_TARGET_MANA_PCT          = 18,      // ManaMin%, ManaMax%, RepeatMin, RepeatMax
    SMART_EVENT_ACCEPTED_QUEST           = 19,      // QuestID(0any)
    SMART_EVENT_REWARD_QUEST             = 20,      // QuestID(0any)
    SMART_EVENT_REACHED_HOME             = 21,      // NONE
    SMART_EVENT_RECEIVE_EMOTE            = 22,      // EmoteId, CooldownMin, CooldownMax, condition, val1, val2, val3
    SMART_EVENT_HAS_AURA                 = 23,      // Param1 = SpellID, Param2 = Stack amount, Param3/4 RepeatMin, RepeatMax
    SMART_EVENT_TARGET_BUFFED            = 24,      // Param1 = SpellID, Param2 = Stack amount, Param3/4 RepeatMin, RepeatMax
    SMART_EVENT_RESET                    = 25,      // Called after combat, when the creature respawn and spawn.
    SMART_EVENT_IC_LOS                   = 26,      // NoHostile, MaxRnage, CooldownMin, CooldownMax
    SMART_EVENT_PASSENGER_BOARDED        = 27,      // CooldownMin, CooldownMax
    SMART_EVENT_PASSENGER_REMOVED        = 28,      // CooldownMin, CooldownMax
    SMART_EVENT_CHARMED                  = 29,      // NONE
    SMART_EVENT_CHARMED_TARGET           = 30,      // NONE
    SMART_EVENT_SPELLHIT_TARGET          = 31,      // SpellID, School, CooldownMin, CooldownMax
    SMART_EVENT_DAMAGED                  = 32,      // MinDmg, MaxDmg, CooldownMin, CooldownMax
    SMART_EVENT_DAMAGED_TARGET           = 33,      // MinDmg, MaxDmg, CooldownMin, CooldownMax
    SMART_EVENT_MOVEMENTINFORM           = 34,      // MovementType(any), PointID
    SMART_EVENT_SUMMON_DESPAWNED         = 35,      // Entry, CooldownMin, CooldownMax
    SMART_EVENT_CORPSE_REMOVED           = 36,      // NONE
    SMART_EVENT_AI_INIT                  = 37,      // NONE
    SMART_EVENT_DATA_SET                 = 38,      // Id, Value, CooldownMin, CooldownMax
    SMART_EVENT_WAYPOINT_START           = 39,      // PointId(0any), pathID(0any)
    SMART_EVENT_WAYPOINT_REACHED         = 40,      // PointId(0any), pathID(0any)
    SMART_EVENT_TRANSPORT_ADDPLAYER      = 41,      // NONE
    SMART_EVENT_TRANSPORT_ADDCREATURE    = 42,      // Entry (0 any)
    SMART_EVENT_TRANSPORT_REMOVE_PLAYER  = 43,      // NONE
    SMART_EVENT_TRANSPORT_RELOCATE       = 44,      // PointId
    SMART_EVENT_INSTANCE_PLAYER_ENTER    = 45,      // Team (0 any), CooldownMin, CooldownMax
    SMART_EVENT_AREATRIGGER_ONTRIGGER    = 46,      // TriggerId(0 any)
    SMART_EVENT_QUEST_ACCEPTED           = 47,      // none
    SMART_EVENT_QUEST_OBJ_COPLETETION    = 48,      // none
    SMART_EVENT_QUEST_COMPLETION         = 49,      // none
    SMART_EVENT_QUEST_REWARDED           = 50,      // none
    SMART_EVENT_QUEST_FAIL               = 51,      // none
    SMART_EVENT_TEXT_OVER                = 52,      // GroupId from creature_text,  creature entry who talks (0 any)
    SMART_EVENT_RECEIVE_HEAL             = 53,      // MinHeal, MaxHeal, CooldownMin, CooldownMax
    SMART_EVENT_JUST_SUMMONED            = 54,      // none
    SMART_EVENT_WAYPOINT_PAUSED          = 55,      // PointId(0any), pathID(0any)
    SMART_EVENT_WAYPOINT_RESUMED         = 56,      // PointId(0any), pathID(0any)
    SMART_EVENT_WAYPOINT_STOPPED         = 57,      // PointId(0any), pathID(0any)
    SMART_EVENT_WAYPOINT_ENDED           = 58,      // PointId(0any), pathID(0any)
    SMART_EVENT_TIMED_EVENT_TRIGGERED    = 59,      // id
    SMART_EVENT_UPDATE                   = 60,      // InitialMin, InitialMax, RepeatMin, RepeatMax
    SMART_EVENT_LINK                     = 61,      // INTERNAL USAGE, no params, used to link together multiple events, does not use any extra resources to iterate event lists needlessly
    SMART_EVENT_GOSSIP_SELECT            = 62,      // menuID, actionID
    SMART_EVENT_JUST_CREATED             = 63,      // none
    SMART_EVENT_GOSSIP_HELLO             = 64,      // none
    SMART_EVENT_FOLLOW_COMPLETED         = 65,      // none
    SMART_EVENT_DUMMY_EFFECT             = 66,      // spellId, effectIndex
    SMART_EVENT_IS_BEHIND_TARGET         = 67,      // cooldownMin, CooldownMax
    SMART_EVENT_GAME_EVENT_START         = 68,      // game_event.Entry
    SMART_EVENT_GAME_EVENT_END           = 69,      // game_event.Entry
    SMART_EVENT_GO_STATE_CHANGED         = 70,      // go state
    SMART_EVENT_GO_EVENT_INFORM          = 71,      // eventId
    SMART_EVENT_ACTION_DONE              = 72,      // eventId (SharedDefines.EventId)
    SMART_EVENT_ON_SPELLCLICK            = 73,      // clicker (unit)
    SMART_EVENT_FRIENDLY_HEALTH_PCT      = 74      // minHpPct, maxHpPct, repeatMin, repeatMax
}

namespace SAI_Comment_Converter
{
    class Program
    {
        private static Dictionary<SmartEvent, string> smartEventStrings = new Dictionary<SmartEvent, string>();

        static void Main(string[] args)
        {
            smartEventStrings.Add(SmartEvent.SMART_EVENT_SPELLHIT, "On Spellhit By _spellName_");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_HAS_AURA, "On Has Aura _spellName_");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_TARGET_BUFFED, "On Target Buffed With _spellName_");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_SPELLHIT_TARGET, "On Target Spellhit By _spellName_");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_FRIENDLY_MISSING_BUFF, "On Friendly Unit Missing Buff _spellName_");

            smartEventStrings.Add(SmartEvent.SMART_EVENT_LINK, "_previousLineComment_");

            smartEventStrings.Add(SmartEvent.SMART_EVENT_HEALT_PCT, "Between _eventParamOne_-_eventParamTwo_% Health");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_MANA_PCT, "Between _eventParamOne_-_eventParamTwo_% Mana");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_RANGE, "Within _eventParamOne_-_eventParamTwo_ Range");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_OOC_LOS, "Within _eventParamOne_-_eventParamTwo_ Range Out of Combat LoS");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_TARGET_HEALTH_PCT, "Target Between _eventParamOne_-_eventParamTwo_% Health");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_FRIENDLY_HEALTH, "Friendly At _eventParamOne_ Health");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_TARGET_MANA_PCT, "Target Between _eventParamOne_-_eventParamTwo_% Mana");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_RECEIVE_EMOTE, "Received Emote _eventParamOne_");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_DAMAGED, "On Damaged Between _eventParamOne_-_eventParamTwo_");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_DAMAGED_TARGET, "On Target Damaged Between _eventParamOne_-_eventParamTwo_");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_MOVEMENTINFORM, "On Reached Point _eventParamTwo_");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_SUMMON_DESPAWNED, "On Summon _npcNameFirstParam_ Despawned");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_DATA_SET, "On Data Set _eventParamOne_ _eventParamTwo_");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_WAYPOINT_REACHED, "On Waypoint _eventParamOne_ Reached");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_TEXT_OVER, "On Text _eventParamOne_ Finished");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_RECEIVE_HEAL, "On Received Heal Between _eventParamOne_-_eventParamTwo_");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_TIMED_EVENT_TRIGGERED, "On Timed Event _eventParamOne_ Triggered");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_GOSSIP_SELECT, "On Gossip _eventParamOne_ _paramTwo Selected");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_GAME_EVENT_START, "On Game Event _eventParamOne_ Started");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_GAME_EVENT_END, "On Game Event _eventParamOne_ Ended");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_GO_EVENT_INFORM, "On Event _eventParamOne_ Inform");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_ACTION_DONE, "On Action _eventParamOne_ Done");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_FRIENDLY_HEALTH_PCT, "On Friendly Between _eventParamOne_-_eventParamTwo_% Health");

            smartEventStrings.Add(SmartEvent.SMART_EVENT_UPDATE_IC, "In Combat");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_UPDATE_OOC, "Out of Combat");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_AGGRO, "On Aggro");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_KILL, "On Killed Unit");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_DEATH, "On Just Died");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_EVADE, "On Evade");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_RESPAWN, "On Respawn");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_TARGET_CASTING, "Target Casting");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_FRIENDLY_IS_CC, "Friendly Crowd Controlled");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_SUMMONED_UNIT, "On Summoned Unit");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_ACCEPTED_QUEST, "On Quest Taken");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_REWARD_QUEST, "On Quest Finished");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_REACHED_HOME, "On Reached Home");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_RESET, "On Reset");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_IC_LOS, "In Combat LoS");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_PASSENGER_BOARDED, "On Passenger Boarded");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_PASSENGER_REMOVED, "On Passenger Removed");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_CHARMED, "On Charmed");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_CHARMED_TARGET, "On Target Charmed");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_CORPSE_REMOVED, "On Corpse Removed");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_AI_INIT, "On Initialize");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_WAYPOINT_START, "On Waypoint Started");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_TRANSPORT_ADDPLAYER, "");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_TRANSPORT_ADDCREATURE, "");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_TRANSPORT_REMOVE_PLAYER, "");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_TRANSPORT_RELOCATE, "");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_INSTANCE_PLAYER_ENTER, "");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_AREATRIGGER_ONTRIGGER, "");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_QUEST_ACCEPTED, "");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_QUEST_OBJ_COPLETETION, "");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_QUEST_COMPLETION, "");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_QUEST_REWARDED, "");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_QUEST_FAIL, "");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_JUST_SUMMONED, "On Just Summoned");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_WAYPOINT_PAUSED, "On Waypoint Paused");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_WAYPOINT_RESUMED, "On Waypoint Resumed");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_WAYPOINT_STOPPED, "On Waypoint Stopped");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_WAYPOINT_ENDED, "On Waypoint Finished");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_UPDATE, "On Update");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_JUST_CREATED, "On Just Created");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_GOSSIP_HELLO, "On Gossip Hello");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_FOLLOW_COMPLETED, "On Follow Complete");
            //smartEventStrings.Add(SmartEvent.SMART_EVENT_DUMMY_EFFECT, "");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_IS_BEHIND_TARGET, "On Behind Target");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_GO_STATE_CHANGED, "On Gameobject State Changed");
            smartEventStrings.Add(SmartEvent.SMART_EVENT_ON_SPELLCLICK, "On Spellclick");

            while (true)
            {
                Console.WriteLine("SQL Information:");
                //Console.Write("Host: ");
                //string host = Console.ReadLine();
                //Console.Write("User: ");
                //string user = Console.ReadLine();
                //Console.Write("Pass: ");
                //string pass = Console.ReadLine();
                //Console.Write("World DB: ");
                //string worldDB = Console.ReadLine();
                //Console.Write("Port: ");
                //string port = Console.ReadLine();

                string host = "127.0.0.1";
                string user = "root";
                string pass = "123";
                string worldDB = "trinitycore_world";
                string port = "3306";

                //Console.WriteLine(host);
                //Console.WriteLine(user);
                //Console.WriteLine(pass);
                //Console.WriteLine(worldDB);
                //Console.WriteLine(port);

                MySqlConnectionStringBuilder connectionString = new MySqlConnectionStringBuilder();
                connectionString.UserID = user;
                connectionString.Password = pass;
                connectionString.Server = host;
                connectionString.Database = worldDB;
                connectionString.Port = Convert.ToUInt32(port);

                using (var connection = new MySqlConnection(connectionString.ToString()))
                {
                    connection.Open();
                    var returnVal = new MySqlDataAdapter(String.Format("SELECT * FROM smart_scripts ORDER BY entryorguid"), connection);
                    var dataTable = new DataTable();
                    returnVal.Fill(dataTable);

                    if (dataTable.Rows.Count <= 0)
                        break;

                    using (var outputFile = new StreamWriter("sai_update.sql", true))
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            MySqlCommand command = new MySqlCommand();
                            command.Connection = connection;

                            string fullLine = "UPDATE `smart_scripts` SET `comment`='";
                            int entryorguid = Convert.ToInt32(row.ItemArray[0].ToString());
                            int entry = entryorguid;

                            if (Convert.ToInt32(row.ItemArray[0].ToString()) < 0)
                            {
                                command.CommandText = (String.Format("SELECT id FROM creature WHERE guid={0}", -entryorguid));
                                MySqlDataReader readerCreatureId = command.ExecuteReader(CommandBehavior.Default);

                                if (readerCreatureId.Read())
                                    entry = Convert.ToInt32(readerCreatureId[0].ToString());

                                readerCreatureId.Close();
                            }

                            command.CommandText = (String.Format("SELECT name FROM creature_template WHERE entry={0}", entry));
                            MySqlDataReader readerCreatureName = command.ExecuteReader(CommandBehavior.Default);

                            if (readerCreatureName.Read())
                                fullLine = readerCreatureName[0].ToString() + " - ";

                            readerCreatureName.Close();
                            Console.WriteLine(fullLine);
                            Console.ReadKey();

                            //! Event type
                            fullLine += smartEventStrings[(SmartEvent)Convert.ToInt32(row.ItemArray[4].ToString())];

                            //! TODO: Figure out how to make this work with linking several lines TO each other
                            if (fullLine.Contains("_previousLineComment_"))
                            {
                                MySqlCommand commandPreviousComment = new MySqlCommand(String.Format("SELECT event_type FROM smart_scripts WHERE entryorguid={0} AND id={1}", entryorguid, (Convert.ToInt32(row.ItemArray[2]) - 1).ToString()), connection);
                                MySqlDataReader readerPreviousLineComment = commandPreviousComment.ExecuteReader(CommandBehavior.Default);

                                if (readerPreviousLineComment.Read())
                                    fullLine = fullLine.Replace("_previousLineComment_", smartEventStrings[(SmartEvent)Convert.ToInt32(readerPreviousLineComment[0].ToString())]);
                                else
                                    fullLine = fullLine.Replace("_previousLineComment_", "Link not found!");

                                readerPreviousLineComment.Close();
                            }

                            //! This must be called AFTER we check for _previousLineComment_ so that copied event types don't need special handling
                            if (fullLine.Contains("_eventParamOne_"))
                                fullLine = fullLine.Replace("_eventParamOne_", row.ItemArray[8].ToString());

                            if (fullLine.Contains("_eventParamTwo_"))
                                fullLine = fullLine.Replace("_eventParamTwo_", row.ItemArray[9].ToString());

                            fullLine += " - ";
                            Console.WriteLine(fullLine);
                        }
                    }
                }
            }
        }
    }
}
